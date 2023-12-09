using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Peregrinus.Model.Exceptions;
using Peregrinus.Resources;
using Peregrinus.Util;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Represents a <see cref="Migration"/> that has not yet been applied to the database.
/// </summary>
public class ApplicableMigration : Migration {
    protected readonly MigrationScriptContent _content;

    static readonly Regex VersionFormat = new Regex(@"(?<VersionInfo>(?<Major>[0-9]{1,})\.(?<Minor>[0-9]{1,})\.(?<Patch>[0-9]{1,})(?:-(?<Pre>(?:[0-9a-zA-Z-]\.{0,1})*){0,1}){0,1}(?:\+(?<meta>(?:[0-9a-zA-Z-]\.{0,1})*)){0,1})");
    static readonly Regex DescriptionFormat = new Regex(@"__(?<Description>\w{1,}?)\.\w+$");

    /// <summary>
    /// Initializes a new <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="version">The version of the migration.</param>
    /// <param name="description">The description of the migration.</param>
    /// <param name="content">The content of the migration.</param>
    public ApplicableMigration(SemVersion version, Description description, MigrationScriptContent content) : base(version, description, (Checksum)content) {
        _content = content;
    }

    /// <summary>
    /// Creates a new <see cref="Migration"/> from the given <see cref="IFileInfo">file</see>.
    /// </summary>
    /// <param name="name">The name of the migration script.</param>
    /// <param name="stream">The <see cref="Stream"/> that contains the script.</param>
    /// <returns>A <see cref="Migration"/>.</returns>
    public static ApplicableMigration FromStream(string name, Stream stream) {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (!name.StartsWith("V", StringComparison.OrdinalIgnoreCase)) throw new ArgumentOutOfRangeException(ApplicableMigrationTranslations.IncorrectMigrationFilenamePrefixMessage, nameof(stream));
        if (!VersionFormat.IsMatch(name)) throw new ArgumentOutOfRangeException(ApplicableMigrationTranslations.IncorrectMigrationVersionFormatMessage, nameof(stream));
        if (!DescriptionFormat.IsMatch(name)) throw new ArgumentOutOfRangeException(ApplicableMigrationTranslations.IncorrectMigrationDescriptionFormatMessage, nameof(stream));
        if (!name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) throw new ArgumentOutOfRangeException(ApplicableMigrationTranslations.IncorrectFileExtensionMessage, nameof(stream));

        return new ApplicableMigration (
            SemVersion.Parse(VersionFormat.Match(name).Groups["VersionInfo"].Value, SemVersionStyles.OptionalPatch),
            new Description(DescriptionFormat.Match(name).Groups["Description"].Value),
            MigrationScriptContent.FromStream(stream)
        );
    }

    /// <summary>
    /// Tries to create a new <see cref="ApplicableMigration"/> from the given <paramref name="name"/> and <see cref="Stream"/> and returns a <see cref="bool"/> to indicate success or failure.
    /// </summary>
    /// <param name="name">The name of the migration script.</param>
    /// <param name="stream">The <see cref="Stream"/> that contains the migration.</param>
    /// <param name="applicableMigration">The <see cref="ApplicableMigration"/> that gets assigned when successful.</param>
    /// <returns><c>true</c> if the file was loaded, otherwise <c>false</c>.</returns>
    public static bool TryFromStream(string name, Stream stream, out ApplicableMigration applicableMigration) {
        if (stream != null &&
            name.StartsWith("V", StringComparison.OrdinalIgnoreCase) &&
            VersionFormat.IsMatch(name) &&
            DescriptionFormat.IsMatch(name) &&
            name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) {
            applicableMigration = FromStream(name, stream);
            return true;
        }

        applicableMigration = null;
        return false;
    }

    /// <summary>
    /// Applies the migration and returns a <see cref="AppliedMigration"/> from the current instance.
    /// </summary>
    /// <param name="migrationContext">The <see cref="MigrationContext"/> that run the migration in.</param>
    /// <returns>A <see cref="AppliedMigration"/>.</returns>
    internal virtual async Task<AppliedMigration> Apply(IMigrationContext migrationContext) {
        if (!string.IsNullOrWhiteSpace(_version.Prerelease)) throw new PermanentPrereleaseMigrationException(this);
        var (executionTime, _) = await Diagnostics.TimeOperationAsync(() => _content.PrepareMigration(migrationContext)());
        return new AppliedMigration(_version, _description, _checksum, executionTime);
    }

    /// <summary>
    /// Adds rollback capability to the given <see cref="ApplicableMigration"/>.
    /// </summary>
    /// <param name="applicableMigration">The applicable migration.</param>
    /// <param name="rollbackMigration">The rollback migration.</param>
    /// <returns>A <see cref="RollbackEnabledApplicableMigration"/>.</returns>
    public static RollbackEnabledApplicableMigration operator +(ApplicableMigration applicableMigration, RollbackMigration rollbackMigration) =>
        new RollbackEnabledApplicableMigration(applicableMigration._version, applicableMigration._description, applicableMigration._content, rollbackMigration);

    /// <summary>
    /// Indicates whether this migration can be rolled back by the given <see cref="RollbackMigration"/>.
    /// </summary>
    /// <param name="rollbackMigration">The <see cref="RollbackMigration"/> to test.</param>
    /// <returns><c>true</c> if the current migration can be rolled back by the given <see cref="RollbackMigration"/>, otherwise <c>false</c>.</returns>
    public bool CanBeRolledBackBy(RollbackMigration rollbackMigration) =>
        rollbackMigration.AppliesTo(_version, _description);

    /// <summary>
    /// Indicates whether the migration can run within a transaction.
    /// </summary>
    public override bool CanRunInTransaction => _content.CanRunInTransaction;
}