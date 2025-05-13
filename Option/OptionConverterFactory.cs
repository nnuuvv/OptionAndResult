using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace nuv.Option;

/// <summary>
/// Add this to your JsonSerializationOptions.Converters to enable Option serialization
/// </summary>
public class OptionConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the specified type can be converted using an OptionConverter 
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);
    }

    /// <summary>
    /// Creates OptionConverter for the specified Option type
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();
        
        var valueType = genericArgs[0];

        var converterType = typeof(OptionConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }
}