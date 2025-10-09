# EntityFilesystem
An EntityFramework Filesystem Provider

Adds the ability to store information in files instead of being limited to databases.

FileBaseContext is a EntityFramework Filesystem Provider for Net8+

Works for
- Unit Test - Mocking
- Serverless db persistance, easier than Sqlite (Tables are created for one thing)
- Works cross platform, easy offline persistant data store
  
## Usage

Install nuget package **EntityFilesystem**
```csharp
PM> Install-Package Microsoft.EntityFrameworkCore
PM> Install-Package EntityFilesystem
```

```csharp
// DbStartup.cs
using FileBaseContext.Extensions;

partial void CustomInit(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseFileBaseContextDatabase(databaseName: "DbFolderName");
}
```

## Examples 

Created with [Entity Framework Visual Editor Extension](https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner2022) from Visual Studio Marketplace also get latest extension (.visx) [from GitHub for 2022+]([https://github.com/Opzet/EFDesigner2022-26/tree/master/dist](https://github.com/Opzet/EFDesigner2022-26/blob/master/dist/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)) (e.g 2026 insiders )  - Ill publish to market place when I figure it out.


 - [Ex1_ModelPerson](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex1_ModelPerson)
 - [Ex2_ModelOne2One](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex2_ModelOne2One)
 - [Ex3_ModelOnetoMany](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex3_ModelOnetoMany)
 - [Ex4_ModeManytoMany](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex4_ModeManytoMany)
 - [Ex5_ModelInvoice](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex5_ModelInvoice)
 - [Ex6_Course](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex6_Course)
 - [Ex7_Mvp](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore/Ex7_Mvp)
   
[https://github.com/Opzet/EFDesignerExamples](https://github.com/Opzet/EFDesignerExamples/tree/main/EFCore)


![image](https://github.com/user-attachments/assets/eee0118c-d966-4835-9084-2e8922b9c72a)

**NUGET** package https://www.nuget.org/packages/EntityFilesystem 

https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli

## Benefits
Store tables in file, easy 'Serverless' file system text file serialised ef db persistance

- Easier than Sqlite, just works 
- you don't need a database server
- rapid modeling
- version control supported
- supports all serializable .NET types
- unit tests


## Configure Provider
Powerful file based database provider for Entity Framework Core, easy 'Serverless' file system text file serialised ef db persistance
##### Named database 
```cs
 protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseFileBaseContextDatabase(databaseName: DatabaseName); 
    }
```
##### Custom location
```cs
optionsBuilder.UseFileBaseContextDatabase(location: "C:\Temp\userDb");
```

## Unit testing
If you need to use the provider in unit tests, you can change IFileSystem to MockFileSystem in OnConfiguring method in datacontext class.

```cs
private readonly MockFileSystem _fileSystem;
public DbTestContext(MockFileSystem fileSystem)
{
    _fileSystem = fileSystem;
}

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseFileBaseContextDatabase(DatabaseName, null, services =>
    {
        services.AddMockFileSystem(_fileSystem);
    });
}
```
## History / Forks

File system Entity Frameworks Providers

### a. FileContext by DevMentor 
https://github.com/pmizel/DevMentor.Context.FileContext
Core 2+ 

### b. FileContextCore by morrisjdev
https://github.com/morrisjdev/FileContextCore
Offers Different serializer supported (XML, JSON, CSV, Excel) 
Core 2/3 - last update Aug 2, 2020

### c. FileBaseContext by dualbios
https://github.com/dualbios/FileBaseContext
FileBaseContext is a provider of Entity Framework Core 8 to store database information in files. 
[Current developement: forked from this, adjusted namespace, tweaks, published nuget and added examples] 
Core 8+ 

---


## Custom Init Override

```cs

using System;
using System.Diagnostics;
using System.IO;
using FileBaseContext.Extensions;
using FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CRMDb
{
    public partial class CRMDbEF : DbContext
    {
        public static string DatabaseName = "my_local_db"; // Will create folder \bin\my_local_db and tables.json files
        private static string SchemaVersion = "1.0"; // Update this version when schema changes
        private static string VersionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseName, "schema_version.txt");
        public static string DatabasePath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, DatabaseName);

        partial void CustomInit(DbContextOptionsBuilder optionsBuilder)
        {
        
            if (HasSchemaChanged())
            {
                DeleteOldStore();
                SaveCurrentSchemaVersion();
            }

            string location = AppDomain.CurrentDomain.BaseDirectory;
            
            Debug.WriteLine ($">CRMDbEF : DbContext -> CustomInit, location: '{location}, databaseName: '{DatabaseName}'");
            Console.WriteLine ($">CRMDbEF : DbContext -> CustomInit, location: '{location}, databaseName: '{DatabaseName}'");
            optionsBuilder.UseFileBaseContextDatabase(databaseName: DatabaseName, location: location);

        }

        public void EnsureDatabaseAndSchemaCreated ()
        {
            // Ensure the database directory exists
            Directory.CreateDirectory (DatabasePath);

            // Iterate over all entity types and ensure their corresponding files are created
            foreach (var entityType in Model.GetEntityTypes ())
            {
                string tableName = entityType.GetTableName ();
                string filePath = Path.Combine (DatabasePath, $"{tableName}.json");

                if (!File.Exists (filePath))
                {
                    File.Create (filePath).Dispose (); // Create the file and close it immediately
                    Debug.WriteLine ($"\tCreated schema file for table: {tableName}");
                }
            }
        }

        public bool DatabaseExists ()
        {
            return File.Exists (VersionFilePath);
        }

        private bool HasSchemaChanged()
        {
            if (!File.Exists(VersionFilePath))
            {
                return true;
            }

            string storedVersion = File.ReadAllText(VersionFilePath);
            return !storedVersion.Equals(SchemaVersion, StringComparison.OrdinalIgnoreCase);
        }

        private void DeleteOldStore()
        {
            string contextPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseName);

            if (Directory.Exists(contextPath))
            {
                try
                {
                    Directory.Delete(contextPath, true);
                    Console.WriteLine("Old FileBasedContext store deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the context store: {ex.Message}");
                }
            }
        }

        private void SaveCurrentSchemaVersion()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VersionFilePath));
            File.WriteAllText(VersionFilePath, SchemaVersion);
        }
    }
}
```



