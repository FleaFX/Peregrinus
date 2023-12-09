using System.Threading.Tasks;

namespace Peregrinus.Model; 

public interface IMigrationContext {
    /// <summary>
    /// Prepares the given <paramref name="query"/> to run as a <see cref="AsyncMigrationOperation"/> within the current migration context.
    /// </summary>
    /// <param name="query">The migration query to run.</param>
    /// <returns>A <see cref="AsyncMigrationOperation"/>.</returns>
    AsyncMigrationOperation PrepareMigration(string query);

    /// <summary>
    /// Writes a history record for the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="migrationHistoryRecord">The <see cref="MigrationHistoryRecord"/> to write.</param>
    Task WriteHistoryRecord(MigrationHistoryRecord migrationHistoryRecord);

    /// <summary>
    /// Removes a history record from the history table in case of a rollback.
    /// </summary>
    /// <param name="migrationHistoryRecord">The migration history record to remove.</param>
    Task RemoveHistoryRecord(MigrationHistoryRecord migrationHistoryRecord);

    /// <summary>
    /// Loads the current <see cref="MigrationHistory"/> from the history table in the target database.
    /// </summary>
    /// <param name="migrationsBatch">A batch of migration and rollback scripts that must match the applied migrations in the database.</param>
    /// <returns>A <see cref="IMigrationHistory"/>.</returns>
    Task<IMigrationHistory> LoadHistory(MigrationsBatch migrationsBatch);
}