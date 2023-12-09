using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Peregrinus.Database; 

/// <summary>
/// <see cref="IDbConnectionFactory"/> implementation that provides connections to the migration target database.
/// </summary>
public class MigrationTargetDatabase : IDbConnectionFactory {
    readonly string _connectionString;

    /// <summary>
    /// Initializes a new <see cref="MigrationTargetDatabase"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    public MigrationTargetDatabase(string connectionString) {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a new <see cref="IDbConnection"/>.
    /// </summary>
    /// <returns>An <see cref="IDbConnection"/>.</returns>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    /// <summary>
    /// Sets the target database to the given <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the new target database.</param>
    public IDbConnectionFactory Target(string database) {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        builder.InitialCatalog = database;
        return new MigrationTargetDatabase(builder.ToString());
    }
}