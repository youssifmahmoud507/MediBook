using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public ErrorType ErrorType { get; }

        protected Result(bool isSuccess, string? error , ErrorType errorType = ErrorType.Validation)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorType = errorType;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string error , ErrorType errorType = ErrorType.Validation) => new(false, error, errorType);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value of a failed result.");

        private Result(bool isSuccess, T? value, string? error , ErrorType errorType) : base(isSuccess, error , errorType)
        {
            _value = value;
        }

        public static Result<T> Success(T value) => new(true, value, null ,default);
        public static new Result<T> Failure(string error , ErrorType errorType = ErrorType.Validation) => new(false, default, error, errorType);
    }
}
