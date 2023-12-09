using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Peregrinus.Database; 

class Query {
    public string Sql { get; }
    public CommandType? CommandType { get; }
    public object Parameters { get; }

    public Query(string sql) {
        Sql = sql ?? throw new ArgumentNullException();
    }

    Query(string sql, CommandType? commandType, object parameters) : this(sql) {
        CommandType = commandType;
        Parameters = parameters;
    }

    public Query WithCommandType(CommandType commandType) {
        return new Query(Sql, commandType, Parameters);
    }

    public Query WithParameters(object parameters) {
        return new Query(Sql, CommandType, parameters);
    }

    public Query WithParameters(IEnumerable<KeyValuePair<string, object>> parameters) {
        var dynamicParameters = new DynamicParameters();
        foreach (var entry in parameters) {
            dynamicParameters.Add(entry.Key, entry.Value);
        }
        return WithParameters(dynamicParameters);
    }
}