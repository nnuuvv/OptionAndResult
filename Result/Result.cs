using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nuv.Result;

/// <summary>
/// Result represents the result of something that may succeed or not.
/// 'Ok' means it was successful; 'Error' means it was not successful.
/// </summary>
/// <typeparam name="OK">Type of the value</typeparam>
/// <typeparam name="ERROR">Type of the error</typeparam>
[JsonConverter(typeof(ResultConverterFactory))]
public abstract record Result<OK, ERROR>
{
    private Result()
    {
    }

    /// <summary>
    /// Implicit conversion from TValue to Result{TValue, TError}
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Result<OK, ERROR>(OK value) => new Ok(value);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="Value">The value associated with a successful result.</param>
    /// <returns>A new result instance representing a successful operation with the specified value.</returns>
    public sealed record Ok(OK Value) : Result<OK, ERROR>
    {
        /// <summary>
        /// Value held by this Result.
        /// Prefer using <see cref="Result.Unwrap{T, TE}"/>
        /// </summary>
        public OK Value { get; set; } = Value;

        /// <summary>
        /// Deconstructs the current instance into its constituent value.
        /// </summary>
        /// <param name="value">The value associated with the successful result.</param>
        public void Deconstruct(out OK value)
        {
            value = Value;
        }
    }


    /// <summary>
    /// Implicit conversion from TError to Result{TValue, TError}
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator Result<OK, ERROR>(ERROR error) => new Error(error);

    /// <summary>
    /// Creates a new result instance representing an error.
    /// </summary>
    /// <param name="Value">The error value associated with the unsuccessful result.</param>
    /// <returns>A result instance encapsulating the provided error value.</returns>
    public sealed record Error(ERROR Value) : Result<OK, ERROR>
    {
        /// <summary>
        /// Value held by this Result.
        /// Prefer using <see cref="Result.UnwrapError{TValue,TError}"/>
        /// </summary>
        public ERROR Value { get; set; } = Value;

        /// <summary>
        /// Deconstructs the current instance into its constituent value.
        /// </summary>
        /// <param name="error">The value associated with the successful result.</param>
        public void Deconstruct(out ERROR error)
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
    /// A wrapper around <see cref="Result{OK,ERROR}"/>.<see cref="Result{OK,ERROR}.Ok"/> 
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    public static Result<OK, ERROR> Ok<OK, ERROR>(OK value) => new Result<OK, ERROR>.Ok(value);

    /// <summary>
    /// A wrapper around <see cref="Result{OK,ERROR}"/>.<see cref="Result{OK,ERROR}.Ok"/>
    /// With Exception as the default Error type
    /// </summary>
    /// <typeparam name="OK"></typeparam>
    /// <returns></returns>
    public static Result<OK, Exception> Ok<OK>(OK value) => new Result<OK, Exception>.Ok(value);


    /// <summary>
    /// A wrapper around <see cref="Result{OK,ERROR}"/>.<see cref="Result{OK,ERROR}.Error"/> 
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    public static Result<OK, ERROR> Error<OK, ERROR>(ERROR error) => new Result<OK, ERROR>.Error(error);

    /// <summary>
    /// A wrapper around <see cref="Result{OK,ERROR}"/>.<see cref="Result{OK,ERROR}.Error"/>
    /// With Exception as the default Error type
    /// </summary>
    /// <typeparam name="OK"></typeparam>
    /// <returns></returns>
    public static Result<OK, Exception> Error<OK>(Exception exception) => new Result<OK, Exception>.Error(exception);

    /// <summary>
    /// Creates a Result{OK, ERROR} from a nullable value, returning an Ok result if the value is not null
    /// or an Error result with the provided error if the value is null.
    /// </summary>
    /// <param name="value">The nullable value to be converted into a Result{OK, ERROR}.</param>
    /// <param name="error">The error to return in case the value is null.</param>
    /// <returns>
    /// A Result{OK, ERROR} that represents either an Ok result with the given value
    /// or an Error result with the provided error.
    /// </returns>
    [Pure]
    public static Result<OK, ERROR> FromNullable<OK, ERROR>(OK? value, ERROR error)
    {
        return value switch
        {
            not null => Ok<OK, ERROR>(value),
            _ => Error<OK, ERROR>(error)
        };
    }

    /// <summary>
    /// Creates a <see cref="Result{OK, ERROR}"/> instance from a nullable value.
    /// If the value is not null, it returns an Ok result containing the value.
    /// If the value is null, it returns an <see cref="Result{OK,ERROR}.Error"/> containing a <see cref="NullReferenceException"/>
    /// </summary>
    /// <param name="value">The nullable value to check.</param>
    /// <returns>
    /// A <see cref="Result{OK, ERROR}"/> representing an Ok result with the value or an Error result containing a <see cref="NullReferenceException"/>
    /// </returns>
    public static Result<OK, Exception> FromNullable<OK>(OK? value)
    {
        return value switch
        {
            not null => Ok<OK>(value),
            _ => Error<OK>(new NullReferenceException())
        };
    }

    /// <summary>
    /// Converts a <see cref="Result{OK, ERROR}"/> to a nullable value of type OK if the result is successful (Ok).
    /// Returns null if the result represents an error.
    /// </summary>
    /// <param name="result">The result instance to convert.</param>
    /// <typeparam name="OK">The type of the value contained in the result.</typeparam>
    /// <typeparam name="ERROR">The type of the error contained in the result.</typeparam>
    /// <returns>A nullable value of type OK if the result is Ok, otherwise null.</returns>
    [Pure]
    public static OK? ToNullable<OK, ERROR>(this Result<OK, ERROR> result)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => (OK?)ok.Value,
            _ => default
        };
    }


    /// <summary>
    /// Checks whether the given result represents a successful operation.
    /// </summary>
    /// <returns>True if the result represents a successful operation; otherwise, false.</returns>
    [Pure]
    public static bool IsOk<OK, ERROR>(this Result<OK, ERROR> result)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the given result represents an unsuccessful operation.
    /// </summary>
    /// <returns>OKrue if the result represents an unsuccessful operation; otherwise, false.</returns>
    [Pure]
    public static bool IsError<OK, ERROR>(this Result<OK, ERROR> result)
    {
        return result switch
        {
            Result<OK, ERROR>.Error => true,
            _ => false
        };
    }


    /// <summary>
    /// Evaluates the result and invokes the appropriate function based on whether the result contains an 'Ok' or an 'Error'.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="ifOk">The function to invoke if the result is 'Ok'.</param>
    /// <param name="ifError">The function to invoke if the result is 'Error'.</param>
    /// <typeparam name="OK">Type of the source value</typeparam>
    /// <typeparam name="ERROR">Type of the Error</typeparam>
    /// <typeparam name="OKAFTER">Type of the result value</typeparam>
    /// <returns>The value obtained by invoking either the 'ifOk' function or the 'ifError' function, based on the result's state.</returns>
    [Pure]
    public static OKAFTER Match<OK, ERROR, OKAFTER>(this Result<OK, ERROR> result, Func<OK, OKAFTER> ifOk,
        Func<ERROR, OKAFTER> ifError)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => ifOk(ok.Value),
            Result<OK, ERROR>.Error error => ifError(error.Value),
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
    [Pure]
    public static Result<OK, ERROR> Match<OK, ERROR>(this Result<OK, ERROR> result, Action<OK> ifOk,
        Action<ERROR> ifError)
    {
        switch (result)
        {
            case Result<OK, ERROR>.Ok ok:
                ifOk(ok.Value);
                break;
            case Result<OK, ERROR>.Error error:
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
    /// <typeparam name="OK">Type of the source value</typeparam>
    /// <typeparam name="ERROR">Type of the Error</typeparam>
    /// <typeparam name="OKAFTER">Type of the result value</typeparam>
    /// <returns>The value obtained by invoking either the 'ifOk' function or the 'ifError' function, based on the result's state.</returns>
    [Pure]
    public static async Task<OKAFTER> MatchAsync<OK, ERROR, OKAFTER>(this Result<OK, ERROR> result,
        Func<OK, Task<OKAFTER>> ifOk,
        Func<ERROR, Task<OKAFTER>> ifError)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => await ifOk(ok.Value).ConfigureAwait(false),
            Result<OK, ERROR>.Error error => await ifError(error.Value).ConfigureAwait(false),
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
    /// <typeparam name="OK">Type of the source value</typeparam>
    /// <typeparam name="ERROR">Type of the Error</typeparam>
    /// <typeparam name="OKAFTER">Type of the result value</typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OKAFTER, ERROR> Map<OK, ERROR, OKAFTER>(this Result<OK, ERROR> result,
        Func<OK, OKAFTER> action)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => Ok<OKAFTER, ERROR>(action(ok.Value)),
            Result<OK, ERROR>.Error error => Error<OKAFTER, ERROR>(error.Value),
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
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns>The original `Result` instance, either with the same value or error.</returns>
    [Pure]
    public static Result<OK, ERROR> Tap<OK, ERROR>(this Result<OK, ERROR> result, Action<OK> action)
    {
        if (result is Result<OK, ERROR>.Ok ok) action(ok.Value);

        return result;
    }

    /// <summary>
    /// Updates a value held within the 'Ok' of a 'Result' by calling a given function on it.
    /// 
    /// If the 'Result' is an 'Error' rather than 'Ok', the function is not called and the 'Result' stays the same.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="OK">Type of the source value</typeparam>
    /// <typeparam name="ERROR">Type of the Error</typeparam>
    /// <typeparam name="OKAFTER">Type of the result value</typeparam>
    /// <returns></returns>
    [Pure]
    public static async Task<Result<OKAFTER, ERROR>> MapAsync<OK, ERROR, OKAFTER>(this Result<OK, ERROR> result,
        Func<OK, Task<OKAFTER>> action)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => Ok<OKAFTER, ERROR>(await action(ok.Value).ConfigureAwait(false)),
            Result<OK, ERROR>.Error error => Error<OKAFTER, ERROR>(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Extracts the value from a 'Result', returning a default value if there is none.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [Pure]
    public static OK Unwrap<OK, ERROR>(this Result<OK, ERROR> result, OK defaultValue)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => ok.Value,
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
    /// <typeparam name="OK">Type of the source value</typeparam>
    /// <typeparam name="ERROR">Type of the Error</typeparam>
    /// <typeparam name="OKAFTER">Type of the result value</typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OKAFTER, ERROR> Try<OK, ERROR, OKAFTER>(this Result<OK, ERROR> result,
        Func<OK, Result<OKAFTER, ERROR>> apply)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => apply(ok.Value),
            Result<OK, ERROR>.Error error => Error<OKAFTER, ERROR>(error.Value),
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
    /// <typeparam name="OK">Type of the source value</typeparam>
    /// <typeparam name="ERROR">Type of the Error</typeparam>
    /// <typeparam name="OKAFTER">Type of the result value</typeparam>
    /// <returns></returns>
    [Pure]
    public static async Task<Result<OKAFTER, ERROR>> TryAsync<OK, ERROR, OKAFTER>(this Result<OK, ERROR> result,
        Func<OK, Task<Result<OKAFTER, ERROR>>> apply)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => await apply(ok.Value).ConfigureAwait(false),
            Result<OK, ERROR>.Error error => Error<OKAFTER, ERROR>(error.Value),
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
    /// <typeparam name="OK">Type of the value</typeparam>
    /// <typeparam name="ERROR">Type of the source error </typeparam>
    /// <typeparam name="ERRORAFTER">Type of the result error</typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OK, ERRORAFTER> MapError<OK, ERROR, ERRORAFTER>(this Result<OK, ERROR> result,
        Func<ERROR, ERRORAFTER> action)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => Ok<OK, ERRORAFTER>(ok.Value),
            Result<OK, ERROR>.Error error => Error<OK, ERRORAFTER>(action(error.Value)),
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
    [Pure]
    public static Result<OK, ERROR> TapError<OK, ERROR>(this Result<OK, ERROR> result, Action<ERROR> action)
    {
        if (result is Result<OK, ERROR>.Error error) action(error.Value);

        return result;
    }

    /// <summary>
    /// Replaces the error value in a Result instance with a new error value, while keeping the successful value unchanged if applicable.
    /// </summary>
    /// <param name="result">The Result instance to process.</param>
    /// <param name="newError">The new error value to be used if the Result instance represents an error.</param>
    /// <returns>A new Result instance with the error value replaced by <paramref name="newError"/>, or the original success value if the instance represents success.</returns>
    [Pure]
    public static Result<OK, ERRORAFTER> ReplaceError<OK, ERROR, ERRORAFTER>(this Result<OK, ERROR> result,
        ERRORAFTER newError)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => Ok<OK, ERRORAFTER>(ok.Value),
            Result<OK, ERROR>.Error => Error<OK, ERRORAFTER>(newError),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    /// <summary>
    /// Extracts the 'Error' value from a result, returning a default value if the result is an 'Ok'.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultError"></param>
    /// <typeparam name="ERROR"></typeparam>
    /// <typeparam name="OK"></typeparam>
    /// <returns></returns>
    [Pure]
    public static ERROR UnwrapError<OK, ERROR>(this Result<OK, ERROR> result, ERROR defaultError)
    {
        return result switch
        {
            Result<OK, ERROR>.Error error => error.Value,
            _ => defaultError,
        };
    }


    /// <summary>
    /// Merges a nested 'result' into a single layer.
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OK, ERROR> Flatten<OK, ERROR>(this Result<Result<OK, ERROR>, ERROR> result)
    {
        return result switch
        {
            Result<OK, ERROR>.Ok ok => ok.Value,
            Result<OK, ERROR>.Error error => Error<OK, ERROR>(error.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    /// <summary>
    /// Extracts the inner value from a result. Both the value and error must be of the same type.
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="OK"></typeparam>
    /// <returns></returns>
    [Pure]
    public static OK UnwrapBoth<OK>(this Result<OK, OK> result)
    {
        return result switch
        {
            Result<OK, OK>.Ok ok => ok.Value,
            Result<OK, OK>.Error error => error.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    /// <summary>
    /// Combines a list of results into a single result.
    /// If all elements in the list are 'Ok' then returns an 'Ok' holding the list of values.
    /// If any element is 'Error' then returns the first error.
    /// </summary>
    /// <param name="results"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<List<OK>, ERROR> All<OK, ERROR>(this List<Result<OK, ERROR>> results)
    {
        var list = new List<OK>();

        foreach (var result in results)
        {
            switch (result)
            {
                case Result<OK, ERROR>.Error error:
                    return Error<List<OK>, ERROR>(error.Value);
                case Result<OK, ERROR>.Ok ok:
                    list.Add(ok.Value);
                    break;
            }
        }

        return Ok<List<OK>, ERROR>(list);
    }


    /// <summary>
    /// Given a list of results, returns a pair where the first element is a list of all the values inside 'Ok' and the second element is a list with all the values inside 'Error'.
    /// </summary>
    /// <param name="results"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    [Pure]
    public static Tuple<List<OK>, List<ERROR>> Partition<OK, ERROR>(this List<Result<OK, ERROR>> results)
    {
        var values = new List<OK>();
        var errors = new List<ERROR>();

        foreach (var result in results)
        {
            switch (result)
            {
                case Result<OK, ERROR>.Ok ok:
                    values.Add(ok.Value);
                    break;
                case Result<OK, ERROR>.Error error:
                    errors.Add(error.Value);
                    break;
            }
        }

        return Tuple.Create(values, errors);
    }


    /// <summary>
    /// Extracts the values from all successful results in the given list.
    /// </summary>
    /// <typeparam name="OK">The type of value associated with a successful result.</typeparam>
    /// <typeparam name="ERROR">The type of the error associated with an unsuccessful result.</typeparam>
    /// <param name="list">The list of Result objects to evaluate.</param>
    /// <returns>A list containing the values from all successful results.</returns>
    [Pure]
    public static List<OK> Values<OK, ERROR>(this List<Result<OK, ERROR>> list)
    {
        return list
            .Select(x =>
            {
                if (x is Result<OK, ERROR>.Ok ok) return ok;
                return null;
            })
            .Where(x => x != null)
            // cant be null, just filtered
            .Select(x => x!.Value)
            .ToList();
    }

    /// <summary>
    /// Calls a function that might throw an exception in a try-catch block.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="mightThrow"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="OKAFTER"></typeparam>
    /// <returns>The Result of the function, the thrown exception or the previous error if it was already the error variant</returns>
    [Pure]
    public static Result<OKAFTER, Exception> TryCatch<OK, OKAFTER>(this Result<OK, Exception> result,
        Func<OK, OKAFTER> mightThrow)
    {
        try
        {
            return result.Map(mightThrow);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Calls the function that might throw with the value in a try catch block.
    /// Returns either the result of the function executing in an Ok() or the exception in an Error()
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mightThrow"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="OKAFTER"></typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OKAFTER, Exception> TryCatch<OK, OKAFTER>(this OK value, Func<OK, OKAFTER> mightThrow)
    {
        try
        {
            return mightThrow(value);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Calls the function that might throw with the value in a try catch block.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mightThrow"></param>
    /// <param name="convertError"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <typeparam name="OKAFTER"></typeparam>
    /// <returns>
    /// Either the result of the function in an Ok()
    /// Or applies the supplied 'convertError' function on the caught exception returning it in an Error()
    /// </returns>
    public static Result<OKAFTER, ERROR> TryCatch<OK, ERROR, OKAFTER>(
        this OK value,
        Func<OK, OKAFTER> mightThrow,
        Func<Exception, ERROR> convertError
    )
    {
        try
        {
            return mightThrow(value);
        }
        catch (Exception e)
        {
            return convertError(e);
        }
    }

    /// <summary>
    /// Returns the first value if it is 'Ok', otherwise returns the second value
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OK, ERROR> Or<OK, ERROR>(this Result<OK, ERROR> first, Result<OK, ERROR> second)
    {
        return first.Match(_ => first, _ => second);
    }


    /// <summary>
    /// Returns the first value if it is 'Ok', otherwise evaluates the given function for a fallback value.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <typeparam name="OK"></typeparam>
    /// <typeparam name="ERROR"></typeparam>
    /// <returns></returns>
    [Pure]
    public static Result<OK, ERROR> LazyOr<OK, ERROR>(this Result<OK, ERROR> first, Func<Result<OK, ERROR>> second)
    {
        return first.Match(_ => first, _ => second());
    }
}