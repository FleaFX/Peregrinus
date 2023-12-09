using System;
using Semver;

namespace Peregrinus.Model.Exceptions; 

/// <summary>
/// Thrown when employing the <see cref="TargetVersionRollbackStrategy"/> with a version that is either not applied or unreachable because of a migration without a rollback script.
/// </summary>
public class UnreachableRollbackTargetVersionException : Exception {
    /// <summary>
    /// Gets the version that was targeted to be rolled back to.
    /// </summary>
    public SemVersion TargetVersion { get; }

    /// <summary>
    /// Initializes a new <see cref="UnreachableRollbackTargetVersionException"/>
    /// </summary>
    /// <param name="targetVersion">The version that was targeted to be rolled back to.</param>
    public UnreachableRollbackTargetVersionException(SemVersion targetVersion) {
        TargetVersion = targetVersion;
    }
}