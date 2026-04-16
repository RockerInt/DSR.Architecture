using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class TranslationGuardTests
{
    [Fact]
    public async Task ExecuteWithGuard_Success_ReturnsResult()
    {
        var logger = NullLogger.Instance;

        var result = await TranslationGuard.ExecuteWithGuard(
            () => Task.FromResult(42),
            "test operation",
            logger);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task ExecuteWithGuard_TranslationFailure_Throws()
    {
        var logger = NullLogger.Instance;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TranslationGuard.ExecuteWithGuard<int>(
                () => throw new InvalidOperationException("The LINQ expression could not be translated"),
                "test operation",
                logger));
    }

    [Fact]
    public async Task ExecuteWithGuard_NonTranslationError_Throws()
    {
        var logger = NullLogger.Instance;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TranslationGuard.ExecuteWithGuard<int>(
                () => throw new InvalidOperationException("Some other error"),
                "test operation",
                logger));
    }
}
