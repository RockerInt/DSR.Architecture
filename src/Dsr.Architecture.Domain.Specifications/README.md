# DSR.Architecture.Domain.Specifications

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Domain.Specifications?style=flat-square)

Specification pattern library for the DSR.Architecture solution. Provides composable query specifications with criteria, includes, ordering, paging, and tracking options. Designed to work with `IQueryable&lt;T&gt;` (e.g. Entity Framework Core) so repositories can apply specifications without exposing query details.

## Features

- **ISpecification&lt;T&gt;**: Contract for criteria, includes (expression and string), order by, take/skip, and AsNoTracking.
- **Specification&lt;T&gt;**: Base class to define specifications with `AddInclude`, `ApplyOrderBy`, `ApplyOrderByDescending`, `ApplyPaging`, and `ApplyAsNoTracking`.
- **Expression combinators**: `And` and `Or` extension methods to combine predicate expressions.
- **TrueSpecification / FalseSpecification**: Constant specifications for always-true or always-false criteria.

Implementations (e.g. in Infrastructure.Persistence.EntityFramework) can apply a specification to an `IQueryable&lt;T&gt;` using its Criteria, Includes (with EF Core `Include`), IncludeStrings, OrderBy, Take/Skip, and AsNoTracking.

## Usage

Reference this package when you need reusable, composable query logic that works with `IQueryable&lt;T&gt;`. Typically used from infrastructure (e.g. EF Core repositories) that reference both this package and `Microsoft.EntityFrameworkCore`.

## Installation

Once published, install via NuGet:

```bash
dotnet add package Dsr.Architecture.Domain.Specifications
```

## Dependencies

- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/) (for `Include` and query extensions)

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

specification-pattern, query, entity-framework, architecture, modular, enterprise, dsr-architecture
