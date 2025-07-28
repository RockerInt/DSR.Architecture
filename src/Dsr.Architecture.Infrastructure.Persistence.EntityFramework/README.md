# Dsr.Architecture.Infrastructure.Persistence.EntityFramework

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence.EntityFramework?style=flat-square)

This project provides a EFCore-based implementation of the persistence layer for the DSR.Architecture solution. It includes repository implementations and data access patterns specifically designed for EFCore like SQLite, SQL Server, Oracle, etc.; enabling scalable, modular, and testable enterprise solutions as part of the DSR.Architecture ecosystem.

## Features

- **EFCore Repository Pattern**: Implements generic repositories for CRUD operations and querying entities in EFCore.
- **Integration with Domain Layer**: Works seamlessly with domain entities and value objects from the DSR.Architecture.Domain project.
- **Configuration and Dependency Injection**: Uses Microsoft.Extensions for configuration and dependency injection.

## Usage

Reference this package in your infrastructure projects to implement data access and persistence logic using EFCore for DSR.Architecture-based solutions.

## Installation

Once published, install via NuGet:

```bash
dotnet add package DSR.Architecture.Infrastructure.Persistence.EntityFramework
```

## Dependencies

- DSR.Architecture.Infrastructure.Persistence (project reference)
- DSR.Architecture.TryCatch (project reference)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

infrastructure, persistence, sqlite, repository, data-access, storage, architecture, modular, enterprise, dsr-architecture
