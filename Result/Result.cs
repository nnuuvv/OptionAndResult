using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful, 'Error' means it was not successful.
/// </summary>
/// <typeparam name="TValue">Type of the value</typeparam>
/// <typeparam name="TError">Type of the error</typeparam>
public record Result<TValue, TError>
{
    /// <summary>
    /// May or may not have a value, check IsOk.
    /// <remarks>
    ///     <para>
    ///     !! Should not be accessed directly outside the Option class
    ///     <para>
    ///     Use <see cref="Result.Unwrap(Result{TValue,TError},x)"/> instead
    ///     </para>
    ///     </para>
    /// </remarks>
    /// </summary>
    public TValue Value { get; set; }

    /// <summary>
    /// Indicates whether Value is set.
    /// <remarks>
    ///     <para>
    ///     !! Should not be accessed directly outside the Result class
    ///     <para>
    ///     Use <see cref="Result.IsOk(Result{TValue,TError})"/> or <see cref="Result.IsError(Result{TValue,TError})"/> instead
    ///     </para>
    ///     </para>
    /// </remarks>
    /// </summary>
    public bool DidSucceed { get; set; }

    /// <summary>
    /// May or may not have a value, check IsOk.
    /// <remarks>
    ///     <para>
    ///     !! Should not be accessed directly outside the Option class
    ///     <para>
    ///     Use <see cref="Result.UnwrapError(Result{TValue,TError},x)"/> instead
    ///     </para>
    ///     </para>
    /// </remarks>
    /// </summary>
    public TError ErrorValue { get; set; }
}

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful, 'Error' means it was not successful.
/// </summary>
public static class Result
{
    public static bool IsOk<TValue, TError>(this Result<TValue, TError> result) => result.DidSucceed;
    public static bool IsError<TValue, TError>(this Result<TValue, TError> result) => !result.DidSucceed;

    public static Result<TValue, TError> Ok<TValue, TError>(TValue value)
    {
        return new Result<TValue, TError>()
        {
            DidSucceed = true,
            Value = value
        };
    }

    public static Result<TValue, TError> Error<TValue, TError>(TError error)
    {
        return new Result<TValue, TError>()
        {
            DidSucceed = false,
            ErrorValue = error
        };
    }

    /// <summary>
    /// Updates a value held within the 'Ok' of a 'Result' by calling a given function on it.
    /// 
    /// If the 'Result' is an 'Error' rather than 'Ok', the function is not called and the 'Result' stays the same.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Result<TResult, TError> Map<TSource, TResult, TError>(this Result<TSource, TError> input,
        Func<TSource, TResult> action)
    {
        return input.IsOk() switch
        {
            true => Ok<TResult, TError>(action(input.Value)),
            false => Error<TResult, TError>(input.ErrorValue),
        };
    }

    /// <summary>
    /// Extracts the value from a 'Result', returning a default value if there is none.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static TValue Unwrap<TValue, TError>(this Result<TValue, TError> result, TValue defaultValue)
    {
        return result.IsOk() switch
        {
            true => result.Value,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Merges a nested 'result' into a single layer.
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Result<TValue, TError> Flatten<TValue, TError>(this Result<Result<TValue, TError>, TError> result)
    {
        return result.IsOk() switch
        {
            true => result.Value,
            _ => Error<TValue, TError>(result.ErrorValue)
        };
    }

    /// <summary>
    /// Updates a value held within the `Ok` of a `Result` by calling a given function
    /// on it, where the given function also returns a `Result`. The two results are
    /// then merged together into one `Result`.
    /// 
    /// If the `Result` is an `Error` rather than `Ok` the function is not called and the original 'Error' is returned.
    /// 
    /// This function is the equivalent of calling `map` followed by `flatten`, and
    /// it is useful for chaining together multiple functions that return `Result`.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="apply"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Result<TResult, TError> Try<TSource, TResult, TError>(this Result<TSource, TError> result,
        Func<TSource, Result<TResult, TError>> apply)
    {
        return result.IsOk() switch
        {
            true => apply(result.Value),
            _ => Error<TResult, TError>(result.ErrorValue)
        };
    }

    /// <summary>
    /// Updates a value held within the 'Error' of a result by calling a given function on it.
    /// 
    /// If the result is 'Ok' rather than 'Error' the function won't be called.
    /// But the value will be transferred into a new 'Result' with the same Error type.
    /// Since the returned 'Error' types need to match (C# limitation)
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TErrorSource"></typeparam>
    /// <typeparam name="TErrorResult"></typeparam>
    /// <returns></returns>
    public static Result<TValue, TErrorResult> MapError<TValue, TErrorSource, TErrorResult>(
        this Result<TValue, TErrorSource> result, Func<TErrorSource, TErrorResult> action)
    {
        return result.IsOk() switch
        {
            true => Ok<TValue, TErrorResult>(result.Value),
            false => Error<TValue, TErrorResult>(action(result.ErrorValue)),
        };
    }

    /// <summary>
    /// Extracts the 'Error' value from a result, returning a default value if the result is an 'Ok'.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultError"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static TError UnwrapError<T, TError>(this Result<T, TError> result, TError defaultError)
    {
        return result.IsOk() switch
        {
            false => result.ErrorValue,
            _ => defaultError,
        };
    }

    /// <summary>
    /// Extracts the inner value from a result. Both the value and error must be of the same type.
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T UnwrapBoth<T>(this Result<T, T> result)
    {
        return result.IsOk() switch
        {
            true => result.Value,
            false => result.ErrorValue
        };
    }

    /// <summary>
    /// Combines a list of results into a single result.
    /// If all elements in the list are 'Ok' then returns an 'Ok' holding the list of values.
    /// If any element is 'Error' then returns the first error.
    /// </summary>
    /// <param name="results"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Result<List<TValue>, TError> All<TValue, TError>(this List<Result<TValue, TError>> results)
    {
        var list = new List<TValue>();

        foreach (var result in results)
        {
            if (result.IsError()) return Error<List<TValue>, TError>(result.ErrorValue);
            list.Add(result.Value);
        }

        return Ok<List<TValue>, TError>(list);
    }

    /// <summary>
    /// Given a list of results, returns a pair where the first element is a list of all the values inside 'Ok' and the second element is a list with all the values inside 'Error'.
    /// </summary>
    /// <param name="results"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static (List<TValue>, List<TError>) Partition<TValue, TError>(this List<Result<TValue, TError>> results)
    {
        var values = new List<TValue>();
        var errors = new List<TError>();

        foreach (var result in results)
        {
            if (result.IsOk()) values.Add(result.Value);
            if (result.IsError()) errors.Add(result.ErrorValue);
        }

        return (values, errors);
    }


    public static List<T> Values<T, TError>(this List<Result<T, TError>> list)
    {
        return list
            .Where(x => x.DidSucceed)
            .Select(x => x.Value)
            .ToList();
    }
}