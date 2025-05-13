using System;
using System.Collections.Generic;
using System.Linq;

namespace nuv.Result;

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful; 'Error' means it was not successful.
/// </summary>
/// <typeparam name="TValue">Type of the value</typeparam>
/// <typeparam name="TError">Type of the error</typeparam>
public abstract record Result<TValue, TError>
{
    /// <summary>
    /// Implicit conversion from TValue to Result{TValue, TError}
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TValue value) => new Ok(value);

    /// <summary>
    /// Implicit conversion from TError to Result{TValue, TError}
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TError error) => new Error(error);

    private Result()
    {
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="Value">The value associated with a successful result.</param>
    /// <returns>A new result instance representing a successful operation with the specified value.</returns>
    public sealed record Ok(TValue Value) : Result<TValue, TError>
    {
        /// <summary>
        /// May or may not have a value, check IsOk.
        /// <remarks>
        ///     <para>
        ///     !! Should not be accessed directly.
        ///     If you do, make sure to check IsOk() or IsError() to see if this holds a value
        ///     <para>
        ///     Use <see cref="Result"/>.<see cref="Result.Unwrap(Result{TValue,TError},x)"/> instead
        ///     </para>
        ///     </para>
        /// </remarks>
        /// </summary>
        public TValue Value { get; set; } = Value;

        /// <summary>
        /// Deconstructs the current instance into its constituent value.
        /// </summary>
        /// <param name="value">The value associated with the successful result.</param>
        public void Deconstruct(out TValue value)
        {
            value = Value;
        }
    }

    /// <summary>
    /// Checks whether the given result represents a successful operation.
    /// </summary>
    /// <returns>True if the result represents a successful operation; otherwise, false.</returns>
    public bool IsOk()
    {
        return this switch
        {
            Error => false,
            Ok => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Creates a new result instance representing an error.
    /// </summary>
    /// <param name="Value">The error value associated with the unsuccessful result.</param>
    /// <returns>A result instance encapsulating the provided error value.</returns>
    public sealed record Error(TError Value) : Result<TValue, TError>
    {
        /// <summary>
        /// May or may not have a value, check IsOk.
        /// <remarks>
        ///     <para>
        ///     !! Should not be accessed directly.
        ///     If you do, make sure to check IsOk() or IsError() to see if this holds a value
        ///     <para>
        ///     Use <see cref="Result"/>.<see cref="Result.UnwrapError(Result{TValue,TError},x)"/> instead
        ///     </para>
        ///     </para>
        /// </remarks>
        /// </summary>
        public TError Value { get; set; } = Value;

        /// <summary>
        /// Deconstructs the current instance into its constituent value.
        /// </summary>
        /// <param name="error">The value associated with the successful result.</param>
        public void Deconstruct(out TError error)
        {
            error = Value;
        }
    }

    /// <summary>
    /// Determines whether the given result represents an unsuccessful operation.
    /// </summary>
    /// <returns>True if the result represents an unsuccessful operation; otherwise, false.</returns>
    public bool IsError()
    {
        return this switch
        {
            Error => true,
            Ok => false,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful, 'Error' means it was not successful.
/// </summary>
public static class Result
{
    private static void Test()
    {
        var someValue = new Result<string, object>.Ok("hehe");

        WithRetry(() => someValue.Try(x => new Result<int, object>.Ok(x.Length)));
    }

    private static Result<TResult, TError> WithRetry<TResult, TError>(Func<Result<TResult, TError>> functionToRetry,
        int retryCount = 1)
    {
        Result<TResult, TError> result;
        do
        {
            result = functionToRetry();
            if (result.IsOk()) return result;

            retryCount--;
        } while (retryCount > 0);

        return result;
    }

    /// <summary>
    /// A wrapper around <see cref="Result"/>.<see cref="Ok{TValue,TError}"/> for backward compatability
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Result<TValue, TError> Ok<TValue, TError>(TValue value)
    {
        return new Result<TValue, TError>.Ok(value);
    }

    /// <summary>
    /// A wrapper around <see cref="Result"/>.<see cref="Error{TValue,TError}"/> for backward compatability
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Result<TValue, TError> Error<TValue, TError>(TError error)
    {
        return new Result<TValue, TError>.Error(error);
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
        return input switch
        {
            Result<TSource, TError>.Ok ok => new Result<TResult, TError>.Ok(action(ok.Value)),
            Result<TSource, TError>.Error error => new Result<TResult, TError>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }

    /// <summary>
    /// Applies a specified action to the value of a successful `Result`, returning the original result instance.
    /// 
    /// If the `Result` represents an error, the action is not executed, and the original result is returned.
    /// </summary>
    /// <param name="result">The `Result` instance to apply the action to.</param>
    /// <param name="action">The action to apply to the value of a successful `Result`.</param>
    /// <typeparam name="TValue">The type of the value associated with a successful result.</typeparam>
    /// <typeparam name="TError">The type of the error associated with an unsuccessful result.</typeparam>
    /// <returns>The original `Result` instance, either with the same value or error.</returns>
    public static Result<TValue, TError> Map<TValue, TError>(this Result<TValue, TError> result, Action<TValue> action)
    {
        if (result is Result<TValue, TError>.Ok ok) action(ok.Value);

        return result;
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
        return result switch
        {
            Result<TValue, TError>.Ok ok => ok.Value,
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
        return result switch
        {
            Result<TValue, TError>.Ok ok => ok.Value,
            Result<TValue, TError>.Error error => new Result<TValue, TError>.Error(error.Value)
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
        return result switch
        {
            Result<TSource, TError>.Ok ok => apply(ok.Value),
            Result<TSource, TError>.Error error => new Result<TResult, TError>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
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
        return result switch
        {
            Result<TValue, TErrorSource>.Ok ok => new Result<TValue, TErrorResult>.Ok(ok.Value),
            Result<TValue, TErrorSource>.Error error => new Result<TValue, TErrorResult>.Error(action(error.Value)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    /// <summary>
    /// Runs an action using the Value held in the 'Error'
    /// 
    /// If the result is 'Ok' rather than 'Error' the action won't be called.
    /// The original result will be returned in both cases.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns>The originial Result</returns>
    public static Result<TValue, TError> MapError<TValue, TError>(
        this Result<TValue, TError> result, Action<TError> action)
    {
        if (result is Result<TValue, TError>.Error error) action(error.Value);

        return result;
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
        return result switch
        {
            Result<T, TError>.Error error => error.Value,
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
        return result switch
        {
            Result<T, T>.Ok ok => ok.Value,
            Result<T, T>.Error error => error.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
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
            switch (result)
            {
                case Result<TValue, TError>.Error error:
                    return new Result<List<TValue>, TError>.Error(error.Value);
                case Result<TValue, TError>.Ok ok:
                    list.Add(ok.Value);
                    break;
            }
        }

        return new Result<List<TValue>, TError>.Ok(list);
    }

    /// <summary>
    /// Given a list of results, returns a pair where the first element is a list of all the values inside 'Ok' and the second element is a list with all the values inside 'Error'.
    /// </summary>
    /// <param name="results"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static Tuple<List<TValue>, List<TError>> Partition<TValue, TError>(this List<Result<TValue, TError>> results)
    {
        var values = new List<TValue>();
        var errors = new List<TError>();

        foreach (var result in results)
        {
            switch (result)
            {
                case Result<TValue, TError>.Ok ok:
                    values.Add(ok.Value);
                    break;
                case Result<TValue, TError>.Error error:
                    errors.Add(error.Value);
                    break;
            }
        }

        return Tuple.Create(values, errors);
    }


    /// <summary>
    /// Extracts the values from all successful results in the given list.
    /// </summary>
    /// <typeparam name="TValue">The type of value associated with a successful result.</typeparam>
    /// <typeparam name="TError">The type of the error associated with an unsuccessful result.</typeparam>
    /// <param name="list">The list of Result objects to evaluate.</param>
    /// <returns>A list containing the values from all successful results.</returns>
    public static List<TValue> Values<TValue, TError>(this List<Result<TValue, TError>> list)
    {
        return list
            .Select(x =>
            {
                if (x is Result<TValue, TError>.Ok ok) return ok;
                return null;
            })
            .Where(x => x != null)
            // cant be null, just filtered
            .Select(x => x!.Value)
            .ToList();
    }
}