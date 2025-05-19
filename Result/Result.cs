using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace nuv.Result;

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful; 'Error' means it was not successful.
/// </summary>
/// <typeparam name="TValue">Type of the value</typeparam>
/// <typeparam name="TError">Type of the error</typeparam>
[JsonConverter(typeof(ResultConverterFactory))]
public abstract record Result<TValue, TError>
{
    private Result()
    {
    }

    /// <summary>
    /// Implicit conversion from TValue to Result{TValue, TError}
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TValue value) => new Ok(value);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="Value">The value associated with a successful result.</param>
    /// <returns>A new result instance representing a successful operation with the specified value.</returns>
    public sealed record Ok(TValue Value) : Result<TValue, TError>
    {
        /// <summary>
        /// Value held by this Result.
        /// Prefer using <see cref="Result{TValue,TError}.Unwrap"/>
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
    /// Implicit conversion from TError to Result{TValue, TError}
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TError error) => new Error(error);

    /// <summary>
    /// Creates a new result instance representing an error.
    /// </summary>
    /// <param name="Value">The error value associated with the unsuccessful result.</param>
    /// <returns>A result instance encapsulating the provided error value.</returns>
    public sealed record Error(TError Value) : Result<TValue, TError>
    {
        /// <summary>
        /// Value held by this Result.
        /// Prefer using <see cref="Result{TValue,TError}.UnwrapError"/>
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

    #region Functions

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


    /// <summary>
    /// Evaluates the result and invokes the appropriate function based on whether the result contains an 'Ok' or an 'Error'.
    /// </summary>
    /// <param name="ifOk">The function to invoke if the result is 'Ok'.</param>
    /// <param name="ifError">The function to invoke if the result is 'Error'.</param>
    /// <returns>The value obtained by invoking either the 'ifOk' function or the 'ifError' function, based on the result's state.</returns>
    public TResult Match<TResult>(Func<TValue, TResult> ifOk, Func<TError, TResult> ifError)
    {
        return this switch
        {
            Ok ok => ifOk(ok.Value),
            Error error => ifError(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Updates a value held within the 'Ok' of a 'Result' by calling a given function on it.
    /// 
    /// If the 'Result' is an 'Error' rather than 'Ok', the function is not called and the 'Result' stays the same.
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Result<TResult, TError> Map<TResult>(Func<TValue, TResult> action)
    {
        return this switch
        {
            Ok ok => new Result<TResult, TError>.Ok(action(ok.Value)),
            Error error => new Result<TResult, TError>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Applies a specified action to the value of a successful `Result`, returning the original result instance.
    /// 
    /// If the `Result` represents an error, the action is not executed, and the original result is returned.
    /// </summary>
    /// <param name="action">The action to apply to the value of a successful `Result`.</param>
    /// <typeparam name="TValue">The type of the value associated with a successful result.</typeparam>
    /// <typeparam name="TError">The type of the error associated with an unsuccessful result.</typeparam>
    /// <returns>The original `Result` instance, either with the same value or error.</returns>
    public Result<TValue, TError> Map(Action<TValue> action)
    {
        if (this is Ok ok) action(ok.Value);

        return this;
    }

    /// <summary>
    /// Extracts the value from a 'Result', returning a default value if there is none.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public TValue Unwrap(TValue defaultValue)
    {
        return this switch
        {
            Ok ok => ok.Value,
            _ => defaultValue
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
    /// <param name="apply"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public Result<TResult, TError> Try<TResult>(Func<TValue, Result<TResult, TError>> apply)
    {
        return this switch
        {
            Ok ok => apply(ok.Value),
            Error error => new Result<TResult, TError>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Updates a value held within the 'Error' of a result by calling a given function on it.
    /// 
    /// If the result is 'Ok' rather than 'Error' the function won't be called.
    /// But the value will be transferred into a new 'Result' with the same Error type.
    /// Since the returned 'Error' types need to match (C# limitation)
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="TErrorResult"></typeparam>
    /// <returns></returns>
    public Result<TValue, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> action)
    {
        return this switch
        {
            Ok ok => new Result<TValue, TErrorResult>.Ok(ok.Value),
            Error error => new Result<TValue, TErrorResult>.Error(action(error.Value)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Runs an action using the Value held in the 'Error'
    /// 
    /// If the result is 'Ok' rather than 'Error' the action won't be called.
    /// The original result will be returned in both cases.
    /// </summary>
    /// <param name="action"></param>
    /// <returns>The originial Result</returns>
    public Result<TValue, TError> MapError(Action<TError> action)
    {
        if (this is Error error) action(error.Value);

        return this;
    }

    /// <summary>
    /// Extracts the 'Error' value from a result, returning a default value if the result is an 'Ok'.
    /// </summary>
    /// <param name="defaultError"></param>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public TError UnwrapError(TError defaultError)
    {
        return this switch
        {
            Error error => error.Value,
            _ => defaultError,
        };
    }

    #endregion
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
            Result<TValue, TError>.Error error => new Result<TValue, TError>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException()
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