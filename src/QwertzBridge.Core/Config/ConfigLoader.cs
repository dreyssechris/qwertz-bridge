using System.Text.Json;
using QwertzBridge.Core.Domain;

namespace QwertzBridge.Core.Config;

/// <summary>Result of parsing a configuration document.</summary>
/// <param name="Config">The effective configuration (defaults if parsing failed).</param>
/// <param name="UsedFallback">True if the input was rejected and defaults are in effect.</param>
/// <param name="Error">Human-readable reason when <paramref name="UsedFallback"/> is true.</param>
public sealed record ConfigLoadResult(BridgeConfig Config, bool UsedFallback, string? Error);

/// <summary>
/// Parses and validates the JSON configuration. Never throws: any invalid input
/// results in the built-in defaults plus an error message.
/// </summary>
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

    /// <summary>Parses a JSON configuration document.</summary>
    /// <param name="json">The raw JSON text.</param>
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

        if (config is null || config.Profiles is not { Count: > 0 })
            return Fallback("No profiles defined.");

        var error = Validate(config);
        return error is null ? new ConfigLoadResult(config, false, null) : Fallback(error);
    }

    /// <summary>Serializes the built-in default configuration (used to create the initial config file).</summary>
    public static string SerializeDefault() =>
        JsonSerializer.Serialize(BridgeConfig.CreateDefault(), Options);

    private static ConfigLoadResult Fallback(string error) =>
        new(BridgeConfig.CreateDefault(), true, error);

    private static string? Validate(BridgeConfig config)
    {
        for (var p = 0; p < config.Profiles.Count; p++)
        {
            var profile = config.Profiles[p];
            if (profile is null || profile.Name is null || profile.ProcessNames is null || profile.Rules is null)
                return $"Profile #{p + 1} is incomplete (name, processNames and rules must not be null).";

            for (var r = 0; r < profile.Rules.Count; r++)
            {
                var rule = profile.Rules[r];
                if (rule is null)
                    return $"Profile \"{profile.Name}\": rule #{r + 1} is null.";
                if (rule.ScanCode == 0)
                    return $"Profile \"{profile.Name}\": rule #{r + 1} has no scan code.";
                if (string.IsNullOrEmpty(rule.Output))
                    return $"Profile \"{profile.Name}\": rule #{r + 1} has no output.";
            }
        }

        return null;
    }
}
