namespace Dsr.Architecture.Infrastructure.Persistence;

public class PersistenceSettings : IPersistenceSettings
{
    public string? DatabaseName { get; set; }

    public string? ConnectionString { get; set; }
}

public interface IPersistenceSettings
{
    string? DatabaseName { get; set; }

    string? ConnectionString { get; set; }
}
