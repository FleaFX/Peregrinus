using System.Data;

namespace Peregrinus.Database; 

/// <summary>
/// Implementors provide consumers with an <see cref="IDbConnection"/>.
/// </summary>
public interface IDbConnectionFactory {
    /// <summary>
    /// Creates a new <see cref="IDbConnection"/>.
    /// </summary>
    /// <returns>An <see cref="IDbConnection"/>.</returns>
    IDbConnection CreateConnection();

    /// <summary>
    /// Sets the target database to the given <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the new target database.</param>
    IDbConnectionFactory Target(string database);
}