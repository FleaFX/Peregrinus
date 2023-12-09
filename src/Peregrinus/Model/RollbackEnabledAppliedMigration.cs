using System;
using System.Threading.Tasks;
using Peregrinus.Resources;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Represents a <see cref="Migration"/> that has been applied to the database and can be rolled back.
/// </summary>
public class RollbackEnabledAppliedMigration : AppliedMigration {
    readonly RollbackMigration _rollbackMigration;

    /// <summary>
    /// Initializes a new <see cref="RollbackEnabledApplicableMigration"/>.
    /// </summary>
    /// <param name="version">The version of the migration.</param>
    /// <param name="description">The description of the migration.</param>
    /// <param name="checksum">The checksum of the migration.</param>
    /// <param name="rollbackMigration">The rollback script.</param>
    /// <param name="executionTime">The time it took to apply the migration.</param>
    public RollbackEnabledAppliedMigration(SemVersion version, Description description, Checksum checksum, RollbackMigration rollbackMigration, TimeSpan? executionTime = null) : base(version, description, checksum, executionTime) {
        if (rollbackMigration == null) throw new ArgumentNullException(nameof(rollbackMigration));
        if (!rollbackMigration.AppliesTo(version, description)) throw new ArgumentOutOfRangeException(RollbackEnabledMigrationTranslations.RollbackMigrationMismatchExceptionMessage, nameof(rollbackMigration));
        _rollbackMigration = rollbackMigration;
    }

    /// <summary>
    /// Rolls back this migration.
    /// </summary>
    /// <param name="migrationContext"></param>
    internal async Task Rollback(IMigrationContext migrationContext) {
        await _rollbackMigration.Rollback(migrationContext);
        await migrationContext.RemoveHistoryRecord(new MigrationHistoryRecord(_version.ToString(), (string)_description, (byte[])_checksum, null));
    }
}