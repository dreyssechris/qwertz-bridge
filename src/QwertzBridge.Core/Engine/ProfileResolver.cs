using QwertzBridge.Core.Domain;

namespace QwertzBridge.Core.Engine;

/// <summary>Picks the profile that applies to the current foreground process.</summary>
public static class ProfileResolver
{
    /// <summary>
    /// Resolution order: first profile whose <see cref="Profile.ProcessNames"/> matches the
    /// process, otherwise the first catch-all profile (empty process list), otherwise
    /// <see cref="Profile.Empty"/> (no remapping).
    /// </summary>
    /// <param name="config">The active configuration.</param>
    /// <param name="processName">Foreground process name, or null if unknown.</param>
    public static Profile Resolve(BridgeConfig config, string? processName)
    {
        if (processName is not null)
        {
            var match = config.Profiles.FirstOrDefault(p => p.MatchesProcess(processName));
            if (match is not null)
                return match;
        }

        return config.Profiles.FirstOrDefault(p => p.ProcessNames.Count == 0) ?? Profile.Empty;
    }
}
