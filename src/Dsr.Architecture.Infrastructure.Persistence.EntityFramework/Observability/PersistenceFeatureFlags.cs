namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Feature flags for the persistence layer migration.
/// All flags default to false for zero-impact deployment.
/// Toggle via configuration: appsettings.json or environment variables.
///
/// Environment variable overrides (dotnet convention: double-underscore):
///   PERSISTENCE_FF__USEBOUNDEDCACHE=true
///   PERSISTENCE_FF__SHADOWMODEENABLED=true
///   PERSISTENCE_FF__USENEWSPECIFICATIONEVALUATOR=true
///   PERSISTENCE_FF__ENFORCESPECCARDINALITY=true
///   PERSISTENCE_FF__ENABLECANONICALCACHEKEYS=true
/// </summary>
public sealed class PersistenceFeatureFlags
{
    /// <summary>
    /// Use Bounded IMemoryCache instead of ConcurrentDictionary for compiled queries.
    /// Risk: compiled queries may expire and recompile, causing latency spikes.
    /// </summary>
    public bool UseBoundedCache { get; set; } = false;

    /// <summary>
    /// Canonicalize expression constant values in cache keys.
    /// Risk: CRITICAL -- changes all cache keys simultaneously.
    /// </summary>
    public bool EnableCanonicalCacheKeys { get; set; } = false;

    /// <summary>
    /// Use the new SpecificationEvaluator instead of AutoCompiledSpecificationExecutor.
    /// Risk: HIGH -- replaces the entire execution pipeline.
    /// </summary>
    public bool UseNewSpecificationEvaluator { get; set; } = false;

    /// <summary>
    /// Enforce SpecCardinality in terminal operations (First/Single vs FirstOrDefault).
    /// Risk: BREAKING -- changes behavior for specs that set cardinality.
    /// </summary>
    public bool EnforceSpecCardinality { get; set; } = false;

    /// <summary>
    /// Run new pipeline in shadow mode alongside old pipeline.
    /// The new pipeline executes in background and results are compared,
    /// but the OLD pipeline result is always returned to the caller.
    /// </summary>
    public bool ShadowModeEnabled { get; set; } = false;

    /// <summary>
    /// Shadow mode sample rate (0.0 to 1.0).
    /// 1.0 = every request is shadow-compared.
    /// 0.01 = 1% sampled (useful for high-traffic production).
    /// </summary>
    public double ShadowSampleRate { get; set; } = 1.0;
}
