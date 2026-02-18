namespace Dsr.Architecture.Domain.Result;

public interface IResult
{
    ResultStatus Status { get; }
    IEnumerable<string> ErrorMessages { get; }
    Type ValueType { get; }
    object? GetValue();
    string Location { get; }
    bool IsSuccess { get; }
    bool IsFailure => !IsSuccess;
    IEnumerable<Error> Errors { get; }
}