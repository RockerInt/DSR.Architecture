# Dsr.Architecture.Infrastructure.Persistence

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence?style=flat-square)

This is the base library for the persistence layer of the **Dsr.Architecture** solution. It provides the common configurations, settings, and abstractions required to implement data access across the various supported providers within the ecosystem.

It serves as the fundamental base for specific infrastructure implementations:
- **Dsr.Architecture.Infrastructure.Persistence.EntityFramework**: Implementation for relational databases such as **SQL Server, MySQL, PostgreSQL, and SQLite**.
- **Dsr.Architecture.Infrastructure.Persistence.Mongo**: Implementation for **MongoDB**.

## Features

- **Unified Configuration**: Defines `PersistenceSettings` to centrally manage the database provider, database names, and connection strings.
- **Settings Abstraction**: Provides the `IPersistenceSettings` interface to facilitate the injection of persistence configurations.
- **Base Dependency Injection**: Includes the `AddPersistenceServicesBase` extension method to simplify the registration and binding of configurations from .NET's `IConfiguration`.
- **Domain Integration**: Aligned with the abstractions of `Dsr.Architecture.Domain`.

## Configuration

To use this base (usually through one of the implementation packages), you must configure the `PersistenceSettings` section in your `appsettings.json`:

```json
{
  "PersistenceSettings": {
    "DatabaseProvider": "SqlServer", // Options: sqlserver, mysql, postgresql, sqlite, mongodb
    
    "ReadDatabaseName": "MyApplicationReadDb",
    "ConnectionStringName": "DefaultConnection",
    "ReadConnectionStringName": "DefaultReadConnection"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=myServer;Database=myDb;User Id=myUser;Password=myPassword;",
    "DefaultReadConnection": "Server=myServer;Database=myReadDb;User Id=myUser;Password=myPassword;"
  }
}
```

or

```json
{
  "PersistenceSettings": {
    "DatabaseProvider": "PostgreSQL", 
    "DatabaseName": "AppDb",
    "ReadDatabaseName": "AppReadDb",
    "ConnectionString": "Server=myServer;Database=myDb;User Id=myUser;Password=myPassword;",
    "ReadConnectionString": "Server=myServer;Database=myReadDb;User Id=myUser;Password=myPassword;"
  },
}
```

## Installation

Install via NuGet:

```bash
dotnet add package Dsr.Architecture.Infrastructure.Persistence
```

## Dependencies

- [Dsr.Architecture.Domain](https://www.nuget.org/packages/Dsr.Architecture.Domain/) (project reference)  
- [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/)
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/)
- [Microsoft.Extensions.Options](https://www.nuget.org/packages/Microsoft.Extensions.Options/)

## Contributing

Contributions are welcome! Please submit issues or pull requests through the official repository on [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) file for more details.

## Authors

- Jonathan Jimenez

## Tags

infrastructure, persistence, database, repository, data-access, storage, architecture, modular, enterprise, dsr-architecture
