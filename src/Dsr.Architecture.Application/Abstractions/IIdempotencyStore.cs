using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Abstraction for storing and retrieving results of idempotent commands.
/// Implementations are responsible for durability and eviction policies.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Gets a previously stored result for the given idempotency key, if it exists.
    /// </summary>
    Task<IResult?> GetAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the result for the given idempotency key.
    /// </summary>
    Task SaveAsync(string idempotencyKey, IResult result, CancellationToken cancellationToken = default);
}

