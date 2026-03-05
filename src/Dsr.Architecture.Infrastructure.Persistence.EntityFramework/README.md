# DSR.Architecture.Infrastructure.Persistence.EntityFramework

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence.EntityFramework?style=flat-square)

This library provides a robust and scalable persistence layer implementation for the DSR.Architecture framework, built upon Entity Framework Core. It offers a complete data access solution designed to support modern architectural patterns like CQRS and Domain-Driven Design. It includes generic repositories, specialized read/write abstractions, and advanced transaction management for multiple database contexts.

## Features

- **CQRS Support**: Facilitates Command Query Responsibility Segregation with distinct `IReadRepository` (optimized for no-tracking reads) and `IWriteRepository` implementations.
- **Multi-Context Unit of Work**: Includes `MultiContextUnitOfWork` to handle transactions across multiple `DbContext` instances, ensuring consistency in complex scenarios.
- **Generic Repository Pattern**: Implements standard CRUD operations and specification-based querying to reduce boilerplate code.
- **EF Core Integration**: Seamlessly leverages Entity Framework Core features like change tracking, LINQ, and migrations.
- **Database Agnostic**: Compatible with any database provider supported by EF Core (SQL Server, PostgreSQL, SQLite, Oracle, etc.).

## Getting Started

This library provides flexible extension methods in `Dsr.Architecture.Infrastructure.Persistence.EntityFramework.DependencyInjection` to register the necessary services. Choose the setup that best fits your application's needs.

### Scenario 1: Full Persistence (Read & Write)

For applications that need both read and write capabilities through a single repository abstraction (`IRepository`), use `AddFullPersistence`. This is the simplest setup.

```csharp
// In your Program.cs or Startup.cs
services.AddFullPersistence<MyApplicationDbContext>(options => 
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
```

### Scenario 2: Read Only Persistence (Read)

For applications that need the read and write capabilities separated, use `AddReadOnlyPersistence`.

```csharp
// In your Program.cs or Startup.cs
services.AddReadOnlyPersistence<MyApplicationDbContext>(options => 
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
);
```

### Scenario 3: Write Only Persistence (Read)

For applications that need the read and write capabilities separated, use `AddWritePersistence`.

```csharp
// In your Program.cs or Startup.cs
services.AddWritePersistence<MyApplicationDbContext>(options => 
    options.UseMySql(configuration.GetConnectionString("DefaultConnection")));
```

### Scenario 4: Multi-Context Unit of Work (Read & Write)

For applications that need multiples contexts for a *same database*, use `AddTrackedDbContext` for the DbContextAccessor and `AddMultiContextUnitOfWork` for the unit of work and repositories.

```csharp
// In your Program.cs or Startup.cs
services.AddScoped<IDbContextAccessor, ScopedDbContextAccessor>();

services.AddTrackedDbContext<BusinessContext>(options => 
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddTrackedDbContext<OutboxContext>(options => 
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddMultiContextUnitOfWork();
```

## Installation

Install the package via NuGet:

```bash
dotnet add package DSR.Architecture.Infrastructure.Persistence.EntityFramework
```

## Dependencies

- DSR.Architecture.Infrastructure.Persistence (project reference)
- DSR.Architecture.TryCatch (project reference)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

infrastructure, persistence, sqlite, repository, data-access, storage, architecture, modular, enterprise, dsr-architecture
