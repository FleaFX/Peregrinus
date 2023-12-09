# Peregrinus  ![example workflow](https://github.com/FleaFX/Peregrinus/actions/workflows/build.yml/badge.svg)

Peregrinus is a tool to perform database migrations from within your application. It uses SQL scripts as embedded resources.

# Installation

Peregrinus is available as a nuget package:

```dotnet add package Peregrinus```

# Usage

## Migration scripts
Peregrinus uses plain SQL scripts to perform migrations. The name of the script should follow this naming convention: V\<semver compliant version number\>__\<descriptive name\>.sql.

Valid examples: `V0.1.0__CreateProductsTable.sql`, `V1.2.15.4__Table_CreateProductsTable.sql`, `V2.0.0.0-alpha1__UpdateRecords.sql`.

Notice how you can make your version numbers anything that make sense to you, as long as they are semver compliant and each version number is unique within your list of scripts.

Each migration script can be accompanied by a rollback script. Rollback scripts follow the same convention as migration scripts, but use the prefix `R` instead of `V`. Additionally, they should have the same name as the migration script they are rolling back for Peregrinus to be able to match them up.

Valid example: script `V1.2.15.4__Table_CreateProductsTable.sql` can be rolled back with the script `R1.2.15.4__Table_CreateProductsTable.sql`
Invalid example: scripts `V1.2.15.4__Table_CreateProductsTable.sql` and rollback script `R1.2.15.4__Table_DropProductsTable.sql` don't match up because the name is different.

Rollback scripts are optional. However, if a migration script is not accompanied by a rollback script, that migration will be considered to be a milestone migration and you will not be able to rollback beyond this script. 

You can organize your files in your project anyway you want. You could divide them into subfolders if that makes sense to you. For example:

```
- Migrations
    - v1.0.0
        - V1.1.0__CreateProductsTable.sql
        - V1.2.0__CreateArticlesTable.sql
    - v2.0
        - V2.1.0__UpdateNameColumn.sql
        - V2.2.0__AddManufacturersTable.sql
```

As long as you mark each script file as an embedded resource, Peregrinus will find them and apply them in version order. Just make sure that each version number only occurs once.

## Running the migrations

To run the migrations, use the `Migrator` class and a `AssemblyResourceStreamMigrationProvider` to discover where the scripts are:

```C#
var provider = new AssemblyResourceStreamMigrationProvider(GetType().Assembly);
var migrator = new Migrator(connectionString: "Server=myServerAddress;Database=myDatabase;Integrated Security=True;", targetDatabase: "myDatabase", provider, "configuration", "operational", "logging");
await migrator.Migrate();
```

`connectionString`, `targetDatabase` and `migrationScriptProvider` are required parameters. In the example above, the last three arguments `"configuration"`, `"operational"` and `"logging"` are SQL schema's that your database uses. If these schema's don't exist, they will be created. The first of these schema's will be used to create a `migration_history` table in. If no schema's are defined, the migration history table will be created in a schema called `meta`.