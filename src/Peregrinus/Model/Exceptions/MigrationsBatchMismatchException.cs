using System;

namespace Peregrinus.Model.Exceptions; 

/// <summary>
/// Thrown when applying a batch of migrations to a database that was originally migrated using a different set of migrations.
/// </summary>
public class MigrationsBatchMismatchException : Exception {
    /// <summary>
    /// Gets the batch that was matched to the applied migrations in the database.
    /// </summary>
    public MigrationsBatch MigrationsBatch { get; }

    /// <summary>
    /// Gets the applied migration -already present in the migration history- that caused the mismatch to the batch.
    /// </summary>
    public AppliedMigration UnmatchedMigration { get; }

    /// <summary>
    /// Initializes a new <see cref="MigrationsBatchMismatchException"/>.
    /// </summary>
    /// <param name="migrationsBatch">The batch that was matched to the applied migrations in the database.</param>
    /// <param name="unmatchedMigration">The applied migration -already present in the migration history- that caused the mismatch to the batch.</param>
    public MigrationsBatchMismatchException(MigrationsBatch migrationsBatch, AppliedMigration unmatchedMigration) {
        MigrationsBatch = migrationsBatch ?? throw new ArgumentNullException(nameof(migrationsBatch));
        UnmatchedMigration = unmatchedMigration ?? throw new ArgumentNullException(nameof(unmatchedMigration));
    }
}