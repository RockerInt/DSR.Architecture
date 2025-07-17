# DSR.Architecture.Application

This project contains the Application layer for the DSR.Architecture solution. It is responsible for implementing use cases, application services, validation, and orchestration of domain logic. The Application layer acts as a bridge between the domain model and external interfaces, coordinating business processes in a modular and scalable enterprise architecture.

## Features

- **Use Cases**: Encapsulates business scenarios and workflows.
- **Application Services**: Provides services for interacting with the domain layer.
- **Validation**: Integrates FluentValidation for input and business rule validation.
- **Orchestration**: Coordinates domain logic and external interactions.

## Usage

Reference this package in your projects to access application services and use case orchestration for DSR.Architecture-based solutions.

## Installation

Once published, install via NuGet:

```bash
dotnet add package DSR.Architecture.Application
```

## Dependencies

- [FluentValidation](https://fluentvalidation.net/)
- [Microsoft.Extensions.Logging.Abstractions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.abstractions)
- DSR.Architecture.Domain (project reference)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

application-layer, use-cases, services, validation, orchestration, architecture, enterprise, modular,