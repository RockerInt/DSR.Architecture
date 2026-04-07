using System.Collections.Concurrent;
using Dsr.Architecture.Domain.Specifications.Enums;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Tracks how SpecCardinality is used across the system.
/// This helps determine which non-List cardinalities are actually
/// used before enforcing them (which would be a behavioral change).
/// </summary>
public static class CardinalityTelemetry
{
    private static readonly ConcurrentDictionary<string, int> _counts = new();

    public static void RecordUsage<TAggregate>(SpecificationResultCardinality cardinality)
    {
        var key = $"{typeof(TAggregate).Name}:{cardinality}";
        _counts.AddOrUpdate(key, 1, (_, v) => v + 1);
    }

    public static IReadOnlyDictionary<string, int> Snapshot() => _counts.ToDictionary();

    public static void LogSnapshot(ILogger logger, string prefix = "Cardinality")
    {
        foreach (var (key, count) in Snapshot())
        {
            logger.LogInformation("{Prefix}: {Key} = {Count} times", prefix, key, count);
        }
    }
}
