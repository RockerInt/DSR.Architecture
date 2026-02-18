namespace Dsr.Architecture.Domain.Validation;

/// <summary>
/// Provides a fluent API for collecting validation errors and technical errors during the validation process. 
/// This class is used to accumulate errors and determine if the overall validation was successful or if there were 
/// any issues that need to be addressed.
/// </summary>
public static class Guard
{
    public static ValidationCollector For()
        => new();
}