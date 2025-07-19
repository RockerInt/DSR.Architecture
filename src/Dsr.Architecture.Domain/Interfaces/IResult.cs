namespace Dsr.Architecture.Domain.Interfaces;

/// <summary>
/// Defines a simple result with a result code and an optional error message.
/// </summary>
public interface IResultSimple
{
    /// <summary>
    /// Gets or sets the result code.
    /// </summary>
    public int ResultCode { get; set; }

    /// <summary>
    /// Gets or sets the error message, if any.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Defines a result with a dynamic content.
/// </summary>
public interface IResult : IResultSimple
{
    /// <summary>
    /// Gets or sets the dynamic content of the result.
    /// </summary>
    public dynamic? Content { get; set; }
}

/// <summary>
/// Defines a generic result with strongly-typed content.
/// </summary>
/// <typeparam name="T">The type of the content.</typeparam>
public interface IResult<T> : IResult
{
    /// <summary>
    /// Gets or sets the strongly-typed content of the result.
    /// </summary>
    public new T? Content { get; set; }
}
