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
              "profiles": [
                {
                  "name": "Default",
                  "processNames": [],
                  "rules": [
                    { "scanCode": "0x33", "altGr": true, "output": "<" },
                    { "scanCode": 52, "altGr": true, "output": ">" }
                  ]
                }
              ]
            }
            """;

        var result = ConfigLoader.Parse(json);

        Assert.False(result.UsedFallback);
        Assert.Null(result.Error);
        var rules = result.Config.Profiles[0].Rules;
        Assert.Equal(0x33, rules[0].ScanCode);
        Assert.Equal(0x34, rules[1].ScanCode);
    }

    [Fact]
    public void DefaultSerialization_RoundTrips()
    {
        var result = ConfigLoader.Parse(ConfigLoader.SerializeDefault());

        Assert.False(result.UsedFallback);
        var rules = result.Config.Profiles[0].Rules;
        Assert.Equal(3, rules.Count);
        Assert.Equal(new[] { "<", ">", "|" }, rules.Select(r => r.Output));
        Assert.All(rules, r => Assert.True(r.AltGr));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{ not json at all")]
    [InlineData("42")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{ "profiles": [] }""")]
    [InlineData("""{ "profiles": null }""")]
    public void InvalidInput_FallsBackToDefaults(string json)
    {
        var result = ConfigLoader.Parse(json);

        Assert.True(result.UsedFallback);
        Assert.NotNull(result.Error);
        // The fallback config must be fully functional.
        Assert.Single(result.Config.Profiles);
        Assert.Equal(3, result.Config.Profiles[0].Rules.Count);
    }

    [Fact]
    public void RuleWithoutOutput_FallsBack()
    {
        const string json = """
            { "profiles": [ { "rules": [ { "scanCode": "0x33", "output": "" } ] } ] }
            """;

        var result = ConfigLoader.Parse(json);

        Assert.True(result.UsedFallback);
        Assert.Contains("output", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RuleWithoutScanCode_FallsBack()
    {
        const string json = """
            { "profiles": [ { "rules": [ { "output": "<" } ] } ] }
            """;

        var result = ConfigLoader.Parse(json);

        Assert.True(result.UsedFallback);
        Assert.Contains("scan code", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidScanCodeString_FallsBack()
    {
        const string json = """
            { "profiles": [ { "rules": [ { "scanCode": "0xZZ", "output": "<" } ] } ] }
            """;

        Assert.True(ConfigLoader.Parse(json).UsedFallback);
    }

    [Fact]
    public void NullRules_FallsBack()
    {
        const string json = """
            { "profiles": [ { "name": "X", "rules": null } ] }
            """;

        Assert.True(ConfigLoader.Parse(json).UsedFallback);
    }

    [Fact]
    public void UnknownProperties_AreIgnored()
    {
        const string json = """
            {
              "somethingElse": true,
              "profiles": [
                { "rules": [ { "scanCode": "0x33", "output": "<", "color": "red" } ] }
              ]
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
              "profiles": [
                { "rules": [ { "scanCode": "0x33", "output": "<" }, ] },
              ],
            }
            """;

        Assert.False(ConfigLoader.Parse(json).UsedFallback);
    }

    [Fact]
    public void CaseInsensitivePropertyNames_AreAccepted()
    {
        const string json = """
            { "Profiles": [ { "Rules": [ { "ScanCode": 51, "Output": "<" } ] } ] }
            """;

        var result = ConfigLoader.Parse(json);

        Assert.False(result.UsedFallback);
        Assert.Equal(ScanCodes.Comma, result.Config.Profiles[0].Rules[0].ScanCode);
    }
}
