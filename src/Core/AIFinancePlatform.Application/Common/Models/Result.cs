using System.Collections.Generic;

namespace AIFinancePlatform.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public IReadOnlyCollection<string> Errors { get; }

    protected Result(bool isSuccess, string errorMessage, IReadOnlyCollection<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string errorMessage) => new Result(false, errorMessage);
    public static Result Failure(string errorMessage, IReadOnlyCollection<string> errors) => new Result(false, errorMessage, errors);
}

public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool isSuccess, string errorMessage, T? data, IReadOnlyCollection<string>? errors = null) 
        : base(isSuccess, errorMessage, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new Result<T>(true, string.Empty, data);
    public static new Result<T> Failure(string errorMessage) => new Result<T>(false, errorMessage, default);
    public static new Result<T> Failure(string errorMessage, IReadOnlyCollection<string> errors) => new Result<T>(false, errorMessage, default, errors);
}
