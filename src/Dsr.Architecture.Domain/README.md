# DSR.Architecture.Domain

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Domain?style=flat-square)

Core domain library for the DSR.Architecture solution. Contains the essential building blocks for domain-driven design: entities, value objects, domain events, aggregates, result types, validation, repository abstractions, and specifications. Designed to be modular and scalable as the foundation for enterprise applications built with DSR.Architecture.

## Features

- **Entities**: Core domain objects with identity and lifecycle (`Entity<TId>`, `IEntity<TId>`).
- **Value Objects**: Immutable types representing descriptive aspects of the domain (`ValueObject`).
- **Aggregates**: Aggregate roots and interfaces for consistency boundaries (`IAggregateRoot`, `AggregateRoot<TId>`).
- **Domain Events**: Events that capture significant domain occurrences (`IDomainEvent`, `DomainEvent`, `IDomainEventDispatcher`, `IEventContextAccessor`, `EventMetadata`).
- **Result**: Explicit operation outcomes with success/error/warnings (`IResult`, `Result`, `ResultStatus`, `Error`, `ErrorList`, `PagedResult`).
- **Validation**: Guard clauses and validation collectors with typed errors (`Guard`, `ValidationCollector`, `DomainError`, `ErrorSeverity`, `ErrorType`) and extensions for strings, numerics, dates, and collections.
- **Repository Abstractions**: Interfaces for persistence and unit of work (`IRepository`, `IUnitOfWork`, `IAggregateRepository`, `IEventSourcedRepository`).
- **Specifications**: Query specification pattern with criteria, includes, ordering, paging, and AsNoTracking (`ISpecification<T>`, `Specification<T>`, `SpecificationExpressionExtensions.Apply`).
- **Domain Services**: Abstraction for domain services (`IDomainServices`).
- **Exceptions**: Base domain exception (`DomainException`).

## Usage

Add this package as a dependency to other projects in the DSR.Architecture solution to share domain models, result types, and persistence abstractions.

## Installation

You can install the NuGet package once published:

```bash
dotnet add package Dsr.Architecture.Domain
```

## Contributing

Contributions are welcome! Please open issues or submit pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) file for details.

## Authors

- Jonathan Jimenez

## Tags

domain, entities, value-objects, aggregates, events, result, validation, specifications, architecture, modular, enterprise
