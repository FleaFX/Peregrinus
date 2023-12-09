using System;
using System.Threading.Tasks;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Represents a <see cref="Migration"/> that has already been applied. An applied migration doesn't have a content script associated with it anymore.
/// </summary>
public class AppliedMigration : Migration {
    readonly TimeSpan? _executionTime;

    /// <summary>
    /// Initializes a new <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="version">The version of the migration.</param>
    /// <param name="description">The description of the migration.</param>
    /// <param name="checksum">The checksum of the migration.</param>
    /// <param name="executionTime">The time it took to perform the migration.</param>
    public AppliedMigration(SemVersion version, Description description, Checksum checksum, TimeSpan? executionTime = null) : base(version, description, checksum) {
        _executionTime = executionTime;
    }

    /// <summary>
    /// Writes a record to the migration history table for the current applied migration.
    /// </summary>
    /// <param name="migrationContext">The <see cref="MigrationContext"/> to use.</param>
    /// <returns>The current <see cref="AppliedMigration"/> instance.</returns>
    internal async Task<AppliedMigration> Historicize(IMigrationContext migrationContext) {
        await migrationContext.WriteHistoryRecord(new MigrationHistoryRecord(_version.ToString(), (string)_description, (byte[])_checksum, _executionTime?.Ticks));
        return this;
    }

    /// <summary>
    /// Indicates whether this migration is a prerelease migration.
    /// </summary>
    public bool IsPrerelease => !string.IsNullOrWhiteSpace(_version.Prerelease);

    /// <summary>
    /// Adds rollback capability to the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="appliedMigration">The applied migration.</param>
    /// <param name="rollbackMigration">The rollback migration.</param>
    /// <returns>A <see cref="RollbackEnabledApplicableMigration"/>.</returns>
    public static RollbackEnabledAppliedMigration operator +(AppliedMigration appliedMigration, RollbackMigration rollbackMigration) =>
        new RollbackEnabledAppliedMigration(appliedMigration._version, appliedMigration._description, appliedMigration._checksum, rollbackMigration, appliedMigration._executionTime);

    #region Application result builder methods

    /// <summary>
    /// Adds the version of the current <see cref="AppliedMigration"/> to the given <see cref="MigrationVersionAnachronismResult"/>.
    /// </summary>
    /// <param name="result">The <see cref="MigrationVersionAnachronismResult"/>.</param>
    /// <returns>A new <see cref="MigrationVersionAnachronismResult"/>.</returns>
    public MigrationVersionAnachronismResult Build(MigrationVersionAnachronismResult result) =>
        result.WithLatestVersion(_version);

    #endregion
}