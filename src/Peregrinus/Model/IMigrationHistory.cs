using System;
using System.Threading.Tasks;

namespace Peregrinus.Model; 

public interface IMigrationHistory {
    /// <summary>
    /// Applies the given migration to the database.
    /// </summary>
    /// <param name="migration">The <see cref="ApplicableMigration"/> to apply.</param>
    /// <returns>A <see cref="MigrationApplicationResult"/>.</returns>
    Task<MigrationApplicationResult> Apply(ApplicableMigration migration);

    /// <summary>
    /// Rolls back the last applied migration.
    /// </summary>
    /// <remarks>
    /// An optional <see cref="Predicate{T}"/> condition gives the opportunity to inspect the rollback candidate in order to stop the rollback if need be. Omitting this <paramref name="shouldRollback">predicate</paramref> will perform the rollback regardless.
    /// </remarks>
    /// <param name="shouldRollback">An optional <see cref="Predicate{T}"/> condition gives the opportunity to inspect the rollback candidate in order to stop the rollback if need be.</param>
    /// <returns>A <see cref="MigrationRollbackResult"/>.</returns>
    Task<MigrationRollbackResult> Rollback(Predicate<AppliedMigration> shouldRollback = null);

    /// <summary>
    /// Rolls back as many migrations as required by the given <paramref name="strategy"/>.
    /// </summary>
    /// <param name="strategy">The <see cref="RollbackStrategy"/> to use.</param>
    /// <returns>A <see cref="MigrationRollbackResult"/>.</returns>
    Task<MigrationRollbackResult> Rollback(RollbackStrategy strategy);
}