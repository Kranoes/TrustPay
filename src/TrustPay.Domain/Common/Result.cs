using System;

namespace TrustPay.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Успешный результат не может содержать ошибку.");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("Ошибочный результат должен содержать ошибку.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);
        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
        public static implicit operator Result(Error error) => Failure(error);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Нельзя получить значение у ошибочного результата.");

        protected Result(T? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public static Result<T> Success(T value) => new(value, true, Error.None);

        public new static Result<T> Failure(Error error) => new(default, false, error);
        public new static Result<T> Failure(string error) => new(default, false, error);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);
    }
}