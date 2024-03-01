using System.Linq;
using Peregrinus.Database;

namespace Peregrinus.Model; 

public static class ExtensionsForProvisioning {
    public static void ProvisionDatabase(this IQueryExecutor queryExecutor, string targetDatabaseName) {
        var provisionDatabase = $@"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE [name] = '{targetDatabaseName}')
BEGIN
	CREATE DATABASE [{targetDatabaseName}];
END";
        queryExecutor.NewQuery(provisionDatabase).Execute();
    }

    public static void ProvisionLogins(this IQueryExecutor queryExecutor, LoginInfo[] logins) {
        foreach (var loginInfo in logins) {
            var provisionUser =
                $"""
                IF NOT EXISTS (SELECT [name] FROM sys.server_principals WHERE [name] = @UserName)
                BEGIN
                    CREATE LOGIN [{loginInfo.Name}] WITH PASSWORD=N'{loginInfo.Password}'
                    , DEFAULT_DATABASE=[master]
                    , CHECK_EXPIRATION=OFF
                    , CHECK_POLICY=OFF
                END
                """;
            queryExecutor.NewQuery(provisionUser).Execute();
        }
    }

    public static void ProvisionManagedSchemas(this IQueryExecutor queryExecutor, string targetDatabaseName, string[] managedSchemas) {
        foreach (var schema in managedSchemas) {
            var provisionSchema = $@"
IF NOT EXISTS (SELECT * FROM [{targetDatabaseName}].sys.schemas WHERE [name] = '{schema}')
BEGIN
	DECLARE @sql NVARCHAR(MAX)
	SET @sql = 'CREATE SCHEMA [{schema}];'
	EXEC [{targetDatabaseName}].dbo.sp_executesql @sql
END";
            queryExecutor.NewQuery(provisionSchema).Execute();
        }
    }

    public static void ProvisionHistoryTable(this IQueryExecutor queryExecutor, string targetDatabaseName, string[] managedSchemas, string migrationHistoryTableName) {
        var provisionHistoryTable = $@"
IF NOT EXISTS (SELECT * FROM [{targetDatabaseName}].sys.tables WHERE [name] = '{migrationHistoryTableName}')
BEGIN
  CREATE TABLE [{targetDatabaseName}].[{managedSchemas?.FirstOrDefault() ?? "dbo"}].[{migrationHistoryTableName}] (
    [Id] uniqueidentifier ROWGUIDCOL NOT NULL PRIMARY KEY CLUSTERED CONSTRAINT DF_{migrationHistoryTableName.ToUpper()}_ID DEFAULT (NEWID()),
    [TimeStamp] bigint NOT NULL,
    [Version] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NOT NULL,
    [Checksum] binary(20) NOT NULL,
    [ExecutionTime] bigint NULL
  );
END";
        queryExecutor.NewQuery(provisionHistoryTable).Execute();
    }
}