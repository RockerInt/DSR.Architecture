# DSR.Architecture.Infrastructure.Persistence

Infrastructure persistence library for the DSR.Architecture solution. This project provides generic repository interfaces and implementations for data access, supporting multiple storage providers and patterns. It enables modular, scalable, and testable enterprise solutions as part of the DSR.Architecture ecosystem.

## Features

- **Generic Repository Pattern**: Abstractions for CRUD operations and querying entities.
- **Support for Multiple Providers**: Easily extendable to different database/storage technologies.
- **Integration with Domain Layer**: Works seamlessly with domain entities and value objects.
- **Configuration and Dependency Injection**: Uses Microsoft.Extensions for configuration and DI.

## Usage

Reference this package in your infrastructure projects to implement data access and persistence logic for DSR.Architecture-based solutions.

## Installation

Once published, install via NuGet:

```bash
dotnet add package DSR.Architecture.Infrastructure.Persistence
```

## Dependencies

- [Microsoft.Extensions.Configuration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration)
- [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection)
- [Microsoft.Extensions.Options](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.options)
- DSR.Architecture.Domain (project reference)
- DSR.Architecture.Utilities (project reference)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

infrastructure, persistence, repository, data-access, storage, architecture, modular, enterprise,