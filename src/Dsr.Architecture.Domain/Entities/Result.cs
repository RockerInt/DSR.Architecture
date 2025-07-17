using Dsr.Architecture.Domain.Interfaces;

namespace Dsr.Architecture.Domain.Entities;

/// <summary>
/// Represents a simple result with a result code and an optional error message.
/// </summary>
/// <param name="resultCode">The result code.</param>
/// <param name="errorMessage">The error message.</param>
public class ResultSimple(int resultCode = 0, string? errorMessage = null) : IResultSimple
{
    /// <summary>
    /// The result code.
    /// </summary>
    public int ResultCode { get; set; } = resultCode;

    /// <summary>
    /// The error message, if any.
    /// </summary>
    public string? ErrorMessage { get; set; } = errorMessage;
}

/// <summary>
/// Represents a result with a content, result code, and an optional error message.
/// </summary>
/// <remarks>
/// Constructor for Result.
/// </remarks>
/// <param name="content">The content of the result.</param>
/// <param name="resultCode">The result code.</param>
/// <param name="errorMessage">The error message.</param>
public class Result(dynamic? content, int resultCode = 0, string? errorMessage = null) : ResultSimple(resultCode, errorMessage), IResult
{
    /// <summary>
    /// The content of the result.
    /// </summary>
    public dynamic? Content { get; set; } = content;
}

/// <summary>
/// Represents a generic result with strongly-typed content, result code, and an optional error message.
/// </summary>
/// <typeparam name="T">The type of the content.</typeparam>
public class Result<T> : Result, IResult<T>
{
    /// <summary>
    /// Constructor for Result with strongly-typed content.
    /// </summary>
    /// <param name="content">The strongly-typed content of the result.</param>
    /// <param name="resultCode">The result code.</param>
    /// <param name="errorMessage">The error message.</param>
    public Result(T? content, int resultCode = 0, string? errorMessage = null)
        : base(content, resultCode, errorMessage)
        => Content = content;

    /// <summary>
    /// The strongly-typed content of the result.
    /// </summary>
    public new T? Content
    {
        get => (T?)base.Content;
        set => base.Content = value;
    }
}
