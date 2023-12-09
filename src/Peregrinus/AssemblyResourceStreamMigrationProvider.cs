using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Peregrinus; 

/// <summary>
/// <see cref="IMigrationScriptProvider"/> which loads the migration scripts by scanning a given <see cref="Assembly"/> for embedded resources which matches the expected filename format for migration scripts.
/// </summary>
public class AssemblyResourceStreamMigrationProvider : IMigrationScriptProvider {
    readonly Assembly _assembly;

    /// <summary>
    /// Initializes a new <see cref="AssemblyResourceStreamMigrationProvider"/>.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly" /> which contains the migration scripts as embedded resources.</param>
    public AssemblyResourceStreamMigrationProvider(Assembly assembly) {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
    }

    /// <summary>
    /// Loads a sequence of migration scripts. These are returned as a <see cref="ValueTuple{T1, T2}"/> where the first item represents the filename of the migration, and the second item is a <see cref="Stream"/> containing the migration script contents.
    /// </summary>
    /// <returns>A sequence of migration scripts.</returns>
    public Task<IEnumerable<(string, Stream)>> LoadMigrationScripts() {
        var migrationFileNameRegex = new Regex(@"[VR]\d+\.\d+\.\d+(?:\.\d+)?(?:-[^_]+)?__\w*(?=\.sql)");
        return Task.FromResult(
            from manifestResourceName in _assembly.GetManifestResourceNames()
            where migrationFileNameRegex.IsMatch(manifestResourceName)
            select (migrationFileNameRegex.Match(manifestResourceName).Value, _assembly.GetManifestResourceStream(manifestResourceName))
        );
    }
}