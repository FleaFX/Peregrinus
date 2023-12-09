using System;

namespace Peregrinus.Model.Exceptions; 

/// <summary>
/// Thrown when there are rollback migrations that do not apply to any other migration script.
/// </summary>
public class UnapplicableRollbackMigrationsException : Exception {
    /// <summary>
    /// Gets the set of <see cref="RollbackMigration"/> that don't apply to any migrations scripts.
    /// </summary>
    public RollbackMigration[] RollbackMigrations { get; }

    /// <summary>
    /// Initializes a new <see cref="UnapplicableRollbackMigrationsException"/>.
    /// </summary>
    /// <param name="rollbackMigrations">The set of <see cref="RollbackMigration"/> that don't apply to any migration scripts.</param>
    public UnapplicableRollbackMigrationsException(params RollbackMigration[] rollbackMigrations) {
        RollbackMigrations = rollbackMigrations;
    }
}