using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nuv.Result;

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful; 'Error' means it was not successful.
/// </summary>
/// <typeparam name="T">Type of the value</typeparam>
/// <typeparam name="TE">Type of the error</typeparam>
[JsonConverter(typeof(ResultConverterFactory))]
public abstract record Result<T, TE>
{
    private Result()
    {
    }

    /// <summary>
    /// Implicit conversion from TValue to Result{TValue, TError}
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Result<T, TE>(T value) => new Ok(value);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="Value">The value associated with a successful result.</param>
    /// <returns>A new result instance representing a successful operation with the specified value.</returns>
    public sealed record Ok(T Value) : Result<T, TE>
    {
        /// <summary>
        /// Value held by this Result.
        /// Prefer using <see cref="Result{TValue,TError}.Unwrap"/>
        /// </summary>
        public T Value { get; set; } = Value;

        /// <summary>
        /// Deconstructs the current instance into its constituent value.
        /// </summary>
        /// <param name="value">The value associated with the successful result.</param>
        public void Deconstruct(out T value)
        {
            value = Value;
        }
    }


    /// <summary>
    /// Implicit conversion from TError to Result{TValue, TError}
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator Result<T, TE>(TE error) => new Error(error);

    /// <summary>
    /// Creates a new result instance representing an error.
    /// </summary>
    /// <param name="Value">The error value associated with the unsuccessful result.</param>
    /// <returns>A result instance encapsulating the provided error value.</returns>
    public sealed record Error(TE Value) : Result<T, TE>
    {
        /// <summary>
        /// Value held by this Result.
        /// Prefer using <see cref="Result{TValue,TError}.UnwrapError"/>
        /// </summary>
        public TE Value { get; set; } = Value;

        /// <summary>
        /// Deconstructs the current instance into its constituent value.
        /// </summary>
        /// <param name="error">The value associated with the successful result.</param>
        public void Deconstruct(out TE error)
        {
            error = Value;
        }
    }
}

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful, 'Error' means it was not successful.
/// </summary>
public static class Result
{
    /// <summary>
    /// A wrapper around <see cref="Result{T,TE}"/>.<see cref="Result{T,TE}.Ok"/> 
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    /// <returns></returns>
    public static Result<T, TE> Ok<T, TE>(T value) => new Result<T, TE>.Ok(value);

    /// <summary>
    /// A wrapper around <see cref="Result{T,TE}"/>.<see cref="Result{T,TE}.Ok"/>
    /// With Exception as the default Error type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Result<T, Exception> Ok<T>(T value) => new Result<T, Exception>.Ok(value);


    /// <summary>
    /// A wrapper around <see cref="Result{T,TE}"/>.<see cref="Result{T,TE}.Error"/> 
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    /// <returns></returns>
    public static Result<T, TE> Error<T, TE>(TE error) => new Result<T, TE>.Error(error);

    /// <summary>
    /// A wrapper around <see cref="Result{T,TE}"/>.<see cref="Result{T,TE}.Error"/>
    /// With Exception as the default Error type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Result<T, Exception> Error<T>(Exception exception) => new Result<T, Exception>.Error(exception);

    /// <summary>
    /// Checks whether the given result represents a successful operation.
    /// </summary>
    /// <returns>True if the result represents a successful operation; otherwise, false.</returns>
    public static bool IsOk<T, TE>(this Result<T, TE> result)
    {
        return result switch
        {
            Result<T, TE>.Ok => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the given result represents an unsuccessful operation.
    /// </summary>
    /// <returns>True if the result represents an unsuccessful operation; otherwise, false.</returns>
    public static bool IsError<T, TE>(this Result<T, TE> result)
    {
        return result switch
        {
            Result<T, TE>.Error => true,
            _ => false
        };
    }


    /// <summary>
    /// Evaluates the result and invokes the appropriate function based on whether the result contains an 'Ok' or an 'Error'.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="ifOk">The function to invoke if the result is 'Ok'.</param>
    /// <param name="ifError">The function to invoke if the result is 'Error'.</param>
    /// <typeparam name="T">Type of the source value</typeparam>
    /// <typeparam name="TE">Type of the Error</typeparam>
    /// <typeparam name="TR">Type of the result value</typeparam>
    /// <returns>The value obtained by invoking either the 'ifOk' function or the 'ifError' function, based on the result's state.</returns>
    public static TR Match<T, TE, TR>(this Result<T, TE> result, Func<T, TR> ifOk, Func<TE, TR> ifError)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => ifOk(ok.Value),
            Result<T, TE>.Error error => ifError(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Executes the provided actions based on whether the result is successful or an error.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="ifOk">The action to be executed if the result is successful (Ok).</param>
    /// <param name="ifError">The action to be executed if the result is an error.</param>
    /// <returns>The current instance of the result.</returns>
    public static Result<T, TE> Match<T, TE>(this Result<T, TE> result, Action<T> ifOk, Action<TE> ifError)
    {
        switch (result)
        {
            case Result<T, TE>.Ok ok:
                ifOk(ok.Value);
                break;
            case Result<T, TE>.Error error:
                ifError(error.Value);
                break;
        }

        return result;
    }
    
    /// <summary>
    /// Evaluates the result and invokes the appropriate function based on whether the result contains an 'Ok' or an 'Error'.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="ifOk">The function to invoke if the result is 'Ok'.</param>
    /// <param name="ifError">The function to invoke if the result is 'Error'.</param>
    /// <typeparam name="T">Type of the source value</typeparam>
    /// <typeparam name="TE">Type of the Error</typeparam>
    /// <typeparam name="TR">Type of the result value</typeparam>
    /// <returns>The value obtained by invoking either the 'ifOk' function or the 'ifError' function, based on the result's state.</returns>
    public static async Task<TR> MatchAsync<T, TE, TR>(this Result<T, TE> result, Func<T, Task<TR>> ifOk, Func<TE, Task<TR>> ifError)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => await ifOk(ok.Value),
            Result<T, TE>.Error error => await ifError(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Updates a value held within the 'Ok' of a 'Result' by calling a given function on it.
    /// 
    /// If the 'Result' is an 'Error' rather than 'Ok', the function is not called and the 'Result' stays the same.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="T">Type of the source value</typeparam>
    /// <typeparam name="TE">Type of the Error</typeparam>
    /// <typeparam name="TR">Type of the result value</typeparam>
    /// <returns></returns>
    public static Result<TR, TE> Map<T, TE, TR>(this Result<T, TE> result, Func<T, TR> action)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => new Result<TR, TE>.Ok(action(ok.Value)),
            Result<T, TE>.Error error => new Result<TR, TE>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Applies a specified action to the value of a successful `Result`, returning the original result instance.
    /// 
    /// If the `Result` represents an error, the action is not executed, and the original result is returned.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action">The action to apply to the value of a successful `Result`.</param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    /// <returns>The original `Result` instance, either with the same value or error.</returns>
    public static Result<T, TE> Map<T, TE>(this Result<T, TE> result, Action<T> action)
    {
        if (result is Result<T, TE>.Ok ok) action(ok.Value);

        return result;
    }
    
    /// <summary>
    /// Updates a value held within the 'Ok' of a 'Result' by calling a given function on it.
    /// 
    /// If the 'Result' is an 'Error' rather than 'Ok', the function is not called and the 'Result' stays the same.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="T">Type of the source value</typeparam>
    /// <typeparam name="TE">Type of the Error</typeparam>
    /// <typeparam name="TR">Type of the result value</typeparam>
    /// <returns></returns>
    public static async Task<Result<TR, TE>> MapAsync<T, TE, TR>(this Result<T, TE> result, Func<T, Task<TR>> action)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => new Result<TR, TE>.Ok(await action(ok.Value)),
            Result<T, TE>.Error error => new Result<TR, TE>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    /// <summary>
    /// Extracts the value from a 'Result', returning a default value if there is none.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T Unwrap<T, TE>(this Result<T, TE> result, T defaultValue)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => ok.Value,
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
    /// <param name="result"></param>
    /// <param name="apply"></param>
    /// <typeparam name="T">Type of the source value</typeparam>
    /// <typeparam name="TE">Type of the Error</typeparam>
    /// <typeparam name="TR">Type of the result value</typeparam>
    /// <returns></returns>
    public static Result<TR, TE> Try<T, TE, TR>(this Result<T, TE> result, Func<T, Result<TR, TE>> apply)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => apply(ok.Value),
            Result<T, TE>.Error error => new Result<TR, TE>.Error(error.Value),
            _ => throw new ArgumentOutOfRangeException()
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
    /// <typeparam name="T">Type of the source value</typeparam>
    /// <typeparam name="TE">Type of the Error</typeparam>
    /// <typeparam name="TR">Type of the result value</typeparam>
    /// <returns></returns>
    public static async Task<Result<TR, TE>> TryAsync<T, TE, TR>(this Result<T, TE> result, Func<T, Task<Result<TR, TE>>> apply)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => await apply(ok.Value),
            Result<T, TE>.Error error => new Result<TR, TE>.Error(error.Value),
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
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <typeparam name="TE">Type of the source error </typeparam>
    /// <typeparam name="TEr">Type of the result error</typeparam>
    /// <returns></returns>
    public static Result<T, TEr> MapError<T, TE, TEr>(this Result<T, TE> result, Func<TE, TEr> action)
    {
        return result switch
        {
            Result<T, TE>.Ok ok => new Result<T, TEr>.Ok(ok.Value),
            Result<T, TE>.Error error => new Result<T, TEr>.Error(action(error.Value)),
            _ => throw new ArgumentOutOfRangeException()
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
    /// <returns>The originial Result</returns>
    public static Result<T, TE> MapError<T, TE>(this Result<T, TE> result, Action<TE> action)
    {
        if (result is Result<T, TE>.Error error) action(error.Value);

        return result;
    }

    /// <summary>
    /// Replaces the error value in a Result instance with a new error value, while keeping the successful value unchanged if applicable.
    /// </summary>
    /// <param name="result">The Result instance to process.</param>
    /// <param name="newError">The new error value to be used if the Result instance represents an error.</param>
    /// <returns>A new Result instance with the error value replaced by <paramref name="newError"/>, or the original success value if the instance represents success.</returns>
    public static Result<T, TEr> ReplaceError<T, TE, TEr>(this Result<T, TE> result, TEr newError)
    {
        return result switch
        {
            Result<T,TE>.Ok ok => new Result<T, TEr>.Ok(ok.Value),
            Result<T,TE>.Error => new Result<T, TEr>.Error(newError),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    /// <summary>
    /// Extracts the 'Error' value from a result, returning a default value if the result is an 'Ok'.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultError"></param>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static TE UnwrapError<T, TE>(this Result<T, TE> result, TE defaultError)
    {
        return result switch
        {
            Result<T, TE>.Error error => error.Value,
            _ => defaultError,
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