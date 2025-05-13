using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace nuv.Result;

/// <summary>
/// Add this to your JsonSerializationOptions.Converters to enable Result serialization
/// </summary>
public class ResultConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the specified type can be converted using a ResultConverter
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>);

    }

    /// <summary>
    /// Creates ResultConverter for the specified Result type
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();

        var valueType = genericArgs[0];
        var errorType = genericArgs[1];

        var converterType = typeof(ResultConverter<,>).MakeGenericType(valueType, errorType);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }
}