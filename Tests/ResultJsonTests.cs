using System.Text.Json;
using System.Text.Json.Serialization;
using nuv.Option;
using nuv.Result;

namespace Tests;

[TestFixture]
public class ResultJsonTests
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new OptionConverterFactory(),
            new ResultConverterFactory()
        },
        PropertyNameCaseInsensitive = true,
    };

    [TestCaseSource(nameof(BasicResults))]
    public void BasicResultTest(Result<string, int> result)
    {
        var json = JsonSerializer.Serialize(result, JsonSerializerOptions);

        var deserialized = JsonSerializer.Deserialize<Result<string, int>>(json, JsonSerializerOptions);

        Assert.That(deserialized, Is.EqualTo(result));
    }

    private static IEnumerable<Result<string, int>> BasicResults()
    {
        yield return new Result<string, int>.Ok("some example string");
        yield return new Result<string, int>.Error(12);
    }

    [TestCaseSource(nameof(ComplexResults))]
    public void ComplexResultTest(Result<TestExample, ExampleClass> result)
    {
        var json = JsonSerializer.Serialize(result, JsonSerializerOptions);

        var deserialized = JsonSerializer.Deserialize<Result<TestExample, ExampleClass>>(json, JsonSerializerOptions);

        Assert.That(deserialized, Is.EqualTo(result));
    }

    private static IEnumerable<Result<TestExample, ExampleClass>> ComplexResults()
    {
        yield return new Result<TestExample, ExampleClass>.Ok(new TestExample.Wibble());
        yield return new Result<TestExample, ExampleClass>.Ok(new TestExample.Wobble());
        yield return new Result<TestExample, ExampleClass>.Error(new ExampleClass(12));
    }
}

/// <summary>
/// equatable for ease of testing
/// </summary>
/// <param name="exampleProperty"></param>
public class ExampleClass(int exampleProperty) : IEquatable<ExampleClass>
{
    public int ExampleProperty { get; set; } = exampleProperty;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ExampleClass)obj);
    }

    public override int GetHashCode()
    {
        return ExampleProperty;
    }

    public bool Equals(ExampleClass? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ExampleProperty == other.ExampleProperty;
    }
}

[JsonDerivedType(typeof(Wibble), nameof(Wibble))]
[JsonDerivedType(typeof(Wobble), nameof(Wobble))]
public abstract record TestExample
{
    private TestExample()
    {
    }

    public sealed record Wibble : TestExample;

    public sealed record Wobble : TestExample;
}