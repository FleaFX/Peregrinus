using System;

namespace Peregrinus.Model.Exceptions; 

/// <summary>
/// Thrown when applying a migration for a prerelease version that has no rollback migration.
/// </summary>
/// <remarks>Prerelease versions are considered to be unstable. When applying a stable version, any prerelease migration
/// should be rolled back before making the permanent changes. If there is no rollback script available for a prerelease
/// migration, it can never be rolled back and the database would be stuck in a state where it can be migrated forward
/// nor backward.</remarks>
public class PermanentPrereleaseMigrationException : Exception {
    /// <summary>
    /// Gets the <see cref="ApplicableMigration"/> that was tried to be applied.
    /// </summary>
    public ApplicableMigration ApplicableMigration { get; }

    /// <summary>
    /// Initializes a new <see cref="PermanentPrereleaseMigrationException"/>.
    /// </summary>
    /// <param name="applicableMigration">The <see cref="ApplicableMigration"/> that was tried to be applied.</param>
    public PermanentPrereleaseMigrationException(ApplicableMigration applicableMigration) {
        ApplicableMigration = applicableMigration ?? throw new ArgumentNullException(nameof(applicableMigration));
    }
}