using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Peregrinus.Database; 

public interface IMigrationExecutor {
    /// <summary>
    /// Creates a new <see cref="IMigrationExecutor"/> using the given SQL statement.
    /// </summary>
    /// <param name="sql">A SQL query.</param>
    /// <returns>A <see cref="IMigrationExecutor"/>.</returns>
    IMigrationExecutor NewMigration(string sql);

    /// <summary>
    /// Executes the migration and returns the number of affected rows
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    Task<int> ExecuteAsync();
}

public class MigrationExecutor : IMigrationExecutor {
    readonly string _sql;
    readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new <see cref="MigrationExecutor"/>.
    /// </summary>
    /// <param name="connectionFactory">The <see cref="IDbConnectionFactory"/> to use when connecting to the database.</param>
    public MigrationExecutor(IDbConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    MigrationExecutor(IDbConnectionFactory connectionFactory, string sql) : this(connectionFactory) {
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
    }
    
    /// <summary>
    /// Creates a new <see cref="IMigrationExecutor"/> using the given SQL statement.
    /// </summary>
    /// <param name="sql">A SQL query.</param>
    /// <returns>A <see cref="IMigrationExecutor"/>.</returns>
    public IMigrationExecutor NewMigration(string sql) {
        return new MigrationExecutor(_connectionFactory, sql);
    }

    /// <summary>
    /// Executes the migration and returns the number of affected rows
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    public Task<int> ExecuteAsync() => Task.Run(() => {
        using var connection = _connectionFactory.CreateConnection();
        var server = new Server(new ServerConnection((SqlConnection)connection));
        return server.ConnectionContext.ExecuteNonQuery(_sql);
    });
}