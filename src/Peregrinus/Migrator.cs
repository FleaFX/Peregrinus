using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Peregrinus.Database;
using Peregrinus.Model;
using Peregrinus.Resources;

namespace Peregrinus; 

public class Migrator {
    readonly string _connectionString;
    readonly string _targetDatabase;
    readonly IMigrationScriptProvider _migrationScriptProvider;
    readonly string[] _managedSchemas;

    /// <summary>
    /// Initializes a new <see cref="Migrator"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to the database server where the migrations should run.</param>
    /// <param name="targetDatabase">The name of the target database to migrate.</param>
    /// <param name="migrationScriptProvider">A <see cref="IMigrationScriptProvider"/> that provides the list of migrations to run.</param>
    /// <param name="managedSchemas">A sequence of schemas that is maintained by the migrator.</param>
    public Migrator(string connectionString, string targetDatabase, IMigrationScriptProvider migrationScriptProvider, params string[] managedSchemas) {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _targetDatabase = targetDatabase ?? throw new ArgumentNullException(nameof(targetDatabase));
        _migrationScriptProvider = migrationScriptProvider ?? throw new ArgumentNullException(nameof(migrationScriptProvider));
        _managedSchemas = managedSchemas.DefaultIfEmpty("meta").ToArray();
    }

    /// <summary>
    /// Migrates the database to the latest version.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    public async Task Migrate() {
        // 1. provision the database, schemas and history table
        CreateProvisioningContext(_targetDatabase).Provision();

        // 2. collect all migration files
        var batch = await ReadMigrations(MigrationsBatch.Empty);

        // 3. load existing history
        var migrationHistory = await CreateMigrationContext(_targetDatabase).LoadHistory(batch);

        // 4. apply pending migrations
        await batch.ApplyTo(migrationHistory, result => {
            switch (result) {
                case AppliedMigrationSkippedResult skipped:
                    Trace.WriteLine(string.Format(MigratorTranslations.AppliedMigrationSkipped, skipped.ApplicableMigration));
                    break;
                case ApplicableMigrationSucceededResult success:
                    Trace.WriteLine(string.Format(MigratorTranslations.ApplicableMigrationSucceeded, success.ApplicableMigration));
                    break;
                case ApplicableMigrationSucceededWithRollbackResult successWithRollback:
                    Trace.WriteLine(string.Format(MigratorTranslations.ApplicableMigrationSucceededWithRollback, successWithRollback.ApplicableMigration, successWithRollback.RolledBackMigration));
                    break;
                case MigrationVersionAnachronismResult anachronism:
                    throw new MigrationException(MigrationFailureReason.MigrationVersionAnachronism, anachronism.ApplicableMigration);
                case ApplicableMigrationFailedResult failure:
                    throw new MigrationException(MigrationFailureReason.ApplicableMigrationFailed, failure.Migration, failure.Exception);
            }
        });
    }

    /// <summary>
    /// Rolls back the database using the given <paramref name="rollbackStrategy"/>.
    /// </summary>
    /// <param name="rollbackStrategy">The <see cref="RollbackStrategy"/> that governs the rollback behaviour.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    public async Task Rollback(RollbackStrategy rollbackStrategy) {
        // 1. provision the database, schemas and history table
        CreateProvisioningContext(_targetDatabase).Provision();

        // 2. collect all migration files
        var batch = await ReadMigrations(MigrationsBatch.Empty);

        // 3. load existing history
        var migrationHistory = await CreateMigrationContext(_targetDatabase).LoadHistory(batch);

        // 4. perform the rollback
        await rollbackStrategy.Rollback(migrationHistory);
    }

    MigrationContext CreateProvisioningContext(string targetDatabase) {
        var migrationDatabase = new MigrationTargetDatabase(_connectionString);
        var master = migrationDatabase.Target("master");
        return new MigrationContext(new QueryExecutor(master), new MigrationExecutor(migrationDatabase), targetDatabase, "migration_history", _managedSchemas);
    }

    MigrationContext CreateMigrationContext(string targetDatabase) {
        var migrationDatabase = new MigrationTargetDatabase(_connectionString).Target(targetDatabase);
        return new MigrationContext(new QueryExecutor(migrationDatabase), new MigrationExecutor(migrationDatabase), targetDatabase,"migration_history", _managedSchemas);
    }

    async Task<MigrationsBatch> ReadMigrations(MigrationsBatch batch) {
        var migrations = await _migrationScriptProvider.LoadMigrationScripts();

        foreach (var (migration, stream) in migrations) {
            batch += MigrationsBatch.FromStream(migration, stream);
        }
        return batch;
    }
}