using System;
using System.Collections.Generic;

namespace MUIS_CORE.Wrappers;

// Result    — para commands que solo retornan éxito/fallo (sin valor de retorno).
// Result<T> — para queries y commands que retornan un valor.

public class Result : IResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }

    protected internal Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, [Error.None]);
    public static Result Failure(Error error) => new(false, [error]);
    public static Result Failure(IReadOnlyList<Error> errors) => new(false, errors);

    // Implementación del miembro estático de IResult (C# 11)
    static IResult IResult.Failure(Error error) => Failure(error);

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T> : Result, IResult
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, IReadOnlyList<Error> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, [Error.None]);
    public new static Result<T> Failure(Error error) => new(default, false, [error]);
    public new static Result<T> Failure(IReadOnlyList<Error> errors) => new(default, false, errors);

    // Implementación del miembro estático de IResult
    static IResult IResult.Failure(Error error) => Failure(error);

    // Map: transforma el valor interno si es exitoso; propaga errores si falla.
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
        => IsSuccess
            ? Result<TOut>.Success(mapper(Value!))
            : Result<TOut>.Failure(Errors);

    // Bind: encadena operaciones que retornan Result<T> (flatMap / monadic chaining).
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
        => IsSuccess ? binder(Value!) : Result<TOut>.Failure(Errors);

    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(T value) => Success(value);
}
