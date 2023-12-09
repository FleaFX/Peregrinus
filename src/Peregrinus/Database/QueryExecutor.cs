using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Peregrinus.Database; 

/// <summary>
/// DO NOT USE THIS INTERFACE DIRECTLY
/// Use the generically typed one instead. 
/// </summary>
public interface IQueryExecutor {
    /// <summary>
    /// Creates a new <see cref="IQueryExecutor"/> using the given SQL statement.
    /// </summary>
    /// <param name="sql">A SQL query.</param>
    /// <returns>A <see cref="IQueryExecutor"/>.</returns>
    IQueryExecutor NewQuery(string sql);

    /// <summary>
    /// Creates a new <see cref="IQueryExecutor"/> that uses the given object to fill the SQL query parameters.
    /// </summary>
    /// <param name="parameters">The parameters object.</param>
    /// <returns>A <see cref="IQueryExecutor"/>.</returns>
    IQueryExecutor WithParameters(object parameters);

    /// <summary>
    /// Creates a new <see cref="IQueryExecutor"/> that uses the given collection to fill the SQL query parameters.
    /// </summary>
    /// <param name="parameters">The parameters collection.</param>
    /// <returns>A <see cref="IQueryExecutor"/>.</returns>
    IQueryExecutor WithParameters(IEnumerable<KeyValuePair<string, object>> parameters);

    /// <summary>
    /// Creates a new <see cref="IQueryExecutor"/> that uses the given command type to execute queries
    /// </summary>
    /// <param name="commandType">The command type that will be used when executing the query.</param>
    /// <returns>A <see cref="IQueryExecutor"/>.</returns>
    IQueryExecutor WithCommandType(CommandType commandType);

    /// <summary>
    /// Executes the query and does not return the result.
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    int Execute();

    /// <summary>
    /// Executes the query and returns the results.
    /// </summary>
    /// <typeparam name="TResult">The type of the data transfer object.</typeparam>
    /// <returns>A collection of <typeparamref name="TResult"/>.</returns>
    IEnumerable<TResult> Execute<TResult>();


    /// <summary>
    /// Executes the query and returns the number of affected rows
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    Task<int> ExecuteAsync();

    /// <summary>
    /// Executes the query and returns the results.
    /// </summary>
    /// <typeparam name="TResult">The type of the data transfer object.</typeparam>
    /// <returns>A collection of <typeparamref name="TResult"/>.</returns>
    Task<IEnumerable<TResult>> ExecuteAsync<TResult>();

}

public class QueryExecutor : IQueryExecutor {
    readonly IDbConnectionFactory _connectionFactory;
    readonly Query _query;

    /// <summary>
    /// Creates a new <see cref="QueryExecutor"/>.
    /// </summary>
    /// <param name="connectionFactory">A <see cref="IDbConnectionFactory"/> to create database connections when necessary.</param>
    public QueryExecutor(IDbConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    QueryExecutor(IDbConnectionFactory connectionFactory, Query query) : this(connectionFactory) {
        _query = query ?? throw new ArgumentNullException(nameof(query));
    }
    
    public IQueryExecutor NewQuery(string sql) {
        return new QueryExecutor(_connectionFactory, new Query(sql));
    }

    public IQueryExecutor WithParameters(object parameters) {
        return new QueryExecutor(_connectionFactory, _query.WithParameters(parameters));
    }
    
    public IQueryExecutor WithParameters(IEnumerable<KeyValuePair<string, object>> parameters) {
        return new QueryExecutor(_connectionFactory, _query.WithParameters(parameters));
    }
    
    public IQueryExecutor WithCommandType(CommandType commandType) {
        return new QueryExecutor(_connectionFactory, _query.WithCommandType(commandType));
    }

    public int Execute() {
        return ExecuteOnConnection(c => c.Execute(_query.Sql, _query.Parameters, commandType: _query.CommandType));
    }

    public IEnumerable<TResult> Execute<TResult>() {
        return ExecuteOnConnection(c => c.Query<TResult>(_query.Sql, _query.Parameters, commandType: _query.CommandType));
    }
    
    public Task<int> ExecuteAsync() {
        return ExecuteOnConnectionAsync(c => c.ExecuteAsync(_query.Sql, _query.Parameters, commandType: _query.CommandType));
    }

    public Task<IEnumerable<TResult>> ExecuteAsync<TResult>() {
        return ExecuteOnConnectionAsync(c => c.QueryAsync<TResult>(_query.Sql, _query.Parameters, commandType: _query.CommandType));
    }
    
    TResult ExecuteOnConnection<TResult>(Func<IDbConnection, TResult> execute) {
        using (var connection = _connectionFactory.CreateConnection()) {
            return execute(connection);
        }
    }
    
    async Task<TResult> ExecuteOnConnectionAsync<TResult>(Func<IDbConnection, Task<TResult>> execute) {
        using (var connection = _connectionFactory.CreateConnection()) {
            return await execute(connection);
        }
    }
}

public interface IQueryExecutor<TConnectionFactory> : IQueryExecutor where TConnectionFactory : IDbConnectionFactory {}

public class QueryExecutor<TConnectionFactory> : IQueryExecutor<TConnectionFactory> where TConnectionFactory : IDbConnectionFactory {
    readonly IQueryExecutor _inner;

    public QueryExecutor(IQueryExecutor inner) {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IQueryExecutor NewQuery(string sql) {
        return _inner.NewQuery(sql);
    }

    public IQueryExecutor WithParameters(object parameters) {
        return _inner.WithParameters(parameters);
    }

    public IQueryExecutor WithParameters(IEnumerable<KeyValuePair<string, object>> parameters) {
        return _inner.WithParameters(parameters);
    }

    public IQueryExecutor WithCommandType(CommandType commandType) {
        return _inner.WithCommandType(commandType);
    }

    public int Execute() {
        return _inner.Execute();
    }

    public IEnumerable<TResult> Execute<TResult>() {
        return _inner.Execute<TResult>();
    }

    public Task<int> ExecuteAsync() {
        return _inner.ExecuteAsync();
    }

    public Task<IEnumerable<TResult>> ExecuteAsync<TResult>() {
        return _inner.ExecuteAsync<TResult>();
    }

}