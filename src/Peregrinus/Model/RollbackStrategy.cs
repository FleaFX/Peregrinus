using System;
using System.Threading.Tasks;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Base class for rollback strategies.
/// </summary>
public abstract class RollbackStrategy {
    /// <summary>
    /// Performs the rollback on the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="history">The history to perform the rollback on.</param>
    /// <returns>An updated <see cref="IMigrationHistory"/>.</returns>
    public abstract Task<IMigrationHistory> Rollback(IMigrationHistory history);

    /// <summary>
    /// Creates a <see cref="RollbackStrategy"/> that rolls back migrations up to a requested count or less.
    /// </summary>
    public static RollbackStrategy Ordinal(int count, Action<MigrationRollbackResult> onProcess = null) =>
        new OrdinalRollbackStrategy(count, onProcess);

    /// <summary>
    /// Creates a <see cref="RollbackStrategy"/> that rolls back migrations up to a specific version.
    /// </summary>
    public static RollbackStrategy TargetVersion(SemVersion version, Action<MigrationRollbackResult> onProcess = null) =>
        new TargetVersionRollbackStrategy(version, onProcess);

    /// <summary>
    /// Creates a <see cref="RollbackStrategy"/> that rolls back migrations to the beginning of the history, or up to the first milestone migration it encounters. Whichever comes first.
    /// </summary>
    public static RollbackStrategy Terminal(Action<MigrationRollbackResult> onProcess = null) =>
        new TerminalRollbackStrategy(onProcess);
}