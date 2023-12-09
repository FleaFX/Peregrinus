namespace Peregrinus.Model; 

public class MigrationRollbackResult {
    /// <summary>
    /// Result returned when a single rollback was attempted that resulted in no rollback being performed.
    /// </summary>
    public static readonly NoRollbackResult NoRollback = new NoRollbackResult();

    /// <summary>
    /// Result returned when a single rollback was performed.
    /// </summary>
    public static readonly RollbackSingleResult RollbackSingle = new RollbackSingleResult();

    /// <summary>
    /// Result returned when completing the whole rollback according to the employed <see cref="RollbackStrategy"/>.
    /// </summary>
    public static readonly RollbackByStrategyResult ByStrategy = new RollbackByStrategyResult();
}

public class RollbackByStrategyResult : MigrationRollbackResult {
    /// <summary>
    /// Gets the migration history as it is after rolling back.
    /// </summary>
    public IMigrationHistory UpdatedHistory { get; }

    /// <summary>
    /// Initializes a new <see cref="RollbackByStrategyResult"/>.
    /// </summary>
    public RollbackByStrategyResult() { }

    /// <summary>
    /// Initializes a new <see cref="RollbackByStrategyResult"/>.
    /// </summary>
    /// <param name="updatedHistory">The migration history as it is after rolling back.</param>
    public RollbackByStrategyResult(IMigrationHistory updatedHistory) {
        UpdatedHistory = updatedHistory;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="RollbackByStrategyResult"/> with the given <see cref="IMigrationHistory"/>.
    /// </summary>
    /// <param name="history">The migration history as it is after rolling back.</param>
    /// <returns></returns>
    public RollbackByStrategyResult WithUpdatedHistory(IMigrationHistory history) => new RollbackByStrategyResult(history);
}

public class NoRollbackResult : MigrationRollbackResult {
    /// <summary>
    /// Gets the reason why there was no rollback performed.
    /// </summary>
    public string Reason { get; }
    
    /// <summary>
    /// Initializes a new <see cref="NoRollbackResult"/>.
    /// </summary>
    public NoRollbackResult() { }

    /// <summary>
    /// Initializes a new <see cref="NoRollbackResult"/>.
    /// </summary>
    /// <param name="reason">The reason why there was no rollback performed.</param>
    public NoRollbackResult(string reason) {
        Reason = reason;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="NoRollbackResult"/> with the given reason.
    /// </summary>
    /// <param name="reason">The reason why there was no rollback performed.</param>
    /// <returns></returns>
    public NoRollbackResult Because(string reason) => new NoRollbackResult(reason);
}

public class RollbackSingleResult : MigrationRollbackResult {
    /// <summary>
    /// Gets the migration that was rolled back.
    /// </summary>
    public RollbackEnabledAppliedMigration RolledBackMigration { get; }

    /// <summary>
    /// Gets the updated history after the rollback.
    /// </summary>
    public IMigrationHistory UpdatedHistory { get; }

    /// <summary>
    /// Initializes a new <see cref="RollbackSingleResult"/>.
    /// </summary>
    public RollbackSingleResult() { }

    /// <summary>
    /// Initializes a new <see cref="RollbackSingleResult"/>.
    /// </summary>
    /// <param name="rolledBackMigration">The migration that was rolled back.</param>
    /// <param name="updatedHistory">The updated history after the rollback.</param>
    public RollbackSingleResult(RollbackEnabledAppliedMigration rolledBackMigration, IMigrationHistory updatedHistory) {
        RolledBackMigration = rolledBackMigration;
        UpdatedHistory = updatedHistory;
    }

    /// <summary>
    /// Builder method that returns a new <see cref="RollbackSingleResult"/> with the given rolled back migration.
    /// </summary>
    /// <param name="migration">The migration that was rolled back.</param>
    /// <returns></returns>
    public RollbackSingleResult WithRolledBackMigration(RollbackEnabledAppliedMigration migration) => new RollbackSingleResult(migration, UpdatedHistory);

    /// <summary>
    /// Builder method that returns a new <see cref="RollbackSingleResult"/> with the given updated history.
    /// </summary>
    /// <param name="history">The updated history after the rollback.</param>
    /// <returns></returns>
    public RollbackSingleResult WithUpdatedHistory(IMigrationHistory history) => new RollbackSingleResult(RolledBackMigration, history);

    protected bool Equals(RollbackSingleResult other) {
        return Equals(RolledBackMigration, other.RolledBackMigration) && Equals(UpdatedHistory, other.UpdatedHistory);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RollbackSingleResult) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            return ((RolledBackMigration != null ? RolledBackMigration.GetHashCode() : 0) * 397) ^ (UpdatedHistory != null ? UpdatedHistory.GetHashCode() : 0);
        }
    }
}