namespace Peregrinus.Model; 

public static class ExtensionsForMigrationApplicationResult {
    /// <summary>
    /// Adds the version of the given <see cref="ApplicableMigration"/> as the version of the last applied migration.
    /// </summary>
    /// <param name="result">The result to build on.</param>
    /// <param name="migration">The migration to take the version of.</param>
    /// <returns>A new <see cref="MigrationVersionAnachronismResult"/>.</returns>
    public static MigrationVersionAnachronismResult WithVersionOf(this MigrationVersionAnachronismResult result, AppliedMigration migration) => migration.Build(result);
}