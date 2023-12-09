using System;
using System.Threading.Tasks;

namespace Peregrinus.Model; 

/// <summary>
/// <see cref="RollbackStrategy"/> that rolls back migrations up to a requested count or less.
/// </summary>
public class OrdinalRollbackStrategy : RollbackStrategy {
    readonly int _count;
    readonly Action<MigrationRollbackResult> _onProcess;

    /// <summary>
    /// Initializes a new <see cref="OrdinalRollbackStrategy"/>.
    /// </summary>
    /// <param name="count">The number of migrations to rollback.</param>
    /// <param name="onProcess">A callback to process the result of each migration rollback.</param>
    public OrdinalRollbackStrategy(int count, Action<MigrationRollbackResult> onProcess = null) {
        _count = count;
        _onProcess = onProcess;
    }

    /// <summary>
    /// Performs the rollback on the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="history">The history to perform the rollback on.</param>
    /// <returns>An updated <see cref="IMigrationHistory"/>.</returns>
    public override async Task<IMigrationHistory> Rollback(IMigrationHistory history) {
        MigrationRollbackResult result;
        var iter = 0;
        do {
            result = await history.Rollback(_ => ++iter <= _count);
            _onProcess?.Invoke(result);
            if (result is RollbackSingleResult success)
                history = success.UpdatedHistory;
        } while (!(result is NoRollbackResult) && iter < _count);

        return history;
    }
}