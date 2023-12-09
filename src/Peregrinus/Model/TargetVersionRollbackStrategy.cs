using System;
using System.Threading.Tasks;
using Peregrinus.Model.Exceptions;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// <see cref="RollbackStrategy"/> that rolls back migrations up to a specific version.
/// </summary>
public class TargetVersionRollbackStrategy : RollbackStrategy {
    readonly AppliedMigration _target;
    readonly SemVersion _targetVersion;
    readonly Action<MigrationRollbackResult> _onProcess;

    /// <summary>
    /// Initializes a new <see cref="TargetVersionRollbackStrategy"/>.
    /// </summary>
    /// <param name="targetVersion">The target version to rollback to.</param>
    /// <param name="onProcess">A callback to process the result of each migration rollback.</param>
    public TargetVersionRollbackStrategy(SemVersion targetVersion, Action<MigrationRollbackResult> onProcess = null) {
        _target = new AppliedMigration(targetVersion, Description.None, Checksum.Empty);
        _targetVersion = targetVersion;
        _onProcess = onProcess;
    }

    /// <summary>
    /// Performs the rollback on the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="history">The history to perform the rollback on.</param>
    /// <returns>An updated <see cref="IMigrationHistory"/>.</returns>
    public override async Task<IMigrationHistory> Rollback(IMigrationHistory history) {
        MigrationRollbackResult result;
        var targetReached = false;
        do {
            result = await history.Rollback(migration => {
                var order = Migration.Compare.ByVersion(_target, migration);
                if (order == 0) targetReached = true;
                return order < 0;
            });
            _onProcess?.Invoke(result);
            if (result is RollbackSingleResult success)
                history = success.UpdatedHistory;
        } while (!(result is NoRollbackResult));

        // processing is stopped, but we may not have reached the target version yet
        if (!targetReached) throw new UnreachableRollbackTargetVersionException(_targetVersion);

        return history;
    }
}