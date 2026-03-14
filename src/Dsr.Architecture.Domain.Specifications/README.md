# DSR.Architecture.Domain.Specifications

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Domain.Specifications?style=flat-square)

Specification pattern library for the DSR.Architecture solution. Provides composable, **persistence-agnostic** query specifications with criteria, includes, ordering, paging, and tracking options. 

The primary goal of this library is to encapsulate query logic within the Domain layer, allowing repositories (Infrastructure layer) to execute these queries against any data source (Entity Framework Core, MongoDB, Dapper, etc.) without exposing the underlying query details or leaking implementation specifics.

## Features

- **ISpecification<TId, TAggregate>**: Core contract defining criteria, includes (expression and string), ordering, paging, and change-tracking preferences.
- **Specification<TId, TAggregate>**: Abstract base class for defining standard query specifications with fluent-like methods: `AddInclude`, `ApplyOrderBy`, `ApplyOrderByDescending`, `ApplyPaging`, and `ApplyAsNoTracking`.
- **IAnalyticsSpecification<TId, TAggregate>**: Extension for analytical queries that require grouping, aggregation, and projection.
- **AnalyticsSpecification<TId, TAggregate>**: Base class for complex analytical queries using `ApplyGroupBy`, `AddAggregation`, `ApplyHaving`, and `ApplyProjection`.
- **Expression Combinators**: `And` and `Or` extension methods to combine multiple predicate expressions dynamically.
- **TrueSpecification / FalseSpecification**: Constant specifications for always-true or always-false criteria.
- **Cardinality Control**: Explicitly define the expected result shape (List, Single, First, Scalar).

## Persistence Agnostic

This library does not depend on any specific ORM or database provider. It relies on standard LINQ `Expression<Func<T, bool>>` and `LambdaExpression` objects, which are supported by most modern data access technologies:

- **Entity Framework Core**: Map specifications directly to `IQueryable<T>` using an executor.
- **MongoDB**: Use expressions to build MongoDB filter definitions.
- **Dapper / SQL**: Translate expressions into raw SQL queries using a custom query builder.
- **In-Memory**: Use `IsSatisfiedBy` or compile expressions to filter standard C# collections.

## Usage

Reference this package in your **Domain** layer to define reusable query logic. Your **Infrastructure** layer will then implement the actual execution logic.

### Standard Specification Example

```csharp
public class ActiveUsersSpecification : Specification<Guid, User>
{
    public ActiveUsersSpecification()
    {
        Criteria = user => user.IsActive;
        AddInclude(user => user.Profile);
        ApplyOrderBy(user => user.LastName);
    }
}
```

### Analytical Specification Example

```csharp
public class SalesByCategorySpecification : AnalyticsSpecification<Guid, Order>
{
    public SalesByCategorySpecification()
    {
        // Filter: only completed orders
        Criteria = order => order.Status == OrderStatus.Completed;

        // Group by category
        ApplyGroupBy(order => order.Category);

        // Aggregations
        AddAggregation(AggregationType.Sum, order => order.TotalAmount, "TotalSales");
        AddAggregation(AggregationType.Count, order => order.Id, "OrderCount");

        // Filter groups: only categories with more than 10 orders
        ApplyHaving<string>(group => group.Count() > 10);
        
        // Define result as a list of analytics objects
        ApplyCardinality(SpecificationResultCardinality.List);
    }
}
```

## Installation

Install via NuGet:

```bash
dotnet add package Dsr.Architecture.Domain.Specifications
```

## Dependencies

- **Dsr.Architecture.Domain**: Core domain primitives and `IAggregateRoot` contract.

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) for details.

## Authors

- Jonathan Jimenez

## Tags

specification-pattern, query, persistence-agnostic, domain-driven-design, architecture, modular, enterprise, dsr-architecture, analytics, aggregation, grouping
