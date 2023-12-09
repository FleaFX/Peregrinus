using System;
using System.Threading.Tasks;
using Peregrinus.Resources;
using Peregrinus.Util;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Represents a <see cref="Migration"/> that has not yet been applied to the database and can be rolled back once it has been applied.
/// </summary>
public class RollbackEnabledApplicableMigration : ApplicableMigration {
    readonly RollbackMigration _rollbackMigration;

    /// <summary>
    /// Initializes a new <see cref="RollbackEnabledApplicableMigration"/>.
    /// </summary>
    /// <param name="version">The version of the migration.</param>
    /// <param name="description">The description of the migration.</param>
    /// <param name="content">The content of the migration.</param>
    /// <param name="rollbackMigration">THe rollback script for this migration.</param>
    public RollbackEnabledApplicableMigration(SemVersion version, Description description, MigrationScriptContent content, RollbackMigration rollbackMigration) : base(version, description, content) {
        if (rollbackMigration == null) throw new ArgumentNullException(nameof(rollbackMigration));
        if (!rollbackMigration.AppliesTo(version, description)) throw new ArgumentOutOfRangeException(RollbackEnabledMigrationTranslations.RollbackMigrationMismatchExceptionMessage, nameof(rollbackMigration));
        _rollbackMigration = rollbackMigration;
    }

    /// <summary>
    /// Applies the migration and returns a <see cref="AppliedMigration"/> from the current instance.
    /// </summary>
    /// <param name="migrationContext">The <see cref="MigrationContext"/> that run the migration in.</param>
    /// <returns>A <see cref="AppliedMigration"/>.</returns>
    internal override async Task<AppliedMigration> Apply(IMigrationContext migrationContext) {
        var (executionTime, _) = await Diagnostics.TimeOperationAsync(() => _content.PrepareMigration(migrationContext)());
        return new RollbackEnabledAppliedMigration(_version, _description, _checksum, _rollbackMigration, executionTime);
    }

    /// <summary>
    /// Indicates whether the migration can run within a transaction.
    /// </summary>
    public override bool CanRunInTransaction => _rollbackMigration.CanRunInTransaction;

    protected bool Equals(RollbackEnabledApplicableMigration other) {
        return base.Equals(other) && Equals(_rollbackMigration, other._rollbackMigration);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RollbackEnabledApplicableMigration) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            return (base.GetHashCode() * 397) ^ (_rollbackMigration != null ? _rollbackMigration.GetHashCode() : 0);
        }
    }
}