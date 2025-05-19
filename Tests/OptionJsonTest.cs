using System.Text.Json;
using nuv.Option;

namespace Tests;
[TestFixture]
public class OptionJsonTests
{
    [TestCaseSource(nameof(BasicOptions))]
    public void BasicOptionTest(Option<string> option)
    {
        var json = JsonSerializer.Serialize(option);

        var deserialized = JsonSerializer.Deserialize<Option<string>>(json);

        Assert.That(deserialized, Is.EqualTo(option));
    }

    private static IEnumerable<Option<string>> BasicOptions()
    {
        yield return new Option<string>.Some("some example string");
        yield return new Option<string>.None();
    }

    [TestCaseSource(nameof(ComplexOptions))]
    public void ComplexResultTest(Option<TestExample> option)
    {
        var json = JsonSerializer.Serialize(option);

        var deserialized = JsonSerializer.Deserialize<Option<TestExample>>(json);

        Assert.That(deserialized, Is.EqualTo(option));
    }

    private static IEnumerable<Option<TestExample>> ComplexOptions()
    {
        yield return new Option<TestExample>.Some(new TestExample.Wibble());
        yield return new Option<TestExample>.Some(new TestExample.Wobble());
        yield return new Option<TestExample>.None();
    }
}


