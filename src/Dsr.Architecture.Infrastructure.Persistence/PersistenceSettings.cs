namespace Dsr.Architecture.Infrastructure.Persistence;

/// <summary>
/// Represents the settings for persistence.
/// </summary>
public class PersistenceSettings : IPersistenceSettings
{
    /// <summary>
    /// Gets or sets the database provider.
    /// </summary>
    public string? DatabaseProvider { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database for reading data.
    /// </summary>
    public string? ReadDatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the connection string for reading data.
    /// </summary>
    public string? ReadConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string.
    /// </summary>
    public string? ConnectionStringName { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string for reading data.
    /// </summary>
    public string? ReadConnectionStringName { get; set; }
}

/// <summary>
/// Defines the interface for persistence settings.
/// </summary>
public interface IPersistenceSettings
{
    /// <summary>
    /// Gets or sets the database provider.
    /// </summary>
    string? DatabaseProvider { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database for reading data.
    /// </summary>
    string? ReadDatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the connection string for reading data.
    /// </summary>
    string? ReadConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string.
    /// </summary>
    string? ConnectionStringName { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string for reading data.
    /// </summary>
    string? ReadConnectionStringName { get; set; }
}
