using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Validation;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

/// <summary>
/// Integration tests that exercise <see cref="EFRepository{TContext, TId, TAggregate}"/> and
/// <see cref="ReadEFRepository{TContext, TId, TAggregate}"/> with an
/// <see cref="AnalyticsSpecification{TId, TAggregate}"/> that drives the
/// BuildAnalyticsQuery pipeline across every <see cref="AggregationType"/>
/// (Sum, Count, Avg, Max, Min).
/// </summary>
public class AnalyticsQueryTests
{
    private readonly DbContextOptions<AnalyticsTestDbContext> _options;

    public AnalyticsQueryTests()
    {
        _options = new DbContextOptionsBuilder<AnalyticsTestDbContext>()
            .UseInMemoryDatabase($"analytics_{Guid.NewGuid()}")
            .Options;
    }

    private async Task SeedAsync()
    {
        await using var ctx = new AnalyticsTestDbContext(_options);
        ctx.Sales.AddRange(
            new SalesAggregate(1, "A", 10),
            new SalesAggregate(2, "A", 20),
            new SalesAggregate(3, "B", 15),
            new SalesAggregate(4, "B", 25),
            new SalesAggregate(5, "B", 35));
        await ctx.SaveChangesAsync();
    }

    private static ICompiledSpecificationExecutor CreateExecutor()
        => new AutoCompiledSpecificationExecutor(
            new CompiledQueryCache(),
            new SpecificationAnalysisCache(),
            new FakeComplexityAnalyzer());

    private (ReadEFRepository<AnalyticsTestDbContext, int, SalesAggregate> read,
             EFRepository<AnalyticsTestDbContext, int, SalesAggregate> repo,
             AnalyticsTestDbContext ctx)
        CreateRepositories()
    {
        var ctx = new AnalyticsTestDbContext(_options);
        var uow = Substitute.For<IUnitOfWork<AnalyticsTestDbContext>>();
        uow.Context.Returns(ctx);

        var executor = CreateExecutor();

        var read = new ReadEFRepository<AnalyticsTestDbContext, int, SalesAggregate>(
            uow,
            executor,
            NullLogger<ReadEFRepository<AnalyticsTestDbContext, int, SalesAggregate>>.Instance);

        var repo = new EFRepository<AnalyticsTestDbContext, int, SalesAggregate>(
            uow,
            executor,
            NullLogger<ReadEFRepository<AnalyticsTestDbContext, int, SalesAggregate>>.Instance,
            NullLogger<WriteEFRepository<AnalyticsTestDbContext, int, SalesAggregate>>.Instance);

        return (read, repo, ctx);
    }

    private static AnalyticsSpecification<int, SalesAggregate> BuildGroupedSpec(
        AggregationType type,
        string alias)
    {
        var spec = new AnalyticsSpecification<int, SalesAggregate>();
        spec.GroupBy(x => x.Category);

        if (type == AggregationType.Count)
            spec.AddAggregation(type, x => x.Id, alias);
        else
            spec.AddAggregation(type, x => x.Amount, alias);

        return spec;
    }

    private static string GetKey(dynamic row) => (string)((Type)row.GetType()).GetProperty("Key")!.GetValue(row)!;

    private static T GetValue<T>(dynamic row, string alias)
        => (T)((Type)row.GetType()).GetProperty(alias)!.GetValue(row)!;

    [Fact]
    public async Task ReadEFRepository_ListDynamicAsync_Count_GroupsByCategory()
    {
        await SeedAsync();
        var (read, _, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = BuildGroupedSpec(AggregationType.Count, "Total");
        var result = await read.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var rows = result.Value!.ToList();
        Assert.Equal(2, rows.Count);

        var byCategory = rows.ToDictionary(r => GetKey(r), r => GetValue<int>(r, "Total"));
        Assert.Equal(2, byCategory["A"]);
        Assert.Equal(3, byCategory["B"]);
    }

    [Fact]
    public async Task ReadEFRepository_ListDynamicAsync_Sum_GroupsByCategory()
    {
        await SeedAsync();
        var (read, _, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = BuildGroupedSpec(AggregationType.Sum, "Total");
        var result = await read.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var byCategory = result.Value!.ToDictionary(r => GetKey(r), r => GetValue<int>(r, "Total"));
        Assert.Equal(30, byCategory["A"]);
        Assert.Equal(75, byCategory["B"]);
    }

    [Fact]
    public async Task ReadEFRepository_ListDynamicAsync_Avg_GroupsByCategory()
    {
        await SeedAsync();
        var (read, _, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = BuildGroupedSpec(AggregationType.Avg, "Average");
        var result = await read.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var byCategory = result.Value!.ToDictionary(r => GetKey(r), r => GetValue<double>(r, "Average"));
        Assert.Equal(15d, byCategory["A"], precision: 4);
        Assert.Equal(25d, byCategory["B"], precision: 4);
    }

    [Fact]
    public async Task ReadEFRepository_ListDynamicAsync_Max_GroupsByCategory()
    {
        await SeedAsync();
        var (read, _, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = BuildGroupedSpec(AggregationType.Max, "Peak");
        var result = await read.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var byCategory = result.Value!.ToDictionary(r => GetKey(r), r => GetValue<int>(r, "Peak"));
        Assert.Equal(20, byCategory["A"]);
        Assert.Equal(35, byCategory["B"]);
    }

    [Fact]
    public async Task ReadEFRepository_ListDynamicAsync_Min_GroupsByCategory()
    {
        await SeedAsync();
        var (read, _, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = BuildGroupedSpec(AggregationType.Min, "Floor");
        var result = await read.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var byCategory = result.Value!.ToDictionary(r => GetKey(r), r => GetValue<int>(r, "Floor"));
        Assert.Equal(10, byCategory["A"]);
        Assert.Equal(15, byCategory["B"]);
    }

    [Theory]
    [InlineData(AggregationType.Count)]
    [InlineData(AggregationType.Sum)]
    [InlineData(AggregationType.Avg)]
    [InlineData(AggregationType.Max)]
    [InlineData(AggregationType.Min)]
    public async Task EFRepository_ListDynamicAsync_AllAggregations_ReturnTwoGroups(AggregationType type)
    {
        await SeedAsync();
        var (_, repo, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = BuildGroupedSpec(type, "Value");
        var result = await repo.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var rows = result.Value!.ToList();
        Assert.Equal(2, rows.Count);

        var keys = rows.Select(r => GetKey(r)).OrderBy(k => k).ToArray();
        Assert.Equal(new[] { "A", "B" }, keys);
    }

    [Fact]
    public async Task ReadEFRepository_ListDynamicAsync_CombinedAggregations_ReturnAllMetrics()
    {
        await SeedAsync();
        var (read, _, ctx) = CreateRepositories();
        await using var _ = ctx;

        var spec = new AnalyticsSpecification<int, SalesAggregate>();
        spec.GroupBy(x => x.Category);
        spec.AddAggregation(AggregationType.Count, x => x.Id, "Count");
        spec.AddAggregation(AggregationType.Sum, x => x.Amount, "Sum");
        spec.AddAggregation(AggregationType.Avg, x => x.Amount, "Avg");
        spec.AddAggregation(AggregationType.Max, x => x.Amount, "Max");
        spec.AddAggregation(AggregationType.Min, x => x.Amount, "Min");

        var result = await read.ListDynamicAsync(spec);

        Assert.True(result.IsSuccess);
        var rows = result.Value!.ToList();
        Assert.Equal(2, rows.Count);

        var groupA = rows.Single(r => GetKey(r) == "A");
        var groupB = rows.Single(r => GetKey(r) == "B");

        Assert.Equal(2, GetValue<int>(groupA, "Count"));
        Assert.Equal(30, GetValue<int>(groupA, "Sum"));
        Assert.Equal(15d, GetValue<double>(groupA, "Avg"), precision: 4);
        Assert.Equal(20, GetValue<int>(groupA, "Max"));
        Assert.Equal(10, GetValue<int>(groupA, "Min"));

        Assert.Equal(3, GetValue<int>(groupB, "Count"));
        Assert.Equal(75, GetValue<int>(groupB, "Sum"));
        Assert.Equal(25d, GetValue<double>(groupB, "Avg"), precision: 4);
        Assert.Equal(35, GetValue<int>(groupB, "Max"));
        Assert.Equal(15, GetValue<int>(groupB, "Min"));
    }
}

// ===== Analytics Test Infrastructure =====

public class AnalyticsTestDbContext(DbContextOptions<AnalyticsTestDbContext> options) : DbContext(options)
{
    public DbSet<SalesAggregate> Sales => Set<SalesAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<ValidationCollector>();
        modelBuilder.Ignore<DomainError>();
    }
}

public class SalesAggregate : AggregateRoot<int>
{
    public string Category { get; private set; } = string.Empty;
    public int Amount { get; private set; }

    public SalesAggregate() : base(0) { }

    public SalesAggregate(int id, string category, int amount) : base(id)
    {
        Category = category;
        Amount = amount;
    }
}
