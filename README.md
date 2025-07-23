# DSR.Architecture

DSR.Architecture is a robust and modular .NET solution designed to facilitate the development of scalable, maintainable, and testable enterprise applications. It promotes a clean architecture approach, separating concerns into distinct layers: Domain, Application, and Infrastructure.

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Domain?style=flat-square&label=Nuget%20Domain)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Application?style=flat-square&label=Nuget%20Application)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence?style=flat-square&label=Nuget%20Infrastructure.Persistence)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence.Mongo?style=flat-square&label=Nuget%20Infrastructure.Persistence.Mongo)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence.SqlLite?style=flat-square&label=Nuget%20Infrastructure.Persistence.SqlLite)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence.SqlServer?style=flat-square&label=Nuget%20Infrastructure.Persistence.SqlServer)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Provider?style=flat-square&label=Nuget%20Infrastructure.Provider)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.TryCatch?style=flat-square&label=Nuget%20TryCatch)
![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Utilities?style=flat-square&label=Nuget%20Utilities)


## Purpose

The primary goal of DSR.Architecture is to provide a solid foundation for building complex business applications by enforcing best practices in software design, such as:

- **Separation of Concerns**: Clear boundaries between business logic, application services, and infrastructure details.
- **Modularity**: Components are loosely coupled, allowing for independent development, testing, and deployment.
- **Testability**: Designed with unit and integration testing in mind, making it easy to verify functionality.
- **Maintainability**: A well-defined structure simplifies understanding and modifying the codebase over time.
- **Extensibility**: Easily integrate new features or change underlying technologies (e.g., different databases) with minimal impact.

## Key Features

- **Domain Layer**: Contains core business entities, value objects, and domain services, independent of any external concerns.
- **Application Layer**: Implements use cases and application-specific business rules, orchestrating interactions between the domain and infrastructure.
- **Infrastructure Layer**: Provides implementations for external concerns such as persistence (SQL Server, SQLite, Mongo), external services, and utilities.
- **Persistence Abstraction**: Supports multiple database technologies through a common repository interface, allowing for flexible data storage choices.
- **Error Handling**: Centralized and consistent error handling mechanisms.

## Project Structure

The repository is organized into the following main directories:

- `src/`: Contains all source code projects, categorized by architectural layer and concern.
  - `Dsr.Architecture.Application/`: Application-specific logic, use cases, and interfaces.
  - `Dsr.Architecture.Domain/`: Core business entities, interfaces, and domain logic.
  - `Dsr.Architecture.Infrastructure.Persistence/`: Common persistence interfaces and base classes.
  - `Dsr.Architecture.Infrastructure.Persistence.Mongo/`: MongoDB specific repository implementations.
  - `Dsr.Architecture.Infrastructure.Persistence.SqlLite/`: SQLite specific repository implementations.
  - `Dsr.Architecture.Infrastructure.Persistence.SqlServer/`: SQL Server specific repository implementations.
  - `Dsr.Architecture.Infrastructure.Provider/`: Implementations for external service integrations.
  - `Dsr.Architecture.TryCatch/`: Utility for robust error handling.
  - `Dsr.Architecture.Utilities/`: General utility functions.
- `test/`: Contains unit and integration tests for the various projects.

## Getting Started

To get started with DSR.Architecture, follow these steps:

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/RockerInt/DSR.Architecture.git
    cd DSR.Architecture
    ```

2.  **Restore NuGet packages:**
    ```bash
    dotnet restore
    ```

3.  **Build the solution:**
    ```bash
    dotnet build
    ```

4.  **Run tests (optional but recommended):**
    ```bash
    dotnet test
    ```

## Contributing

Contributions are welcome! Please refer to the [CONTRIBUTING.md](CONTRIBUTING.md) (if available) for guidelines on how to contribute to this project. You can also submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Authors

- Jonathan Jimenez
