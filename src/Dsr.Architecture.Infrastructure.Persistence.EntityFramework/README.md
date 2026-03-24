# Dsr.Architecture.Infrastructure.Persistence.EntityFramework

![NuGet Version](https://img.shields.io/nuget/v/Dsr.Architecture.Infrastructure.Persistence.EntityFramework?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)
![Target Framework](https://img.shields.io/badge/.NET-10.0-512bd4?style=flat-square&logo=dotnet)

`Dsr.Architecture.Infrastructure.Persistence.EntityFramework` provides the Entity Framework Core implementation of the persistence abstractions defined in the **Dsr.Architecture** ecosystem. It offers a production-ready data access layer that supports Domain-Driven Design (DDD), CQRS, and advanced performance optimizations through an intelligent auto-compiled query engine.

## Key Features

- **Full Persistence Support**: Concrete implementations for `IRepository`, `IReadRepository`, and `IWriteRepository`.
- **Intelligent Compiled Queries**: Automated system that analyzes specification complexity to decide when to use EF Core compiled queries for maximum performance.
- **Multi-Context Unit of Work**: Orchestrate transactions across multiple `DbContext` instances sharing the same database connection.
- **CQRS Optimized**: Specialized `ReadEFRepository` that defaults to no-tracking for improved read performance.
- **Result-Pattern Integration**: Seamlessly handles database operations and returns `Result<T>` or `Result` to maintain clean, exception-free application logic.
- **Advanced Analytics Engine**: Perform complex data aggregations and grouping directly in the database using the `AnalyticsSpecification`. Supports `GroupBy`, `Sum`, `Count`, `Average`, `Max`, `Min`, and `Having` clauses, all translated to efficient SQL.
- **Extensible DI**: Fluent registration methods to set up the entire persistence stack with a single line of code.
- **Database Agnostic**: Compatible with any database provider supported by EF Core (SQL Server, PostgreSQL, SQLite, Oracle, etc.).

## Components Provided

### Repositories

- **`EFRepository<TContext, TId, TAggregate>`**: A composite repository for full CRUD operations, delegating to specialized read and write implementations.
- **`ReadEFRepository<TContext, TId, TAggregate>`**: Optimized for read operations, utilizing the compiled query engine and `NoTracking` by default.
- **`WriteEFRepository<TContext, TId, TAggregate>`**: Specialized for data modifications (Add, Update, Remove) with built-in existence and null checks.

### Unit of Work

- **`UnitOfWork<TContext>`**: Standard Unit of Work for managing a single `DbContext` lifecycle and atomic save operations.
- **`MultiContextUnitOfWork`**: Advanced implementation for scenarios requiring atomicity across different database contexts.

### Performance Engine (Auto-Compiled Queries)

- **`AutoCompiledSpecificationExecutor`**: The core engine that analyzes specifications, generates structural fingerprints, and caches compiled execution plans.
- **`SpecificationComplexityAnalyzer`**: Evaluates query "shape" (includes, criteria, ordering) to ensure compiled queries are used only where they provide a measurable benefit (simple to medium complexity queries).
- **`CompiledQueryCache`**: A thread-safe cache for storing compiled delegates, bypassing LINQ translation overhead on subsequent executions.

## Usage & Scenarios

### Scenario 1: Unified Persistence (Read & Write)

```csharp
// Registration
services.AddUnifiedPersistence<MyDbContext>(options => 
    options.UseSqlite(connectionString));

// Usage in Service
public class ProductService(IRepository<Guid, Product> repository, IUnitOfWork<MyDbContext> uow)
{
    public async Task<Result> CreateProduct(Product product)
    {
        var result = await repository.AddAsync(product);
        if (result.IsSuccess) await uow.SaveChangesAsync();
        return result;
    }
}
```

### Scenario 2: Optimized Read-Only Access

```csharp
// Registration
services.AddReadOnlyPersistence<ReadDbContext>(options => 
    options.UseSqlServer(connectionString, o => o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)));

// Usage
public class ProductQueryService(IReadRepository<Guid, Product> readRepository)
{
    public async Task<Result<IEnumerable<Product>>> GetAllActive()
        => await readRepository.ListAsync(new ActiveProductsSpec());
}
```

### Scenario 3: Write-Only Persistence

Focuses exclusively on data modification operations, ideal for the command side of a CQRS system.

```csharp
// Registration
services.AddWritePersistence<WriteDbContext>(options => 
    options.UseNpgsql(connectionString));

// Usage
public class CreateProductHandler(IWriteRepository<Guid, Product> repository, IUnitOfWork<WriteDbContext> uow)
{
    public async Task<Result<Product>> Handle(Product product)
    {
        var result = await repository.AddAsync(product);
        if (result.IsSuccess) await uow.SaveChangesAsync();
        return result;
    }
}
```

### Scenario 4: Multi-Context Transactions

Allows managing transactions across different contexts (e.g., Business Context and Outbox Context).

> [!WARNING]
> **Multi-Context Unit of Work Requirement**  
> This pattern **only works** when all involved `DbContext` instances use the **exact same ConnectionString**. The engine shares the underlying `DbTransaction` across contexts, which is only supported by EF Core when they share the same physical connection.

```csharp
// Registration
services.AddScoped<IDbContextAccessor, ScopedDbContextAccessor>();
services.AddTrackedDbContext<BusinessDbContext>(o => o.UseSqlServer(sharedConn));
services.AddTrackedDbContext<OutboxDbContext>(o => o.UseSqlServer(sharedConn));
services.AddMultiContextUnitOfWork();

// Usage
public class TransactionalHandler(ITransactionalUnitOfWork unitOfWork)
{
    public async Task Handle()
    {
        await unitOfWork.ExecuteInTransactionAsync(async (ct) => {
            // All operations here share the same transaction
        });
    }
}
```

### Scenario 5: Advanced Analytics with Grouping and Aggregations

Execute complex analytical queries for reporting and business intelligence.

```csharp
// Usage
public class SalesReportService(IReadRepository<Guid, Order> repository)
{
    public async Task<Result<IEnumerable<dynamic>>> GetSalesByCustomer()
    {
        var spec = new AnalyticsSpecification<Guid, Order>()
            .Where(o => o.Status == OrderStatus.Completed)
            .GroupBy(o => o.CustomerId)
            .AddAggregation(AggregationType.Sum, o => o.Total, "TotalSales")
            .AddAggregation(AggregationType.Count, o => o.Id, "OrderCount");

        return await repository.ListDynamicAsync(spec);
    }
}
```

### Scenario 6: Analytics with Filtering and Post-Aggregation Filtering (`Having`)

This scenario demonstrates a more complex analytical query that calculates aggregated sales data for customers who have spent over a certain threshold.

```csharp
// Usage
public class HighValueCustomerReport(IReadRepository<Guid, Order> repository)
{
    public async Task<Result<IEnumerable<dynamic>>> GetHighValueCustomers()
    {
        var spec = new AnalyticsSpecification<Guid, Order>()
            .Where(o => o.OrderDate > new DateTime(2023, 1, 1))
            .GroupBy(o => o.CustomerId)
            .AddAggregation(AggregationType.Sum, o => o.Total, "TotalSpent")
            .AddAggregation(AggregationType.Count, o => o.Id, "TotalOrders")
            .Having(group => group.Sum(o => o.Total) > 1000); // Post-aggregation filter

        return await repository.ListDynamicAsync(spec);
    }
}
```

This example filters orders after a certain date, groups them by customer, calculates the total amount spent and the number of orders, and finally, only returns customers who have spent more than 1000.

## Functional Result Pattern

All repository operations return a `Result<T>` or `Result` object. This pattern ensures that:

1. **Exceptions are handled** within the infrastructure layer and converted to failure results.
2. **Business logic remains clean** and doesn't need to use try-catch for expected database issues.
3. **Explicit state handling**: Developers must check `IsSuccess` before accessing the `Value`.

## Performance Optimization

The library includes an **Auto-Compiled Query Engine** to bypass LINQ-to-SQL translation overhead:

1. **Fingerprinting**: Generates a structural key based on the query shape (criteria, includes, ordering).
2. **Analysis**: Evaluates if the query complexity warrants compilation.
3. **Caching**: Stores the compiled delegate in a thread-safe cache.
4. **Execution**: Subsequent calls with the same specification "shape" execute the compiled version directly.

> [!NOTE]
> **Analytics and Compiled Queries**
> While the auto-compiled query engine supports analytics queries, it is designed to optimize for simple to medium complexity specifications. An `AnalyticsSpecification` with a `GroupBy` and a single aggregation is typically eligible for compilation. However, adding more aggregations or a `Having` clause increases the complexity score, and the engine may opt for direct execution to avoid the overhead of compiling highly complex queries. This ensures that you get the best performance for a wide range of query types.

## Installation

```bash
dotnet add package Dsr.Architecture.Infrastructure.Persistence.EntityFramework
```

## Dependencies

- **[Dsr.Architecture.Persistence.Abstractions](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.Persistence.Abstractions)**: Core interfaces and contracts for the persistence layer.
- **[Dsr.Architecture.Domain.Specifications](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.Domain.Specifications)**: Provides the specification pattern support for complex queries.
- **[Dsr.Architecture.Domain](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.Domain)**: Core domain types including `IAggregateRoot<TId>`, `Result<T>`, and `Result`.
- **[Dsr.Architecture.TryCatch](https://github.com/RockerInt/DSR.Architecture/tree/main/src/Dsr.Architecture.TryCatch)**: Functional extensions for error handling and safe execution.
- **Microsoft.EntityFrameworkCore.Relational**: Essential for relational (SQL) database support, including providers for SQL Server, PostgreSQL, MySQL, SQLite, Oracle, and others.

## Contributing

Contributions are welcome! Please submit issues or pull requests via [GitHub](https://github.com/RockerInt/DSR.Architecture).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/RockerInt/DSR.Architecture/blob/main/LICENSE) for details.

## Authors

- **Jonathan Jimenez** - *Initial work* - [RockerInt](https://github.com/RockerInt)

## Tags

infrastructure, persistence, entity-framework, repository, unit-of-work, cqrs, ddd, compiled-queries, performance, dsr-architecture, dotnet10
