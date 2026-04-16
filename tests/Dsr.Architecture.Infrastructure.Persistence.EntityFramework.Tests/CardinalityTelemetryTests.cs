using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class CardinalityTelemetryTests
{
    [Fact]
    public void RecordUsage_IncrementsCount()
    {
        CardinalityTelemetry.RecordUsage<TestAggregate>(SpecificationResultCardinality.List);

        var snapshot = CardinalityTelemetry.Snapshot();

        Assert.True(snapshot.ContainsKey("TestAggregate:List"));
        Assert.True(snapshot["TestAggregate:List"] >= 1);
    }

    [Fact]
    public void RecordUsage_MultipleIncrements()
    {
        var before = CardinalityTelemetry.Snapshot().GetValueOrDefault("TestAggregate:First", 0);

        CardinalityTelemetry.RecordUsage<TestAggregate>(SpecificationResultCardinality.First);
        CardinalityTelemetry.RecordUsage<TestAggregate>(SpecificationResultCardinality.First);

        var after = CardinalityTelemetry.Snapshot()["TestAggregate:First"];

        Assert.Equal(before + 2, after);
    }

    [Fact]
    public void Snapshot_ReturnsNonEmptyDictionary()
    {
        CardinalityTelemetry.RecordUsage<TestAggregate>(SpecificationResultCardinality.SingleOrDefault);

        var snapshot = CardinalityTelemetry.Snapshot();

        Assert.NotEmpty(snapshot);
    }

    [Fact]
    public void LogSnapshot_DoesNotThrow()
    {
        CardinalityTelemetry.RecordUsage<TestAggregate>(SpecificationResultCardinality.Single);

        var logger = NullLogger.Instance;
        CardinalityTelemetry.LogSnapshot(logger, "Test");
    }
}
