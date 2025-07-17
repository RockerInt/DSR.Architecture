# DSR.Architecture.Infrastructure.Persistence.SqlServer

This project provides a SQL Server-based implementation of the persistence layer for the DSR.Architecture solution. It includes repository implementations and data access patterns specifically designed for SQL Server, enabling scalable, modular, and testable enterprise solutions as part of the DSR.Architecture ecosystem.

## Features

- **SQL Server Repository Pattern**: Implements generic repositories for CRUD operations and querying entities in SQL Server.
- **Integration with Domain Layer**: Works seamlessly with domain entities and value objects from the DSR.Architecture.Domain project.
- **Configuration and Dependency Injection**: Uses Microsoft.Extensions for configuration and dependency injection.

## Usage

Reference this package in your infrastructure projects to implement data access and persistence logic using SQL Server for DSR.Architecture-based solutions.

## Installation

Once published, install via NuGet:

```bash
dotnet add package DSR.Architecture.Infrastructure.Persistence.SqlServer
```

## Dependencies

- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/)
- DSR.Architecture.Domain (project reference)
- DSR.Architecture.Infrastructure.Persistence (project reference)
- DSR.Architecture.Utilities (project reference)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

infrastructure, persistence, sqlserver, repository, data-access, storage, architecture, modular, enterprise, dsr-architecture
