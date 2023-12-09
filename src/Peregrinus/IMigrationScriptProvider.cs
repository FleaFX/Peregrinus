using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Peregrinus; 

public interface IMigrationScriptProvider {
    /// <summary>
    /// Loads a sequence of migration scripts. These are returned as a <see cref="ValueTuple{T1, T2}"/> where the first item represents the filename of the migration, and the second item is a <see cref="Stream"/> containing the migration script contents.
    /// </summary>
    /// <returns>A sequence of migration scripts.</returns>
    Task<IEnumerable<(string, Stream)>> LoadMigrationScripts();
}