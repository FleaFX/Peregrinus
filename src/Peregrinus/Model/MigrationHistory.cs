using System;
using System.Linq;
using System.Threading.Tasks;
using Peregrinus.Resources;
using Peregrinus.Util;

namespace Peregrinus.Model; 

public class MigrationHistory : IMigrationHistory {
    readonly IMigrationContext _migrationContext;
    readonly AppliedMigration[] _appliedMigrations;

    /// <summary>
    /// Initializes a new <see cref="MigrationHistory"/>.
    /// </summary>
    /// <param name="migrationContext">The <see cref="MigrationContext"/> to run migrations in.</param>
    /// <param name="appliedMigrations">A collection of migrations that are already applied.</param>
    public MigrationHistory(IMigrationContext migrationContext, params AppliedMigration[] appliedMigrations) {
        _migrationContext = migrationContext ?? throw new ArgumentNullException(nameof(migrationContext));
        _appliedMigrations = appliedMigrations;
    }

    /// <summary>
    /// Applies the given migration to the database.
    /// </summary>
    /// <param name="migration">The <see cref="ApplicableMigration"/> to apply.</param>
    /// <returns>A <see cref="MigrationApplicationResult"/>.</returns>
    public async Task<MigrationApplicationResult> Apply(ApplicableMigration migration) {
        // 1. check if the migration was already applied.
        var appliedMigration = _appliedMigrations.FirstOrDefault(applied => applied.Equals(migration));
        if (appliedMigration != null)
            return MigrationApplicationResult.AppliedMigrationSkipped.
                WithApplicableMigration(migration).
                WithAppliedMigration(appliedMigration);

        // 2. check if the version does not precede already applied migrations
        var lastAppliedMigration = _appliedMigrations.LastOrDefault();
        if (lastAppliedMigration != null) {
            if (Migration.Compare.ByVersion(lastAppliedMigration, migration) >= 0)
                return MigrationApplicationResult.MigrationVersionAnachronism.
                    WithApplicableMigration(migration).
                    WithVersionOf(lastAppliedMigration);
        }

        // 3. check if the last applied migration was a prerelease version, in which case we want to roll it back before applying the new migration
        if (await Rollback(last => last.IsPrerelease) is RollbackSingleResult rolledBackLast) {
            // apply the replacing migration to the current history and write a history record for it
            var replacement = await (await migration.Apply(_migrationContext)).Historicize(_migrationContext);
            return MigrationApplicationResult.ApplicableMigrationSucceededWithRollback.
                ForMigration(migration).
                WithAppliedMigration(replacement).
                WithRolledBackMigration(rolledBackLast.RolledBackMigration).
                WithHistory(new MigrationHistory(_migrationContext,
                    _appliedMigrations.Take(_appliedMigrations.Length - 1).Concat(new[] {replacement}).ToArray()));
        }

        // 4. apply the migration to the current history and write a history record for it
        var result = await (await migration.Apply(_migrationContext)).Historicize(_migrationContext);
        return MigrationApplicationResult.ApplicableMigrationSucceeded.
            ForMigration(migration).
            WithAppliedMigration(result).
            WithHistory(new MigrationHistory(_migrationContext, _appliedMigrations.Concat(new [] { result }).ToArray()));
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
        // 1. take the last applied migration. If there is none, there is nothing to rollback
        var lastAppliedMigration = _appliedMigrations.LastOrDefault();
        if (lastAppliedMigration == null)
            return MigrationRollbackResult.NoRollback.Because(MigrationHistoryTranslations.NoRollbackHistoryEmptyReason);

        // 2. check if the migration has rollback capacity
        if (!(lastAppliedMigration is RollbackEnabledAppliedMigration rollbackMigration))
            return MigrationRollbackResult.NoRollback.Because(MigrationHistoryTranslations.NoRollbackScriptReason);

        // 3. should we perform the rollback?
        if (!shouldRollback?.Invoke(rollbackMigration) ?? false)
            return MigrationRollbackResult.NoRollback.Because(MigrationHistoryTranslations.NoRollbackPreconditionFailedReason);

        // 4. nothing stopping us now, do the rollback
        await rollbackMigration.Rollback(_migrationContext);

        return MigrationRollbackResult.RollbackSingle.WithRolledBackMigration(rollbackMigration)
            .WithUpdatedHistory(new MigrationHistory(_migrationContext, _appliedMigrations.Take(_appliedMigrations.Length - 1).ToArray()));
    }

    /// <summary>
    /// Rolls back as many migrations as required by the given <paramref name="strategy"/>.
    /// </summary>
    /// <param name="strategy">The <see cref="RollbackStrategy"/> to use.</param>
    /// <returns>A <see cref="MigrationRollbackResult"/>.</returns>
    public async Task<MigrationRollbackResult> Rollback(RollbackStrategy strategy) {
        var history = await strategy.Rollback(this);
        return MigrationRollbackResult.ByStrategy.WithUpdatedHistory(history);
    }

    bool Equals(MigrationHistory other) {
        return _appliedMigrations.SequenceEqual(other._appliedMigrations);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MigrationHistory) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        return (_appliedMigrations != null && _appliedMigrations.Length > 0 ? _appliedMigrations.Select(v => v.GetHashCode()).Reduce((a, b) => a * 97 ^ b) : 0);
    }
}