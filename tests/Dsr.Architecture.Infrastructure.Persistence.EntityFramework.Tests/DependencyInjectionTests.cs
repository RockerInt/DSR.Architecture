using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddCompiledQueriesPersistence_DefaultFlags_RegistersLoggingExecutor()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var executor = scope.ServiceProvider.GetRequiredService<ICompiledSpecificationExecutor>();
        Assert.NotNull(executor);
        // Default pipeline is LoggingSpecificationExecutor wrapping AutoCompiledSpecificationExecutor
        Assert.IsType<LoggingSpecificationExecutor>(executor);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_CanaryFlag_RegistersCanaryExecutor()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureFlags:Persistence:UseNewSpecificationEvaluator"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence(config);

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var executor = scope.ServiceProvider.GetRequiredService<ICompiledSpecificationExecutor>();
        Assert.IsType<CanarySpecificationExecutor>(executor);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_ShadowFlag_RegistersShadowExecutor()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureFlags:Persistence:ShadowModeEnabled"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence(config);

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var executor = scope.ServiceProvider.GetRequiredService<ICompiledSpecificationExecutor>();
        Assert.IsType<ShadowSpecificationExecutor>(executor);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_BoundedCacheFlag_RegistersBoundedCache()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureFlags:Persistence:UseBoundedCache"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence(config);

        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<CompiledQueryCache>();
        Assert.NotNull(cache);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_RegistersFeatureFlags()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();

        var sp = services.BuildServiceProvider();
        var flags = sp.GetRequiredService<PersistenceFeatureFlags>();

        Assert.NotNull(flags);
        Assert.False(flags.UseBoundedCache);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_RegistersEvaluator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var evaluator = scope.ServiceProvider.GetRequiredService<ISpecificationEvaluator>();
        Assert.NotNull(evaluator);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_RegistersAnalyzer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();

        var sp = services.BuildServiceProvider();
        var analyzer = sp.GetRequiredService<ISpecificationComplexityAnalyzer>();

        Assert.NotNull(analyzer);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_NullConfig_DefaultFlags()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence(null);

        var sp = services.BuildServiceProvider();
        var flags = sp.GetRequiredService<PersistenceFeatureFlags>();

        Assert.False(flags.ShadowModeEnabled);
        Assert.False(flags.UseNewSpecificationEvaluator);
    }

    [Fact]
    public void AddCompiledQueriesPersistence_ShadowSampleRate_Parsed()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureFlags:Persistence:ShadowSampleRate"] = "0.25"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence(config);

        var sp = services.BuildServiceProvider();
        var flags = sp.GetRequiredService<PersistenceFeatureFlags>();

        Assert.Equal(0.25, flags.ShadowSampleRate);
    }

    [Fact]
    public void AddUnifiedPersistence_RegistersExpectedDescriptors()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();
        services.AddUnifiedPersistence<TestDbContext>(opt =>
            opt.UseInMemoryDatabase($"test_{Guid.NewGuid()}"));

        Assert.Contains(services, s => s.ServiceType == typeof(IUnitOfWork<TestDbContext>));
    }

    [Fact]
    public void AddReadOnlyPersistence_RegistersExpectedDescriptors()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();
        services.AddReadOnlyPersistence<TestDbContext>(opt =>
            opt.UseInMemoryDatabase($"test_{Guid.NewGuid()}"));

        Assert.Contains(services, s => s.ServiceType == typeof(IUnitOfWork<TestDbContext>));
    }

    [Fact]
    public void AddWritePersistence_RegistersExpectedDescriptors()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompiledQueriesPersistence();
        services.AddWritePersistence<TestDbContext>(opt =>
            opt.UseInMemoryDatabase($"test_{Guid.NewGuid()}"));

        Assert.Contains(services, s => s.ServiceType == typeof(IUnitOfWork<TestDbContext>));
    }
}
