using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Peregrinus.Model.Exceptions;
using Peregrinus.Resources;
using Peregrinus.Util;

namespace Peregrinus.Model; 

/// <summary>
/// Collects a bunch of <see cref="ApplicableMigration"/> and applies them in version order.
/// </summary>
public class MigrationsBatch {
    readonly IEnumerable<ApplicableMigration> _applicableMigrations;
    readonly IEnumerable<RollbackMigration> _rollbackMigrations;

    /// <summary>
    /// Returns a <see cref="MigrationsBatch"/> with no migrations in it.
    /// </summary>
    public static MigrationsBatch Empty => new(Array.Empty<ApplicableMigration>());

    /// <summary>
    /// Initializes a new <see cref="MigrationsBatch"/>.
    /// </summary>
    /// <param name="applicableMigrations"></param>
    public MigrationsBatch(params ApplicableMigration[] applicableMigrations) {
        _applicableMigrations = applicableMigrations;
        _rollbackMigrations = Array.Empty<RollbackMigration>();
    }

    /// <summary>
    /// Initializes a new <see cref="MigrationsBatch"/>.
    /// </summary>
    /// <param name="rollbackMigrations"></param>
    public MigrationsBatch(params RollbackMigration[] rollbackMigrations) {
        _rollbackMigrations = rollbackMigrations;
        _applicableMigrations = Array.Empty<ApplicableMigration>();
    }

    /// <summary>
    /// Initializes a new <see cref="MigrationsBatch"/>.
    /// </summary>
    /// <param name="applicableMigrations"></param>
    /// <param name="rollbackMigrations"></param>
    MigrationsBatch(IEnumerable<ApplicableMigration> applicableMigrations, IEnumerable<RollbackMigration> rollbackMigrations) {
        var (a, b) = Reconcile(applicableMigrations, rollbackMigrations);
        _applicableMigrations = a;
        _rollbackMigrations = b;
    }

    /// <summary>
    /// Loads a migrations script as a <see cref="MigrationsBatch"/> from the given <paramref name="name"/> and <see cref="Stream"/>.
    /// </summary>
    /// <param name="name">The name of the migration script.</param>
    /// <param name="stream">The <see cref="Stream"/> holding the migration script.</param>
    /// <returns>A <see cref="MigrationsBatch"/>.</returns>
    public static MigrationsBatch FromStream(string name, Stream stream) {
        if (ApplicableMigration.TryFromStream(name, stream, out ApplicableMigration applicableMigration))
            return applicableMigration;
        if (RollbackMigration.TryFromStream(name, stream, out RollbackMigration rollbackMigration))
            return rollbackMigration;
        throw new InvalidOperationException(MigrationsBatchTranslations.FromFileUnsupportedFileExceptionMessage);
    }

    /// <summary>
    /// Matches <see cref="RollbackMigration"/> with its corresponding <see cref="ApplicableMigration"/> and upgrade to a <see cref="RollbackEnabledApplicableMigration"/> when possible.
    /// </summary>
    /// <param name="applicableMigrations">The set of <see cref="ApplicableMigration"/>.</param>
    /// <param name="rollbackMigrations">The set of <see cref="RollbackMigration"/>.</param>
    /// <returns>A tuple of applicable migrations and unused rollback migrations.</returns>
    static (IEnumerable<ApplicableMigration>, IEnumerable<RollbackMigration>) Reconcile(IEnumerable<ApplicableMigration> applicableMigrations,
        IEnumerable<RollbackMigration> rollbackMigrations) {
        var rollbackEnabledMigrations = applicableMigrations
            .Select(a => {
                var rollback = rollbackMigrations.FirstOrDefault(a.CanBeRolledBackBy);
                return !Equals(rollback, default(RollbackMigration)) ? a + rollback : a;
            });
        var unusedRollbacks = rollbackMigrations
            .Where(r => !applicableMigrations.Any(a => a.CanBeRolledBackBy(r)));
        return (rollbackEnabledMigrations, unusedRollbacks);
    }

    /// <summary>
    /// Applies all the migrations in the current <see cref="MigrationsBatch"/> to the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="migrationHistory">The <see cref="IMigrationHistory"/> to apply the migrations to.</param>
    /// <param name="onProcess">A callback to process the result of each applied migration.</param>
    public async Task ApplyTo(IMigrationHistory migrationHistory, Action<MigrationApplicationResult> onProcess) {
        if (_rollbackMigrations.Any()) throw new UnapplicableRollbackMigrationsException(_rollbackMigrations.ToArray());
        var migrations = new List<ApplicableMigration>(_applicableMigrations);
        migrations.Sort(Migration.Compare.ByVersion);
        foreach (var migration in migrations) {
            var result = await migrationHistory.Apply(migration);
            onProcess(result);
            if (result is ApplicableMigrationSucceededResult success) {
                migrationHistory = success.UpdatedHistory;
            }
            if (result is ApplicableMigrationSucceededWithRollbackResult successWithRollback) {
                migrationHistory = successWithRollback.UpdatedHistory;
            }
        }
    }

    /// <summary>
    /// Tries to match the given <see cref="AppliedMigration"/> to an <see cref="ApplicableMigration"/> in this batch. It will then do one of three things:
    /// * if no applicable migration is found, an exception is thrown. This means we either lost the original migration file, or we're trying to apply a batch of different migrations than what was originally applied to this database.
    /// * if an applicable migration was matched, that has no rollback capability, the given <see cref="AppliedMigration"/> will be returned as is.
    /// * if an applicable migration with rollback capability was matched, we will return a rollback capable instance of the given <see cref="AppliedMigration"/>.
    /// </summary>
    /// <param name="appliedMigration">The <see cref="AppliedMigration"/> to match in this batch.</param>
    /// <returns>Either a <see cref="AppliedMigration"/> or a <see cref="RollbackEnabledAppliedMigration"/>.</returns>
    public async Task<AppliedMigration> Match(AppliedMigration appliedMigration) {
        if (_rollbackMigrations.Any()) throw new InvalidOperationException(MigrationsBatchTranslations.UnmatchedRollbackMigrationsFoundExceptionMessage);
        var match = _applicableMigrations.FirstOrDefault(applicable => appliedMigration.Equals(applicable));
        if (match == null) throw new MigrationsBatchMismatchException(this, appliedMigration);
        if (match is RollbackEnabledApplicableMigration rollbackEnabledApplicableMigration) {
            return await rollbackEnabledApplicableMigration.Apply(new NullMigrationContext());
        }
        return appliedMigration;
    }

    /// <summary>
    /// Creates a new <see cref="MigrationsBatch"/> for the given <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="applicableMigration">The <see cref="ApplicableMigration"/> to put into a <see cref="MigrationsBatch"/>.</param>
    public static implicit operator MigrationsBatch(ApplicableMigration applicableMigration) =>
        new(new[] { applicableMigration });

    /// <summary>
    /// Creates a new <see cref="MigrationsBatch"/> for the given <see cref="RollbackMigration"/>.
    /// </summary>
    /// <param name="rollbackMigration">The <see cref="RollbackMigration"/> to put into a <see cref="MigrationsBatch"/>.</param>
    public static implicit operator MigrationsBatch(RollbackMigration rollbackMigration) =>
        new(rollbackMigration);

    /// <summary>
    /// Merges the collection of <see cref="ApplicableMigration"/> into a new <see cref="MigrationsBatch"/>.
    /// </summary>
    /// <param name="batch1">The first <see cref="MigrationsBatch"/>.</param>
    /// <param name="batch2">The second <see cref="MigrationsBatch"/>.</param>
    /// <returns>A new <see cref="MigrationsBatch"/>.</returns>
    public static MigrationsBatch operator +(MigrationsBatch batch1, MigrationsBatch batch2) {
        if (ReferenceEquals(batch1, null)) return batch2;
        if (ReferenceEquals(batch2, null)) return batch1;
        return new MigrationsBatch(
            batch1._applicableMigrations.Concat(batch2._applicableMigrations).Distinct().ToArray(),
            batch1._rollbackMigrations.Concat(batch2._rollbackMigrations).Distinct().ToArray());
    }

    bool Equals(MigrationsBatch other) {
        return
            Enumerable.SequenceEqual(_applicableMigrations, other._applicableMigrations) &&
            Enumerable.SequenceEqual(_rollbackMigrations, other._rollbackMigrations);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MigrationsBatch)obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() =>
        _applicableMigrations != null && _applicableMigrations.Any() ? _applicableMigrations.Select(v => v.GetHashCode()).Reduce((a, b) => a * 97 ^ b) : 0;
}