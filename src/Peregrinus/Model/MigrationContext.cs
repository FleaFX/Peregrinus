using System;
using System.Linq;
using System.Threading.Tasks;
using Peregrinus.Database;
using Peregrinus.Util;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Holds general configuration information for migrations to run.
/// </summary>
public class MigrationContext : IMigrationContext {
    readonly IQueryExecutor _queryExecutor;
    readonly IMigrationExecutor _migrationExecutor;
    readonly string _targetDatabaseName;
    readonly string _migrationHistoryTableName;
    readonly string[] _managedSchemas;

    /// <summary>
    /// Initializes a new <see cref="MigrationContext"/>.
    /// </summary>
    /// <param name="queryExecutor">The <see cref="IQueryExecutor"/> to use.</param>
    /// <param name="migrationExecutor">The <see cref="IMigrationExecutor"/> to use.</param>
    /// <param name="targetDatabaseName">The name of the target database.</param>
    /// <param name="migrationHistoryTableName">The name fo the migration history table. Defaults to "migration_history".</param>
    /// <param name="managedSchemas">A collection of schema names that are managed. The first schema in this collection is where the migration history table will live.</param>
    public MigrationContext(IQueryExecutor queryExecutor, IMigrationExecutor migrationExecutor, string targetDatabaseName, string migrationHistoryTableName = "migration_history", params string[] managedSchemas) {
        _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        _migrationExecutor = migrationExecutor ?? throw new ArgumentNullException(nameof(migrationExecutor));
        _targetDatabaseName = targetDatabaseName ?? throw new ArgumentNullException(nameof(targetDatabaseName));
        _migrationHistoryTableName = migrationHistoryTableName ?? "migration_history";
        _managedSchemas = managedSchemas ?? new [] { "dbo" };
    }

    void ProvisionDatabase() {
        var provisionDatabase = $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE [name] = '{_targetDatabaseName}')
BEGIN
	CREATE DATABASE [{_targetDatabaseName}];
END";
        _queryExecutor.NewQuery(provisionDatabase).Execute();
    }

    void ProvisionManagedSchemas() {
        foreach (var schema in _managedSchemas) {
            var provisionSchema = $@"
IF NOT EXISTS (SELECT * FROM [{_targetDatabaseName}].sys.schemas WHERE [name] = '{schema}')
BEGIN
	DECLARE @sql NVARCHAR(MAX)
	SET @sql = 'CREATE SCHEMA [{schema}];'
	EXEC [{_targetDatabaseName}].dbo.sp_executesql @sql
END";
            _queryExecutor.NewQuery(provisionSchema).Execute();
        }
    }

    void ProvisionHistoryTable() {
        var provisionHistoryTable = $@"
IF NOT EXISTS (SELECT * FROM [{_targetDatabaseName}].sys.tables WHERE [name] = '{_migrationHistoryTableName}')
BEGIN
  CREATE TABLE [{_targetDatabaseName}].[{_managedSchemas?.FirstOrDefault() ?? "dbo"}].[{_migrationHistoryTableName}] (
    [Id] uniqueidentifier ROWGUIDCOL NOT NULL PRIMARY KEY CLUSTERED CONSTRAINT DF_{_migrationHistoryTableName.ToUpper()}_ID DEFAULT (NEWID()),
    [TimeStamp] bigint NOT NULL,
    [Version] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NOT NULL,
    [Checksum] binary(20) NOT NULL,
    [ExecutionTime] bigint NULL
  );
END";
        _queryExecutor.NewQuery(provisionHistoryTable).Execute();
    }

    /// <summary>
    /// Provisions the database, managed schemas and the migration history table, if necessary.
    /// </summary>
    public void Provision() {
        ProvisionDatabase();
        ProvisionManagedSchemas();
        ProvisionHistoryTable();
    }

    /// <summary>
    /// Loads the current <see cref="MigrationHistory"/> from the history table in the target database.
    /// </summary>
    /// <remarks>If the target database and/or history table does not exist yet, it will be provisioned.</remarks>
    /// <param name="migrationsBatch">A batch of migration and rollback scripts that must match the applied migrations in the database.</param>
    /// <returns>A <see cref="IMigrationHistory"/>.</returns>
    public async Task<IMigrationHistory> LoadHistory(MigrationsBatch migrationsBatch) {
        var loadSql = $@"
SELECT
  [Id]
, [TimeStamp]
, [Version]
, [Description]
, [Checksum]
, [ExecutionTime]
FROM [{_targetDatabaseName}].[{_managedSchemas?.FirstOrDefault() ?? "dbo"}].[{_migrationHistoryTableName}]
ORDER BY [TimeStamp] ASC";
        var appliedMigrations = await (
            await _queryExecutor
                .NewQuery(loadSql)
                .ExecuteAsync<MigrationHistoryRecord>()
        ).SelectAsync(record =>
            migrationsBatch.Match(
                new AppliedMigration(
                    SemVersion.Parse(record.Version, SemVersionStyles.OptionalPatch),
                    new Description(record.Description),
                    new Checksum(record.Checksum),
                    record.ExecutionTimeTicks.HasValue ? TimeSpan.FromTicks(record.ExecutionTimeTicks.Value) : (TimeSpan?)null
                )
            )
        );

        return new TransactionalMigrationHistory(new MigrationHistory(this, appliedMigrations.ToArray()));
    }

    /// <summary>
    /// Prepares the given <paramref name="query"/> to run as a <see cref="AsyncMigrationOperation"/> within the current migration context.
    /// </summary>
    /// <param name="query">The migration query to run.</param>
    /// <returns>A <see cref="AsyncMigrationOperation"/>.</returns>
    AsyncMigrationOperation IMigrationContext.PrepareMigration(string query) =>
        _migrationExecutor.NewMigration(query).ExecuteAsync;

    /// <summary>
    /// Writes a history record for the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="migrationHistoryRecord">The <see cref="MigrationHistoryRecord"/> to write.</param>
    async Task IMigrationContext.WriteHistoryRecord(MigrationHistoryRecord migrationHistoryRecord) {
        var sql = $@"
INSERT INTO [{_targetDatabaseName}].[{_managedSchemas?.FirstOrDefault() ?? "dbo"}].[{_migrationHistoryTableName}] ([Id], [TimeStamp], [Version], [Description], [Checksum], [ExecutionTime])
VALUES (NEWID(), @TimeStampTicks, @Version, @Description, @Checksum, @ExecutionTimeTicks)
";
        await _queryExecutor
            .NewQuery(sql)
            .WithParameters(migrationHistoryRecord)
            .ExecuteAsync();
    }

    /// <summary>
    /// Removes a history record from the history table in case of a rollback.
    /// </summary>
    /// <param name="migrationHistoryRecord">The migration history record to remove.</param>
    async Task IMigrationContext.RemoveHistoryRecord(MigrationHistoryRecord migrationHistoryRecord) {
        var sql = $@"
DELETE FROM [{_targetDatabaseName}].[{_managedSchemas?.FirstOrDefault() ?? "dbo"}].[{_migrationHistoryTableName}]
WHERE [Version] = @Version AND [Description] = @Description
";
        await _queryExecutor
            .NewQuery(sql)
            .WithParameters(migrationHistoryRecord)
            .ExecuteAsync();
    }
}