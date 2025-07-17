namespace Dsr.Architecture.Domain.Interfaces;

public interface IResultSimple
{
    public int ResultCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IResult : IResultSimple
{
    public dynamic? Content { get; set; }
}

public interface IResult<T> : IResult
{
    public new T? Content { get; set; }
}