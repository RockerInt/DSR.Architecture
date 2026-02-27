# DSR.Architecture.Application

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Application?style=flat-square)

This project contains the Application layer for the DSR.Architecture solution, designed with **Clean Architecture** principles. It is responsible for implementing use cases, application services, and orchestration of domain logic. By promoting **observability**, **centralized error handling**, and **structured validation** through dedicated behaviors, this layer significantly enhances **testability** and maintains the **Single Responsibility Principle (SRP)**. The Application layer acts as a bridge between the domain model and external interfaces, coordinating business processes in a modular and scalable enterprise architecture.

## Features

- **Use Cases**: Encapsulates business scenarios and workflows, now with enhanced separation of concerns.
- **CQRS markers**: `ICommand<TResponse>` and `IQuery<TResponse>` for explicit command/query separation (write vs read).
- **Transaction behavior**: Commits `IUnitOfWork` after successful command execution (applies only to `ICommand`).
- **Application Services**: Provides services for interacting with the domain layer.
- **Validation Behaviors**: Integrates FluentValidation for input and business rule validation, applied as a behavior to UseCases.
- **Logging Behaviors**: Provides structured logging for UseCase execution.
- **Exception Handling Behaviors**: Centralized exception handling for UseCases, ensuring consistent error responses.
- **Result Handling**: Uses the Domain `Result` / `Result<T>` for consistent and explicit handling of operation outcomes (success, errors, warnings).
- **Orchestration**: Coordinates domain logic and external interactions.
- **Abstractions**: `ICurrentUserService` and `ITimeProviderService` for user context and testable time (implemented by the host).

## Usage

Reference this package in your projects to access application services and use case orchestration for DSR.Architecture-based solutions.

When using **TransactionBehavior** (for `ICommand` use cases), register `IUnitOfWork` in your composition root (e.g. in the Infrastructure or API project). Implement `ICurrentUserService` and `ITimeProviderService` in the host if you need user context or testable time in handlers.

For more patterns and best practices (authorization, idempotency, correlation ID, domain events), see [Application Layer Patterns](../../docs/Application-Layer-Patterns.md) in the repository docs.

## Installation

Once published, install via NuGet:

```bash
dotnet add package DSR.Architecture.Application
```

## Dependencies

- [Ardalis.Result](https://github.com/ardalis/Ardalis.Result)
- [Dsr.Architecture.Domain](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.Domain) (project reference)
- [FluentValidation](https://fluentvalidation.net/)
- [Microsoft.Extensions.Logging.Abstractions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.abstractions)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

application-layer, use-cases, services, validation, orchestration, architecture, enterprise, modular
