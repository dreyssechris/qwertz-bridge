using QwertzBridge.Core.Config;
using QwertzBridge.Core.Domain;

namespace QwertzBridge.Core.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void ValidConfig_Parses()
    {
        const string json = """
            {
              "rules": [
                { "scanCode": "0x33", "altGr": true, "output": "<" },
                { "scanCode": 52, "altGr": true, "output": ">" }
              ]
            }
            """;

        var result = ConfigLoader.Parse(json);

        Assert.False(result.UsedFallback);
        Assert.Null(result.Error);
        Assert.Equal(0x33, result.Config.Rules[0].ScanCode);
        Assert.Equal(0x34, result.Config.Rules[1].ScanCode);
    }

    [Fact]
    public void DefaultSerialization_RoundTrips()
    {
        var result = ConfigLoader.Parse(ConfigLoader.SerializeDefault());

        Assert.False(result.UsedFallback);
        Assert.Equal(3, result.Config.Rules.Count);
        Assert.Equal(new[] { "<", ">", "|" }, result.Config.Rules.Select(r => r.Output));
        Assert.All(result.Config.Rules, r => Assert.True(r.AltGr));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{ not json at all")]
    [InlineData("42")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{ "rules": [] }""")]
    [InlineData("""{ "rules": null }""")]
    public void InvalidInput_FallsBackToDefaults(string json)
    {
        var result = ConfigLoader.Parse(json);

        Assert.True(result.UsedFallback);
        Assert.NotNull(result.Error);
        // The fallback config must be fully functional.
        Assert.Equal(3, result.Config.Rules.Count);
    }

    [Fact]
    public void RuleWithoutOutput_FallsBack()
    {
        const string json = """{ "rules": [ { "scanCode": "0x33", "output": "" } ] }""";

        var result = ConfigLoader.Parse(json);

        Assert.True(result.UsedFallback);
        Assert.Contains("output", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RuleWithoutScanCode_FallsBack()
    {
        const string json = """{ "rules": [ { "output": "<" } ] }""";

        var result = ConfigLoader.Parse(json);

        Assert.True(result.UsedFallback);
        Assert.Contains("scan code", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidScanCodeString_FallsBack()
    {
        const string json = """{ "rules": [ { "scanCode": "0xZZ", "output": "<" } ] }""";

        Assert.True(ConfigLoader.Parse(json).UsedFallback);
    }

    [Fact]
    public void UnknownProperties_AreIgnored()
    {
        const string json = """
            {
              "somethingElse": true,
              "rules": [ { "scanCode": "0x33", "output": "<", "color": "red" } ]
            }
            """;

        Assert.False(ConfigLoader.Parse(json).UsedFallback);
    }

    [Fact]
    public void CommentsAndTrailingCommas_AreAllowed()
    {
        const string json = """
            {
              // friendly for hand-edited files
              "rules": [ { "scanCode": "0x33", "output": "<" }, ],
            }
            """;

        Assert.False(ConfigLoader.Parse(json).UsedFallback);
    }

    [Fact]
    public void CaseInsensitivePropertyNames_AreAccepted()
    {
        const string json = """{ "Rules": [ { "ScanCode": 51, "Output": "<" } ] }""";

        var result = ConfigLoader.Parse(json);

        Assert.False(result.UsedFallback);
        Assert.Equal(ScanCodes.Comma, result.Config.Rules[0].ScanCode);
    }
}
