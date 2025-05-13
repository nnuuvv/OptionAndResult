using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using nuv.Result;

namespace nuv.Option;

/// <summary>
/// Will be instantiated by <see cref="OptionConverterFactory"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class OptionConverter<T> : JsonConverter<Option<T>>
{
    /// <summary>
    /// Reads and converts JSON data into an instance of <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the 'Some' value.</typeparam>
    /// <param name="reader">The UTF-8 JSON reader to read from.</param>
    /// <param name="typeToConvert">The type being converted.</param>
    /// <param name="options">Options to control the behavior of the JSON serializer.</param>
    /// <returns>An instance of <see cref="Option{T}"/> based on the JSON data.</returns>
    /// <exception cref="JsonException">Thrown when the JSON data is invalid or missing required properties.</exception>
    public override Option<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);

        var root = document.RootElement;

        if (!root.TryGetProperty("OptionType", out var discriminator))
            throw new JsonException("Missing discriminator property 'OptionType'");

        var optionType = discriminator.GetString();

        switch (optionType)
        {
            case "some":
                if (root.TryGetProperty("Value", out var valueElement))
                {
                    var value = JsonSerializer.Deserialize<T>(valueElement.GetRawText(), options);

                    if (value == null) throw new JsonException($"Option {optionType} 'Some' Value was null");

                    return new Option<T>.Some(value);
                }

                throw new JsonException("Missing 'Value' property for Some option");

            case "none":
                return new Option<T>.None();

            default:
                throw new JsonException($"Unknown option type: {optionType}");
        }
    }

    /// <summary>
    /// Writes the specified <see cref="Result{TValue, TError}"/> object as JSON using the provided writer.
    /// </summary>
    /// <typeparam name="T">The type of the 'some' option value.</typeparam>
    /// <param name="writer">The UTF-8 JSON writer to write to.</param>
    /// <param name="value">The <see cref="Option{T}"/> instance to serialize.</param>
    /// <param name="options">Options to control the behavior of the JSON serializer.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="value"/> instance is of an unrecognized type.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
    {
        // filter self from JsonSerializationOptions.Converters to avoid loop ----------------------------
        var opts = new JsonSerializerOptions(options);
        opts.Converters.Clear();

        foreach (var converter in options.Converters)
        {
            if (converter is OptionConverter<T>) return;
            opts.Converters.Add(converter);
        }

        writer.WriteStartObject();

        // write type discriminator ----------------------------
        writer.WriteString("OptionType", value switch
        {
            Option<T>.Some => "some",
            Option<T>.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        });
        

        // write value if is Some ----------------------------
        if (value is Option<T>.Some some)
        {
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, some.Value, opts);
        }

        writer.WriteEndObject();
    }
}