using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace nuv.Option.SwaggerGen;

public class OptionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || context.Type.GetGenericTypeDefinition() != typeof(Option<>))
            return;

        var genericArgs = context.Type.GetGenericArguments();
        var valueType = genericArgs[0];

        // add discriminator property
        schema.Properties["OptionType"] = new OpenApiSchema
        {
            Type = "string",
            Enum = new List<IOpenApiAny>
            {
                new OpenApiString("some"),
                new OpenApiString("none"),
            }
        };
        schema.Required.Add("OptionType");

        // add value property 
        schema.Properties["Value"] = context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository);

        // add example
        schema.Example = new OpenApiObject
        {
            ["OptionType"] = new OpenApiString("some"),
            ["Value"] = new OpenApiString($"[{valueType.Name} value]"),
        };

        schema.Description =
            $"Represents an Option<{valueType.Name}> type. Can either be Some {valueType.Name} or None (without a Value)";
    }
}