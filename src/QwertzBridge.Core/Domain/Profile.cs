namespace QwertzBridge.Core.Domain;

/// <summary>A named set of remap rules, optionally restricted to specific processes.</summary>
public sealed class Profile
{
    /// <summary>A profile with no rules, used when no profile applies.</summary>
    public static Profile Empty { get; } = new() { Name = "(no profile)" };

    /// <summary>Display name of the profile.</summary>
    public string Name { get; init; } = "Default";

    /// <summary>
    /// Process names this profile applies to (with or without ".exe", case-insensitive).
    /// An empty list marks the catch-all profile.
    /// </summary>
    public List<string> ProcessNames { get; init; } = [];

    /// <summary>The remap rules of this profile.</summary>
    public List<RemapRule> Rules { get; init; } = [];

    /// <summary>Checks whether this profile targets the given process name.</summary>
    /// <param name="processName">Process name with or without ".exe" suffix.</param>
    public bool MatchesProcess(string processName)
    {
        var normalized = Normalize(processName);
        return ProcessNames.Any(p => Normalize(p).Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string name) =>
        name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name;
}
