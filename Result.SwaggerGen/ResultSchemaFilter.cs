using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace nuv.Result.SwaggerGen;

public class ResultSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || context.Type.GetGenericTypeDefinition() != typeof(Result<,>))
            return;

        var genericArgs = context.Type.GetGenericArguments();
        var valueType = genericArgs[0];
        var errorType = genericArgs[1];

        // add discriminator property
        schema.Properties["ResultType"] = new OpenApiSchema
        {
            Type = "string",
            Enum = new List<IOpenApiAny>
            {
                new OpenApiString("ok"),
                new OpenApiString("error"),
            }
        };
        schema.Required.Add("ResultType");

        // add value property
        schema.Properties["Value"] = new OpenApiSchema
        {
            OneOf = new List<OpenApiSchema>
            {
                context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository),
                context.SchemaGenerator.GenerateSchema(errorType, context.SchemaRepository),
            }
        };
        schema.Required.Add("Value");

        // add example
        schema.Example = new OpenApiObject
        {
            ["ResultType"] = new OpenApiString("ok"),
            ["Value"] = new OpenApiString($"[{valueType.Name} value]"),
        };

        schema.Description =
            $"Represents a Result<{valueType.Name},{errorType.Name}> type. Can either be Ok {valueType.Name} or Error {errorType.Name}";
    }
}