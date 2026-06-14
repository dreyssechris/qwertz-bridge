using System.Text.Json;
using QwertzBridge.Core.Domain;

namespace QwertzBridge.Core.Config;

// UsedFallback is true when the input was rejected and the built-in defaults are in
// effect; Error then explains why.
public sealed record ConfigLoadResult(BridgeConfig Config, bool UsedFallback, string? Error);

// Parses and validates the JSON config. Never throws: invalid input yields the
// built-in defaults plus an error message.
public static class ConfigLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        Converters = { new ScanCodeJsonConverter() },
    };

    public static ConfigLoadResult Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Fallback("Configuration file is empty.");

        BridgeConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<BridgeConfig>(json, Options);
        }
        catch (JsonException ex)
        {
            return Fallback($"Invalid JSON: {ex.Message}");
        }

        if (config?.Rules is not { Count: > 0 })
            return Fallback("No rules defined.");

        var error = Validate(config);
        return error is null ? new ConfigLoadResult(config, false, null) : Fallback(error);
    }

    public static string SerializeDefault() =>
        JsonSerializer.Serialize(BridgeConfig.CreateDefault(), Options);

    private static ConfigLoadResult Fallback(string error) =>
        new(BridgeConfig.CreateDefault(), true, error);

    private static string? Validate(BridgeConfig config)
    {
        for (var i = 0; i < config.Rules.Count; i++)
        {
            var rule = config.Rules[i];
            if (rule is null)
                return $"Rule #{i + 1} is null.";
            if (rule.ScanCode == 0)
                return $"Rule #{i + 1} has no scan code.";
            if (string.IsNullOrEmpty(rule.Output))
                return $"Rule #{i + 1} has no output.";
        }

        return null;
    }
}
