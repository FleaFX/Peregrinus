using System.Threading.Tasks;

namespace Peregrinus.Model; 

/// <summary>
/// Null-pattern implementation of <see cref="IMigrationContext"/>.
/// </summary>
public class NullMigrationContext : IMigrationContext {
    /// <summary>
    /// Prepares the given <paramref name="query"/> to run as a <see cref="AsyncMigrationOperation"/> within the current migration context.
    /// </summary>
    /// <param name="query">The migration query to run.</param>
    /// <returns>A <see cref="AsyncMigrationOperation"/>.</returns>
    AsyncMigrationOperation IMigrationContext.PrepareMigration(string query) {
        return () => Task.FromResult(0);
    }

    /// <summary>
    /// Writes a history record for the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="migrationHistoryRecord">The <see cref="MigrationHistoryRecord"/> to write.</param>
    Task IMigrationContext.WriteHistoryRecord(MigrationHistoryRecord migrationHistoryRecord) => Task.CompletedTask;

    /// <summary>
    /// Removes a history record from the history table in case of a rollback.
    /// </summary>
    /// <param name="migrationHistoryRecord">The migration history record to remove.</param>
    Task IMigrationContext.RemoveHistoryRecord(MigrationHistoryRecord migrationHistoryRecord) => Task.CompletedTask;

    /// <summary>
    /// Loads the current <see cref="MigrationHistory"/> from the history table in the target database.
    /// </summary>
    /// <param name="migrationsBatch">A batch of migration and rollback scripts that must match the applied migrations in the database.</param>
    /// <returns>A <see cref="IMigrationHistory"/>.</returns>
    Task<IMigrationHistory> IMigrationContext.LoadHistory(MigrationsBatch migrationsBatch) => Task.FromResult<IMigrationHistory>(new MigrationHistory(this));
}