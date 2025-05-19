# nuv.Option.SwaggerGen

Adds `OptionSchemaFilter` to enable proper integration with Swashbuckle/SwaggerGen

Add this to your swagger setup:

```csharp
builder.Service.AddSwaggerGen(setup => 
{
    setup.SchemaFilter<OptionSchemaFilter();
});
```