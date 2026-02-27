# Dsr.Architecture.Persistence.Abstractions

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Persistence.Abstractions?style=flat-square)

This package contains the core abstractions and interfaces for the persistence layer within the **Dsr.Architecture** solution. It provides the contracts required to implement repositories and the Unit of Work pattern, ensuring a clean separation between domain logic and infrastructure implementations.

## Interfaces Provided

- **IUnitOfWork**: Contract for coordinating multiple repository operations and committing changes as a single transaction.
- **IRepository<TAggregate, TId>**: Base contract for standard CRUD operations on domain aggregates.
- **IReadRepository<TAggregate, TId>**: Contract focused on data retrieval operations (read-only).
- **IWriteRepository<TAggregate, TId>**: Contract focused on data modification operations (write-only).
- **IEventSourcedRepository<TAggregate, TId>**: Contract for repositories handling event-sourced aggregate roots.

## Purpose

These abstractions allow the domain and application layers to interact with data persistence without being coupled to specific database technologies. Implementations for these interfaces can be found in sibling packages such as:

- `Dsr.Architecture.Infrastructure.Persistence.EntityFramework`
- `Dsr.Architecture.Infrastructure.Persistence.Mongo`

## Installation

Install via NuGet:

```bash
dotnet add package Dsr.Architecture.Persistence.Abstractions
```

## Dependencies

- [Dsr.Architecture.Domain](https://www.nuget.org/packages/Dsr.Architecture.Domain/)

## Contributing

Contributions are welcome! Please submit issues or pull requests through the official repository on [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) file for more details.

## Authors

- Jonathan Jimenez

## Tags

persistence, abstractions, interfaces, repository, unit-of-work, architecture, clean-architecture, dsr-architecture
