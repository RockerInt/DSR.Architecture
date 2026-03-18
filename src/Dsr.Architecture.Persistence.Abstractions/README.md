# Dsr.Architecture.Persistence.Abstractions

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Persistence.Abstractions?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)
![Target Framework](https://img.shields.io/badge/.NET-10.0-512bd4?style=flat-square&logo=dotnet)

`Dsr.Architecture.Persistence.Abstractions` provides the core contracts and interfaces for the persistence layer within the **Dsr.Architecture** ecosystem. It defines the standards for repositories and the Unit of Work pattern, enabling a clean separation between domain logic and specific infrastructure implementations (SQL, NoSQL, Event Store, etc.).

## Key Features

- **Repository Pattern**: Standardized interfaces for Read, Write, and CRUD operations.
- **Specification Pattern Support**: Built-in support for `ISpecification` to decouple query logic from repository implementations.
- **Result-Oriented API**: All operations return `Result<T>` or `Result` to promote explicit error handling and consistent response patterns.
- **Unit of Work**: Coordination of multiple repository operations into a single atomic transaction.
- **Event Sourcing Support**: Specialized interfaces for event-sourced aggregate roots with optimistic concurrency control.
- **Async & Sync Support**: Full support for both asynchronous and synchronous operations across all repository types.

## Interfaces Provided

### Core Repositories

- **`IReadRepository<TId, TAggregate>`**: Focused on data retrieval, supporting specifications, projections, scalars, aggregate existence checks, and pagination.
- **`IWriteRepository<TId, TAggregate>`**: Focused on data modification (Add, Update, Remove) with support for single and range operations.
- **`IRepository<TId, TAggregate>`**: Combines both Read and Write interfaces for full CRUD capabilities.
- **`IEventSourcedRepository<T, TId>`**: Contract for repositories handling event-sourced aggregate roots, managing event streams and snapshots.

### Unit of Work

- **`IUnitOfWork`**: Coordinates multiple repository operations and commits changes as a single transaction using `SaveChangesAsync`.
- **`ITransactionalUnitOfWork`**: Extends `IUnitOfWork` to provide explicit transaction management with `ExecuteInTransactionAsync`.

## Usage Example

### Defining a Repository Interface

```csharp
using Dsr.Architecture.Persistence.Abstractions;
using Dsr.Architecture.Domain.Aggregates;

public interface IOrderRepository : IRepository<Guid, Order>
{
    // Add domain-specific persistence logic here
}
```

### Using Repositories in an Application Service

```csharp
public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Order order, CancellationToken cancellationToken)
    {
        // Add the aggregate to the repository
        var addResult = await _repository.AddAsync(order, cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult;
        }

        // Commit changes through the Unit of Work
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
```

## Implementations

These abstractions are designed to be implemented by infrastructure-specific packages:

- `Dsr.Architecture.Infrastructure.Persistence.EntityFramework`: Implementation using Entity Framework Core.
- `Dsr.Architecture.Infrastructure.Persistence.Mongo`: Implementation using MongoDB.
- `Dsr.Architecture.Infrastructure.Persistence.Dapper`: Implementation using Dapper for lightweight SQL access.

## Installation

```bash
dotnet add package Dsr.Architecture.Persistence.Abstractions
```

## Dependencies

- **[Dsr.Architecture.Domain.Specifications](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.Domain.Specifications)**: Provides the `ISpecification` interface.
- **[Dsr.Architecture.Domain](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.Domain)**: Provides `IAggregateRoot<TId>`, `Result<T>`, and `Result` types (transitive dependency).

## Contributing

Contributions are welcome! Please submit issues or pull requests through the official repository on [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) file for more details.

## Authors

- **Jonathan Jimenez** - *Initial work* - [RockerInt](https://github.com/RockerInt)

## Tags

persistence, abstractions, interfaces, repository, unit-of-work, architecture, clean-architecture, dsr-architecture, dotnet10
