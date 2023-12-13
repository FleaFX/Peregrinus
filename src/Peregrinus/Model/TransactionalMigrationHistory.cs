using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Peregrinus.Model; 

/// <summary>
/// A <see cref="IMigrationHistory"/> decorator class that applies migrations within a transaction.
/// </summary>
public class TransactionalMigrationHistory : IMigrationHistory {
    readonly IMigrationHistory _innerMigrationHistory;

    /// <summary>
    /// Initializes a new <see cref="TransactionalMigrationHistory"/>.
    /// </summary>
    /// <param name="innerMigrationHistory">The decorated <see cref="IMigrationHistory"/>.</param>
    public TransactionalMigrationHistory(IMigrationHistory innerMigrationHistory) {
        _innerMigrationHistory = innerMigrationHistory ?? throw new ArgumentNullException(nameof(innerMigrationHistory));
    }

    /// <summary>
    /// Applies the given migration to the database.
    /// </summary>
    /// <param name="migration">The <see cref="ApplicableMigration"/> to apply.</param>
    /// <returns>A <see cref="MigrationApplicationResult"/>.</returns>
    public async Task<MigrationApplicationResult> Apply(ApplicableMigration migration) {
        if (!migration.CanRunInTransaction) return await _innerMigrationHistory.Apply(migration);

        using var transaction = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
        try {
            var result = await _innerMigrationHistory.Apply(migration);

            switch (result) {
                case ApplicableMigrationSucceededResult success :
                    // complete the transaction and make sure to decorate the updated history
                    transaction.Complete();
                    return success.WithHistory(new TransactionalMigrationHistory(success.UpdatedHistory));

                case ApplicableMigrationSucceededWithRollbackResult successWithRollback :
                    // complete the transaction and make sure to decorate the updated history
                    transaction.Complete();
                    return successWithRollback.WithHistory(new TransactionalMigrationHistory(successWithRollback.UpdatedHistory));

                default:
                    // we don't handle failure here. Pass it on
                    return result;
            }
        } catch (Exception exception) {
            return MigrationApplicationResult.ApplicableMigrationFailed.
                WithMigration(migration).
                WithException(exception);
        }
    }

    /// <summary>
    /// Rolls back the last applied migration.
    /// </summary>
    /// <remarks>
    /// An optional <see cref="Predicate{T}"/> condition gives the opportunity to inspect the rollback candidate in order to stop the rollback if need be. Omitting this <paramref name="shouldRollback">predicate</paramref> will perform the rollback regardless.
    /// </remarks>
    /// <param name="shouldRollback">An optional <see cref="Predicate{T}"/> condition gives the opportunity to inspect the rollback candidate in order to stop the rollback if need be.</param>
    /// <returns>A <see cref="MigrationRollbackResult"/>.</returns>
    public async Task<MigrationRollbackResult> Rollback(Predicate<AppliedMigration> shouldRollback = null) {
        using var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        // delegate to inner
        var result = await _innerMigrationHistory.Rollback(shouldRollback);

        // always complete. Rolling back either happens or not. There's no harm in committing anyway in case of the latter.
        transaction.Complete();

        // make sure to decorate the updated history
        if (result is RollbackSingleResult rollbackSingle)
            return rollbackSingle.WithUpdatedHistory(new TransactionalMigrationHistory(rollbackSingle.UpdatedHistory));

        return result;
    }

    /// <summary>
    /// Rolls back as many migrations as required by the given <paramref name="strategy"/>.
    /// </summary>
    /// <param name="strategy">The <see cref="RollbackStrategy"/> to use.</param>
    /// <returns>A <see cref="MigrationRollbackResult"/>.</returns>
    public async Task<MigrationRollbackResult> Rollback(RollbackStrategy strategy) {
        using var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        // delegate to inner
        var result = await _innerMigrationHistory.Rollback(strategy);

        // complete transaction. Failure situations should have thrown an exception, causing us to not reach this point
        transaction.Complete();

        // make sure to decorate the updated history
        if (result is RollbackByStrategyResult rollbackByStrategy)
            return rollbackByStrategy.WithUpdatedHistory(new TransactionalMigrationHistory(rollbackByStrategy.UpdatedHistory));

        return result;
    }
}