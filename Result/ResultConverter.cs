using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace nuv.Result;

/// <summary>
/// Will be instantiated by <see cref="ResultConverterFactory"/>
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TError"></typeparam>
public class ResultConverter<TValue, TError> : JsonConverter<Result<TValue, TError>>
{
    /// <summary>
    /// Reads and converts JSON data into an instance of <see cref="Result{TValue, TError}"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the successful result value.</typeparam>
    /// <typeparam name="TError">The type of the error result value.</typeparam>
    /// <param name="reader">The UTF-8 JSON reader to read from.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">Options to control the behavior of the JSON serializer.</param>
    /// <returns>An instance of <see cref="Result{TValue, TError}"/> based on the JSON data.</returns>
    /// <exception cref="JsonException">Thrown when the JSON data is invalid or missing required properties.</exception>
    public override Result<TValue, TError> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);

        var root = document.RootElement;

        if (!root.TryGetProperty("ResultType", out var discriminator))
            throw new JsonException("Missing discriminator property 'ResultType'");

        var resultType = discriminator.GetString();

        switch (resultType)
        {
            case "ok":
                if (root.TryGetProperty("Value", out var valueElement))
                {
                    var value = JsonSerializer.Deserialize<TValue>(valueElement.GetRawText(), options);

                    if (value == null) throw new JsonException($"Result {resultType} Ok Value was null");

                    return new Result<TValue, TError>.Ok(value);
                }

                throw new JsonException("Missing 'Value' property for Ok result");

            case "error":
                if (root.TryGetProperty("Value", out var errorValueElement))
                {
                    var value = JsonSerializer.Deserialize<TValue>(errorValueElement.GetRawText(), options);

                    if (value == null) throw new JsonException($"Result {resultType} Error Value was null");

                    return new Result<TValue, TError>.Ok(value);
                }

                throw new JsonException("Missing 'Value' property for Error result");
            default:
                throw new JsonException($"Unknown result type: {resultType}");
        }
    }

    /// <summary>
    /// Writes the specified <see cref="Result{TValue, TError}"/> object as JSON using the provided writer.
    /// </summary>
    /// <typeparam name="TValue">The type of the successful result value.</typeparam>
    /// <typeparam name="TError">The type of the error result value.</typeparam>
    /// <param name="writer">The UTF-8 JSON writer to write to.</param>
    /// <param name="value">The <see cref="Result{TValue, TError}"/> instance to serialize.</param>
    /// <param name="options">Options to control the behavior of the JSON serializer.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="value"/> instance is of an unrecognized type.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, Result<TValue, TError> value, JsonSerializerOptions options)
    {
        // filter self from json option converter to avoid loop ----------------------------
        var opts = new JsonSerializerOptions(options);
        opts.Converters.Clear();

        foreach (var converter in options.Converters)
        {
            if (converter is ResultConverter<TValue, TError>) return;
            opts.Converters.Add(converter);
        }


        writer.WriteStartObject();

        // write type discriminator ----------------------------
        writer.WriteString("ResultType", value switch
        {
            Result<TValue, TError>.Ok => "ok",
            Result<TValue, TError>.Error => "error",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        });


        // write value ----------------------------
        writer.WritePropertyName("Value");
        switch (value)
        {
            case Result<TValue, TError>.Ok ok:
                JsonSerializer.Serialize(writer, ok.Value, opts);
                break;
            case Result<TValue, TError>.Error error:
                JsonSerializer.Serialize(writer, error.Value, opts);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }

        writer.WriteEndObject();
    }
}