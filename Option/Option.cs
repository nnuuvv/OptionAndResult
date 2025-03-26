using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a container that may or may not hold a value of type <typeparamref name="T"/>.
/// Should be used for Optional parameters. For return values use <see cref="Result"/>
/// </summary>
/// <typeparam name="T">The type of value the Option can hold.</typeparam>
public record Option<T>
{
    /// <summary>
    /// May or may not have a value, check HasValue.
    /// <remarks>
    ///     <para>
    ///     !! Should not be accessed directly outside the Option class
    ///     <para>
    ///     Use <see cref="Option.Unwrap(Option{T},x)"/> instead
    ///     </para>
    ///     </para>
    /// </remarks>
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Indicates whether Value is set.
    /// <remarks>
    ///     <para>
    ///     !! Should not be accessed directly outside the Option class
    ///     <para>
    ///     Use <see cref="Option.IsSome(Option{T})"/> or <see cref="Option.IsNone(Option{T})"/> instead
    ///     </para>
    ///     </para>
    /// </remarks>
    /// </summary>
    public bool HasValue { get; set; }
}

/// <summary>
/// Provides a set of operations to create, transform, and work with instances of <see cref="Option{T}"/>.
/// Represents utility methods for handling optional values.
/// </summary>
public static class Option
{
    /// <summary>
    /// Determines whether the specified <see cref="Option{T}"/> contains a value.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> to inspect.</param>
    /// <typeparam name="T">The type of value the <see cref="Option{T}"/> can hold.</typeparam>
    /// <returns>True if the <see cref="Option{T}"/> contains a value; otherwise, false.</returns>
    public static bool IsSome<T>(this Option<T> option) => option.HasValue;

    /// <summary>
    /// Determines whether the specified <see cref="Option{T}"/> does not contain a value.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> to inspect.</param>
    /// <typeparam name="T">The type of value the <see cref="Option{T}"/> can hold.</typeparam>
    /// <returns>True if the <see cref="Option{T}"/> does not contain a value; otherwise, false.</returns>
    public static bool IsNone<T>(this Option<T> option) => !option.HasValue;

    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> that holds a value.
    /// </summary>
    /// <param name="value">The value to be wrapped in an <see cref="Option{T}"/>.</param>
    /// <typeparam name="T">The type of the value to wrap.</typeparam>
    /// <returns>An <see cref="Option{T}"/> containing the specified value.</returns>
    public static Option<T> Some<T>(T value)
    {
        return new Option<T>()
        {
            HasValue = true,
            Value = value
        };
    }

    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> that does not contain a value.
    /// </summary>
    /// <typeparam name="T">The type of value the <see cref="Option{T}"/> can hold.</typeparam>
    /// <returns>An <see cref="Option{T}"/> with no value.</returns>
    public static Option<T> None<T>()
    {
        return new Option<T>()
        {
            HasValue = false,
        };
    }

    /// <summary>
    /// Converts the specified <see cref="Option{T}"/> to a <see cref="Result{TValue, TError}"/>.
    /// If the <see cref="Option{T}"/> contains a value, a successful <see cref="Result{TValue, TError}"/> is returned.
    /// Otherwise, an error <see cref="Result{TValue, TError}"/> containing a <see cref="Nil"/> is returned.
    /// </summary>
    /// <param name="option">The <see cref="Option{T}"/> to convert.</param>
    /// <typeparam name="TValue">The type of the value contained in the <see cref="Option{T}"/>.</typeparam>
    /// <returns>
    /// A <see cref="Result{TValue, TError}"/> representing either the successful conversion of the value or an error with <see cref="Nil"/>.
    /// </returns>
    public static Result<TValue, Nil> ToResult<TValue>(this Option<TValue> option)
    {
        return option.HasValue switch
        {
            true => Result.Ok<TValue, Nil>(option.Value),
            false => Result.Error<TValue, Nil>(new Nil()),
        };
    }

    /// <summary>
    /// Converts the specified <see cref="Result{TValue, TError}"/> to an <see cref="Option{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value held by the <see cref="Result{TValue, TError}"/>.</typeparam>
    /// <typeparam name="TError">The type of the error held by the <see cref="Result{TValue, TError}"/>.</typeparam>
    /// <param name="result">The <see cref="Result{TValue, TError}"/> to convert.</param>
    /// <returns>An <see cref="Option{TValue}"/> containing the value of the <see cref="Result{TValue, TError}"/> if successful; otherwise, an empty <see cref="Option{TValue}"/>.</returns>
    public static Option<TValue> FromResult<TValue, TError>(this Result<TValue, TError> result)
    {
        return result.DidSucceed switch
        {
            true => Some(result.Value),
            false => None<TValue>(),
        };
    }

    /// <summary>
    /// Extracts the value from an 'Option', returning a default value if there is none.
    /// </summary>
    /// <param name="option"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Unwrap<T>(this Option<T> option, T defaultValue)
    {
        return option.IsSome() switch
        {
            true => option.Value,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Updates a value held within the 'Some' of an 'Option' by calling a given function on it.
    ///
    /// If the 'Option' is a 'None' rather than 'Some', the function is not called and the 'Option' stays the same.
    /// </summary>
    /// <param name="option"></param>
    /// <param name="action"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Option<TResult> Map<TSource, TResult>(this Option<TSource> option, Func<TSource, TResult> action)
    {
        return option.IsSome() switch
        {
            true => Some(action(option.Value)),
            _ => None<TResult>()
        };
    }


    /// <summary>
    /// Merges a nested 'option' into a single layer.
    /// </summary>
    /// <param name="option"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
    {
        return option.IsSome() switch
        {
            true => option.Value,
            _ => None<T>()
        };
    }

    /// <summary>
    /// Updates a value held within the `Some` of an `Option` by calling a given function
    /// on it, where the given function also returns an `Option`. The two options are
    /// then merged together into one `Option`.
    ///
    /// If the `Option` is a `None` rather than `Some` the function is not called and the
    /// option stays the same.
    ///
    /// This function is the equivalent of calling `map` followed by `flatten`, and
    /// it is useful for chaining together multiple functions that return `Option`.
    /// </summary>
    /// <param name="option"></param>
    /// <param name="apply"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Option<TResult> Then<TSource, TResult>(this Option<TSource> option,
        Func<TSource, Option<TResult>> apply)
    {
        return option.IsSome() switch
        {
            true => apply(option.Value),
            _ => None<TResult>()
        };
    }

    /// <summary>
    /// Combines a list of 'Option's into a single 'Option'.
    /// If all elements in the list are 'Some' then returns a 'Some' holding the list of values.
    /// If any element is 'None' then returns 'None'.
    /// </summary>
    /// <param name="options"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Option<List<T>> All<T>(this List<Option<T>> options)
    {
        var list = new List<T>();

        foreach (var x in options)
        {
            if (x.IsNone()) return None<List<T>>();
            list.Add(x.Value);
        }

        return Some(list);
    }

    /// <summary>
    /// Extracts the values from a list of <see cref="Option{T}"/> where the options contain a value.
    /// </summary>
    /// <param name="list">The list of <see cref="Option{T}"/> to extract values from.</param>
    /// <typeparam name="T">The type of value the <see cref="Option{T}"/> can hold.</typeparam>
    /// <returns>A list of values extracted from the specified <see cref="Option{T}"/> instances that contain a value.</returns>
    public static List<T> Values<T>(this List<Option<T>> list)
    {
        return list
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .ToList();
    }
}