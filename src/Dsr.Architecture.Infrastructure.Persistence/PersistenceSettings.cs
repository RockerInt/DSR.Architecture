namespace Dsr.Architecture.Infrastructure.Persistence;

/// <summary>
/// Represents the settings for persistence.
/// </summary>
public class PersistenceSettings : IPersistenceSettings
{
    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string.
    /// </summary>
    public string? ConnectionStringName { get; set; }
}

/// <summary>
/// Defines the interface for persistence settings.
/// </summary>
public interface IPersistenceSettings
{
    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string.
    /// </summary>
    public string? ConnectionStringName { get; set; }
}
