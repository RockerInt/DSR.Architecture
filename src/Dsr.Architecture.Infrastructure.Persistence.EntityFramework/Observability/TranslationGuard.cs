using Dsr.Architecture.Domain.Result;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Catches EF Core translation failures and logs the full context for debugging.
/// </summary>
public static class TranslationGuard
{
    public static async Task<Result<T>> ExecuteWithGuard<T>(
        Func<Task<T>> operation,
        string description,
        ILogger logger)
    {
        try
        {
            return await operation();
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("could not be translated", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogCritical(ex,
                "EF Core translation failed: {Description}. " +
                "This may indicate an Expression.Invoke or unsupported LINQ expression.",
                description);
            throw;
        }
    }
}
