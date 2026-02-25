namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Marks a command as idempotent and provides the idempotency key used
/// to detect duplicate executions.
/// </summary>
public interface IIdempotentCommand
{
    /// <summary>
    /// Gets the idempotency key that uniquely identifies this command execution.
    /// </summary>
    string IdempotencyKey { get; }
}

