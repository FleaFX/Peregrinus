using System;
using Peregrinus.Model;
using Peregrinus.Resources;

namespace Peregrinus; 

public enum MigrationFailureReason : int {
    MigrationVersionAnachronism,
    ApplicableMigrationFailed
}

public class MigrationException : Exception {
    /// <summary>
    /// Gets the reason why a migration failed.
    /// </summary>
    public MigrationFailureReason Reason { get; }

    public ApplicableMigration ApplicableMigration { get; }

    /// <summary>
    /// Gets the error code that matches the reason why a migration failed.
    /// </summary>
    public int ErrorCode => (int) Reason;

    /// <summary>
    /// Initializes a new <see cref="MigrationException"/>.
    /// </summary>
    /// <param name="reason">The reason why a migration failed.</param>
    /// <param name="applicableMigration">The applicable migration that failed.</param>
    public MigrationException(MigrationFailureReason reason, ApplicableMigration applicableMigration) {
        Reason = reason;
        ApplicableMigration = applicableMigration;
    }

    /// <summary>
    /// Initializes a new <see cref="MigrationException"/>.
    /// </summary>
    /// <param name="reason">The reason why a migration failed.</param>
    /// <param name="applicableMigration">The applicable migration that failed.</param>
    /// <param name="innerException">The <see cref="Exception"/> that occurred during the migration.</param>
    public MigrationException(MigrationFailureReason reason, ApplicableMigration applicableMigration, Exception innerException) : base(string.Empty, innerException) {
        Reason = reason;
        ApplicableMigration = applicableMigration;
    }

    /// <summary>Gets a message that describes the current exception.</summary>
    /// <returns>The error message that explains the reason for the exception, or an empty string ("").</returns>
    public override string Message => MigrationExceptionTranslations.ResourceManager.GetString(Enum.GetName(typeof(MigrationFailureReason), Reason) ?? "UnknownReason") ?? "";
}