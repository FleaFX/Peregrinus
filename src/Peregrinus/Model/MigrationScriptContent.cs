using System;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Peregrinus.Model; 

/// <summary>
/// Represents the contents of a migration script.
/// </summary>
public readonly struct MigrationScriptContent {
    readonly string _value;

    /// <summary>
    /// Initializes a new <see cref="MigrationScriptContent"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    public MigrationScriptContent(string value) => _value = value;

    /// <summary>
    /// Indicates whether the script can run within a transaction.
    /// </summary>
    public bool CanRunInTransaction => !_value.Contains("ALTER DATABASE", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Calculates the <see cref="Checksum"/> of the given <see cref="MigrationScriptContent"/>.
    /// </summary>
    /// <param name="instance">The <see cref="MigrationScriptContent"/> to calculate the <see cref="Checksum"/> of.</param>
    public static explicit operator Checksum(MigrationScriptContent instance) =>
        new(SHA1.HashData(Encoding.UTF8.GetBytes(instance._value.ReplaceLineEndings(string.Empty).Trim())));

    /// <summary>
    /// Creates a <see cref="MigrationScriptContent"/> by reading the given <see cref="FileInfoBase"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read.</param>
    /// <returns>A <see cref="MigrationScriptContent"/>.</returns>
    public static MigrationScriptContent FromStream(Stream stream) {
        using var reader = new StreamReader(stream);
        return new MigrationScriptContent(reader.ReadToEnd());
    }

    /// <summary>
    /// Uses the given <see cref="MigrationContext"/> to prepare a new <see cref="AsyncMigrationOperation"/> with the current content.
    /// </summary>
    /// <param name="migrationContext">The <see cref="MigrationContext"/> to use.</param>
    /// <returns>A <see cref="AsyncMigrationOperation"/>.</returns>
    internal AsyncMigrationOperation PrepareMigration(IMigrationContext migrationContext) =>
        migrationContext.PrepareMigration(_value);
}