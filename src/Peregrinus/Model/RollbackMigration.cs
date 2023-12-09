using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Peregrinus.Resources;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Represents a <see cref="Migration"/> that rolls back a previously applied migration from the database.
/// </summary>
public class RollbackMigration : Migration {
    readonly MigrationScriptContent _content;

    static readonly Regex VersionFormat = new(@"(?<VersionInfo>(?<Major>[0-9]{1,})\.(?<Minor>[0-9]{1,})\.(?<Patch>[0-9]{1,})(?:-(?<Pre>(?:[0-9a-zA-Z-]\.{0,1})*){0,1}){0,1}(?:\+(?<meta>(?:[0-9a-zA-Z-]\.{0,1})*)){0,1})");
    static readonly Regex DescriptionFormat = new(@"__(?<Description>\w{1,}?)\.\w+$");

    /// <summary>
    /// Initializes a new <see cref="RollbackMigration"/>.
    /// </summary>
    /// <param name="version">The version of the migration.</param>
    /// <param name="description">The description of the migration.</param>
    /// <param name="content">The content of the migration.</param>
    public RollbackMigration(SemVersion version, Description description, MigrationScriptContent content) : base(version, description, (Checksum)content) {
        _content = content;
    }

    /// <summary>
    /// Creates a new <see cref="RollbackMigration"/> from the given <see cref="IFileInfo">file</see>.
    /// </summary>
    /// <param name="name">The name of the rollback script.</param>
    /// <param name="stream">The <see cref="Stream"/> that contains the rollback script.</param>
    /// <returns>A <see cref="RollbackMigration"/>.</returns>
    public static RollbackMigration FromStream(string name, Stream stream) {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (!name.StartsWith("R", StringComparison.OrdinalIgnoreCase)) throw new ArgumentOutOfRangeException(RollbackMigrationTranslations.IncorrectMigrationFilenamePrefixMessage, nameof(stream));
        if (!VersionFormat.IsMatch(name)) throw new ArgumentOutOfRangeException(RollbackMigrationTranslations.IncorrectMigrationVersionFormatMessage, nameof(stream));
        if (!DescriptionFormat.IsMatch(name)) throw new ArgumentOutOfRangeException(RollbackMigrationTranslations.IncorrectMigrationDescriptionFormatMessage, nameof(stream));
        if (!name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) throw new ArgumentOutOfRangeException(RollbackMigrationTranslations.IncorrectFileExtensionMessage, nameof(stream));

        return new RollbackMigration (
            SemVersion.Parse(VersionFormat.Match(name).Groups["VersionInfo"].Value, SemVersionStyles.OptionalPatch),
            new Description(DescriptionFormat.Match(name).Groups["Description"].Value),
            MigrationScriptContent.FromStream(stream)
        );
    }

    /// <summary>
    /// Tries to create a new <see cref="RollbackMigration"/> from the given <see cref="IFileInfo"/> and returns a <see cref="bool"/> to indicate success or failure.
    /// </summary>
    /// <param name="name">The name of the rollback script.</param>
    /// <param name="stream">The <see cref="Stream"/> that contains the migration.</param>
    /// <param name="rollbackMigration">The <see cref="RollbackMigration"/> that gets assigned when successful.</param>
    /// <returns><c>true</c> if the file was loaded, otherwise <c>false</c>.</returns>
    public static bool TryFromStream(string name, Stream stream, out RollbackMigration rollbackMigration) {
        if (stream != null &&
            name.StartsWith("R", StringComparison.OrdinalIgnoreCase) &&
            VersionFormat.IsMatch(name) &&
            DescriptionFormat.IsMatch(name) &&
            name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) {
            rollbackMigration = FromStream(name, stream);
            return true;
        }

        rollbackMigration = null;
        return false;
    }

    /// <summary>
    /// Indicates whether this <see cref="RollbackMigration"/> applies to the given <see cref="SemVersion"/> and <see cref="Description"/>.
    /// </summary>
    /// <remarks>We cannot just rely on the version alone. The developer may have made an error. Relying on the description as well,
    /// and requiring it to be the same as the description of the migration being rolled back, gives us an extra safety line.</remarks>
    /// <param name="version">The version to compare to.</param>
    /// <param name="description">The description to compare to.</param>
    /// <returns><c>true</c> if both of the given <see cref="SemVersion"/> and <see cref="Description"/> are equal to those of this <see cref="RollbackMigration"/>,
    /// otherwise <c>false</c>.</returns>
    public bool AppliesTo(SemVersion version, Description description) =>
        Equals(_version, version) && Equals(_description, description);

    /// <summary>
    /// Performs the rollback.
    /// </summary>
    /// <param name="migrationContext">The <see cref="IMigrationContext"/> to use.</param>
    internal async Task Rollback(IMigrationContext migrationContext) {
        await _content.PrepareMigration(migrationContext)();
    }

    /// <summary>
    /// Indicates whether the migration can run within a transaction.
    /// </summary>
    public override bool CanRunInTransaction => _content.CanRunInTransaction;
}