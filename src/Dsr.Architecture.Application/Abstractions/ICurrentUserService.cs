namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Provides the current user context for the request. Implemented by the host (e.g. API) using claims or JWT.
/// Use in use case handlers for authorization and audit (who performed the action).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's unique identifier, or null if not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's name or display name, or null if not available.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the value of a claim by type, or null if not present.
    /// </summary>
    string? GetClaim(string type);

    /// <summary>
    /// Gets all values for a claim type (e.g. roles).
    /// </summary>
    IReadOnlyList<string> GetClaimValues(string type);
}
