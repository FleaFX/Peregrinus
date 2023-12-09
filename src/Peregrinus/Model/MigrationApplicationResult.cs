using System;
using Semver;

namespace Peregrinus.Model; 

public abstract class MigrationApplicationResult {
    /// <summary>
    /// Result returned when attempting to apply a migration that is already applied to the database.
    /// </summary>
    public static readonly AppliedMigrationSkippedResult AppliedMigrationSkipped = new AppliedMigrationSkippedResult();

    /// <summary>
    /// Result returned when attempting to apply a migration that has a version earlier than the version of the last applied migration.
    /// </summary>
    public static readonly MigrationVersionAnachronismResult MigrationVersionAnachronism = new MigrationVersionAnachronismResult();

    /// <summary>
    /// Result returned when the migration was successfully applied.
    /// </summary>
    public static readonly ApplicableMigrationSucceededResult ApplicableMigrationSucceeded = new ApplicableMigrationSucceededResult();

    /// <summary>
    /// Result returned when the migration was applicable, but failed during the actual application.
    /// </summary>
    public static readonly ApplicableMigrationFailedResult ApplicableMigrationFailed = new ApplicableMigrationFailedResult();

    /// <summary>
    /// Result returned when the migration was successfully applied, while the last applied migration was rolled back because it was a prerelease version.
    /// </summary>
    public static readonly ApplicableMigrationSucceededWithRollbackResult ApplicableMigrationSucceededWithRollback = new ApplicableMigrationSucceededWithRollbackResult();
}

/// <summary>
/// Result returned when attempting to apply a migration that is already applied to the database.
/// </summary>
public class AppliedMigrationSkippedResult : MigrationApplicationResult {
    /// <summary>
    /// Gets the migration that was to be applied.
    /// </summary>
    public ApplicableMigration ApplicableMigration { get; }

    /// <summary>
    /// Gets the migration that was found to be already applied.
    /// </summary>
    public AppliedMigration AppliedMigration { get; }

    public AppliedMigrationSkippedResult() {}

    /// <summary>
    /// Initializes a new <see cref="AppliedMigrationSkippedResult"/>.
    /// </summary>
    /// <param name="applicableMigration">The migration that was to be applied.</param>
    /// <param name="appliedMigration">The migration that was found to be already applied.</param>
    public AppliedMigrationSkippedResult(ApplicableMigration applicableMigration, AppliedMigration appliedMigration) {
        ApplicableMigration = applicableMigration;
        AppliedMigration = appliedMigration;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="AppliedMigrationSkippedResult"/> with the given <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was to be applied.</param>
    /// <returns>A new <see cref="AppliedMigrationSkippedResult"/>.</returns>
    public AppliedMigrationSkippedResult WithApplicableMigration(ApplicableMigration migration) =>
        new AppliedMigrationSkippedResult(migration, AppliedMigration);

    /// <summary>
    /// Builder method that returns a new <see cref="AppliedMigrationSkippedResult"/> with the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was found to be already applied.</param>
    /// <returns>A new <see cref="AppliedMigrationSkippedResult"/>.</returns>
    public AppliedMigrationSkippedResult WithAppliedMigration(AppliedMigration migration) =>
        new AppliedMigrationSkippedResult(ApplicableMigration, migration);

    bool Equals(AppliedMigrationSkippedResult other) {
        return Equals(ApplicableMigration, other.ApplicableMigration) && Equals(AppliedMigration, other.AppliedMigration);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((AppliedMigrationSkippedResult) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            return ((ApplicableMigration != null ? ApplicableMigration.GetHashCode() : 0) * 397) ^ (AppliedMigration != null ? AppliedMigration.GetHashCode() : 0);
        }
    }
}

/// <summary>
/// Result returned when attempting to apply a migration that has a version earlier than the version of the last applied migration.
/// </summary>
public class MigrationVersionAnachronismResult : MigrationApplicationResult {
    /// <summary>
    /// Gets the migration that was to be applied.
    /// </summary>
    public ApplicableMigration ApplicableMigration { get; }

    /// <summary>
    /// Gets the version of the last applied migration.
    /// </summary>
    public SemVersion LatestVersion { get; }

    /// <summary>
    /// Initializes a new <see cref="MigrationVersionAnachronismResult"/>.
    /// </summary>
    public MigrationVersionAnachronismResult() { }
    
    /// <summary>
    /// Initializes a new <see cref="MigrationVersionAnachronismResult"/>.
    /// </summary>
    /// <param name="applicableMigration">The migration that was to be applied.</param>
    /// <param name="latestVersion">The version of the last applied migration.</param>
    public MigrationVersionAnachronismResult(ApplicableMigration applicableMigration, SemVersion latestVersion) {
        ApplicableMigration = applicableMigration;
        LatestVersion = latestVersion;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="MigrationVersionAnachronismResult"/> with the given <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was to be applied.</param>
    /// <returns>A <see cref="MigrationVersionAnachronismResult"/>.</returns>
    public MigrationVersionAnachronismResult WithApplicableMigration(ApplicableMigration migration) =>
        new MigrationVersionAnachronismResult(migration, LatestVersion);

    /// <summary>
    /// Builder method that returns a new <see cref="MigrationVersionAnachronismResult"/> with the given <see cref="SemVersion"/>.
    /// </summary>
    /// <param name="version">The version of the last applied migration.</param>
    /// <returns>A <see cref="MigrationVersionAnachronismResult"/>.</returns>
    public MigrationVersionAnachronismResult WithLatestVersion(SemVersion version) =>
        new MigrationVersionAnachronismResult(ApplicableMigration, version);

    bool Equals(MigrationVersionAnachronismResult other) {
        return Equals(ApplicableMigration, other.ApplicableMigration) && Equals(LatestVersion, other.LatestVersion);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MigrationVersionAnachronismResult) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            return ((ApplicableMigration != null ? ApplicableMigration.GetHashCode() : 0) * 397) ^ (LatestVersion != null ? LatestVersion.GetHashCode() : 0);
        }
    }
}

/// <summary>
/// Result returned when the migration was successfully applied.
/// </summary>
public class ApplicableMigrationSucceededResult : MigrationApplicationResult {
    /// <summary>
    /// Gets the migration that was to be applied to the migration history.
    /// </summary>
    public ApplicableMigration ApplicableMigration { get; }

    /// <summary>
    /// Gets the migration that was applied to the migration history.
    /// </summary>
    public AppliedMigration AppliedMigration { get; }

    /// <summary>
    /// Gets the migration history as it is after applying the migration.
    /// </summary>
    public IMigrationHistory UpdatedHistory { get; }

    /// <summary>
    /// Initializes a new <see cref="ApplicableMigrationSucceededResult"/>.
    /// </summary>
    public ApplicableMigrationSucceededResult() { }

    /// <summary>
    /// Initializes a new <see cref="ApplicableMigrationSucceededResult"/>.
    /// </summary>
    /// <param name="applicableMigration">The migration that is to be applied.</param>
    /// <param name="appliedMigration">The migration that was applied.</param>
    /// <param name="updatedHistory">The migration history after the migration was applied.</param>
    public ApplicableMigrationSucceededResult(ApplicableMigration applicableMigration, AppliedMigration appliedMigration, IMigrationHistory updatedHistory) {
        ApplicableMigration = applicableMigration;
        AppliedMigration = appliedMigration;
        UpdatedHistory = updatedHistory;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededResult"/> with the given <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was to be applied.</param>
    /// <returns>A <see cref="ApplicableMigrationSucceededResult"/>.</returns>
    public ApplicableMigrationSucceededResult ForMigration(ApplicableMigration migration) => new ApplicableMigrationSucceededResult(migration, AppliedMigration, UpdatedHistory);

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededResult"/> with the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was applied.</param>
    /// <returns></returns>
    public ApplicableMigrationSucceededResult WithAppliedMigration(AppliedMigration migration) => new ApplicableMigrationSucceededResult(ApplicableMigration, migration, UpdatedHistory);

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededResult"/> with the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="migrationHistory">The migration history to which the migration is to be applied.</param>
    /// <returns>A <see cref="ApplicableMigrationSucceededResult"/>.</returns>
    public ApplicableMigrationSucceededResult WithHistory(IMigrationHistory migrationHistory) => new ApplicableMigrationSucceededResult(ApplicableMigration, AppliedMigration, migrationHistory);

    bool Equals(ApplicableMigrationSucceededResult other) {
        return Equals(ApplicableMigration, other.ApplicableMigration) && Equals(AppliedMigration, other.AppliedMigration) && Equals(UpdatedHistory, other.UpdatedHistory);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ApplicableMigrationSucceededResult) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            var hashCode = (ApplicableMigration != null ? ApplicableMigration.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (AppliedMigration != null ? AppliedMigration.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (UpdatedHistory != null ? UpdatedHistory.GetHashCode() : 0);
            return hashCode;
        }
    }
}

/// <summary>
/// Result returned when the migration was applicable, but failed during the actual application.
/// </summary>
public class ApplicableMigrationFailedResult : MigrationApplicationResult {
    /// <summary>
    /// Gets the migration that was to be applied.
    /// </summary>
    public ApplicableMigration Migration { get; }

    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a <see cref="ApplicableMigrationFailedResult"/>.
    /// </summary>
    public ApplicableMigrationFailedResult() { }

    /// <summary>
    /// Initializes a new <see cref="ApplicableMigrationFailedResult"/>.
    /// </summary>
    /// <param name="migration">The migration that was to be applied.</param>
    /// <param name="exception">The exception that occurred.</param>
    public ApplicableMigrationFailedResult(ApplicableMigration migration, Exception exception) {
        Migration = migration;
        Exception = exception;
    }

    /// <summary>
    /// Builder method that adds the migration that was to be applied to the result.
    /// </summary>
    /// <param name="migration">The migration that was to be applied.</param>
    /// <returns>A <see cref="ApplicableMigrationFailedResult"/>.</returns>
    public ApplicableMigrationFailedResult WithMigration(ApplicableMigration migration) => new ApplicableMigrationFailedResult(migration, Exception);

    /// <summary>
    /// Builder method that adds the exception that occurred to the result.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A <see cref="ApplicableMigrationFailedResult"/>.</returns>
    public ApplicableMigrationFailedResult WithException(Exception exception) => new ApplicableMigrationFailedResult(Migration, exception);

    bool Equals(ApplicableMigrationFailedResult other) {
        return Equals(Migration, other.Migration) && Equals(Exception, other.Exception);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ApplicableMigrationFailedResult) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            return ((Migration != null ? Migration.GetHashCode() : 0) * 397) ^ (Exception != null ? Exception.GetHashCode() : 0);
        }
    }
}

/// <summary>
/// Result returned when the migration was successfully applied, while the last applied migration was rolled back because it was a prerelease version.
/// </summary>
public class ApplicableMigrationSucceededWithRollbackResult : MigrationApplicationResult {
    /// <summary>
    /// Gets the migration that was to be applied to the migration history.
    /// </summary>
    public ApplicableMigration ApplicableMigration { get; }

    /// <summary>
    /// Gets the migration that was applied to the migration history.
    /// </summary>
    public AppliedMigration AppliedMigration { get; }

    /// <summary>
    /// Gets the migration that was rolled back.
    /// </summary>
    public RollbackEnabledAppliedMigration RolledBackMigration { get; }

    /// <summary>
    /// Gets the migration history as it is after applying the migration.
    /// </summary>
    public IMigrationHistory UpdatedHistory { get; }

    /// <summary>
    /// Initializes a new <see cref="ApplicableMigrationSucceededResult"/>.
    /// </summary>
    public ApplicableMigrationSucceededWithRollbackResult() { }

    /// <summary>
    /// Initializes a new <see cref="ApplicableMigrationSucceededResult"/>.
    /// </summary>
    /// <param name="applicableMigration">The migration that is to be applied.</param>
    /// <param name="appliedMigration">The migration that was applied.</param>
    /// <param name="rolledBackMigration">The migration that was rolled back.</param>
    /// <param name="updatedHistory">The migration history after the migration was applied.</param>
    public ApplicableMigrationSucceededWithRollbackResult(ApplicableMigration applicableMigration, AppliedMigration appliedMigration, RollbackEnabledAppliedMigration rolledBackMigration, IMigrationHistory updatedHistory) {
        ApplicableMigration = applicableMigration;
        AppliedMigration = appliedMigration;
        RolledBackMigration = rolledBackMigration;
        UpdatedHistory = updatedHistory;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededResult"/> with the given <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was to be applied.</param>
    /// <returns>A <see cref="ApplicableMigrationSucceededResult"/>.</returns>
    public ApplicableMigrationSucceededWithRollbackResult ForMigration(ApplicableMigration migration) => new ApplicableMigrationSucceededWithRollbackResult(migration, AppliedMigration, RolledBackMigration, UpdatedHistory);

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededResult"/> with the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was applied.</param>
    /// <returns></returns>
    public ApplicableMigrationSucceededWithRollbackResult WithAppliedMigration(AppliedMigration migration) => new ApplicableMigrationSucceededWithRollbackResult(ApplicableMigration, migration, RolledBackMigration, UpdatedHistory);

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededWithRollbackResult"/> with the given <see cref="RollbackEnabledAppliedMigration"/>.
    /// </summary>
    /// <param name="migration">The migration that was rolled back.</param>
    /// <returns></returns>
    public ApplicableMigrationSucceededWithRollbackResult WithRolledBackMigration(RollbackEnabledAppliedMigration migration) => new ApplicableMigrationSucceededWithRollbackResult(ApplicableMigration, AppliedMigration, migration, UpdatedHistory);

    /// <summary>
    /// Builder method that returns a new <see cref="ApplicableMigrationSucceededResult"/> with the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="migrationHistory">The migration history to which the migration is to be applied.</param>
    /// <returns>A <see cref="ApplicableMigrationSucceededResult"/>.</returns>
    public ApplicableMigrationSucceededWithRollbackResult WithHistory(IMigrationHistory migrationHistory) => new ApplicableMigrationSucceededWithRollbackResult(ApplicableMigration, AppliedMigration, RolledBackMigration, migrationHistory);

    bool Equals(ApplicableMigrationSucceededWithRollbackResult other) {
        return Equals(ApplicableMigration, other.ApplicableMigration) &&
               Equals(AppliedMigration, other.AppliedMigration) &&
               Equals(RolledBackMigration, other.RolledBackMigration) &&
               Equals(UpdatedHistory, other.UpdatedHistory);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ApplicableMigrationSucceededWithRollbackResult) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            var hashCode = (ApplicableMigration != null ? ApplicableMigration.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (AppliedMigration != null ? AppliedMigration.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (UpdatedHistory != null ? UpdatedHistory.GetHashCode() : 0);
            return hashCode;
        }
    }
}