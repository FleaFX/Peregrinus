using System;
using System.Threading.Tasks;

namespace Peregrinus.Model; 

/// <summary>
/// <see cref="RollbackStrategy"/> that rolls back migrations to the beginning of the history, or up to the first milestone migration it encounters. Whichever comes first.
/// </summary>
public class TerminalRollbackStrategy : RollbackStrategy {
    readonly Action<MigrationRollbackResult> _onProcess;

    /// <summary>
    /// Initializes a new <see cref="TerminalRollbackStrategy"/>.
    /// </summary>
    /// <param name="onProcess">A callback to process the result of each migration rollback.</param>
    public TerminalRollbackStrategy(Action<MigrationRollbackResult> onProcess = null) {
        _onProcess = onProcess;
    }

    /// <summary>
    /// Performs the rollback on the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="history">The history to perform the rollback on.</param>
    /// <returns>An updated <see cref="IMigrationHistory"/>.</returns>
    public override async Task<IMigrationHistory> Rollback(IMigrationHistory history) {
        MigrationRollbackResult result;
        do {
            result = await history.Rollback();
            _onProcess?.Invoke(result);
            if (result is RollbackSingleResult success)
                history = success.UpdatedHistory;
        } while (!(result is NoRollbackResult));

        return history;
    }
}