namespace Dsr.Architecture.Infrastructure.Persistence;

public class PersistenceSettings : IPersistenceSettings
{
    public string? ConnectionString { get; set; }
}

public interface IPersistenceSettings
{
    string? ConnectionString { get; set; }
}
