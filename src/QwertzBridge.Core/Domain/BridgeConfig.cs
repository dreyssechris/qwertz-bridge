namespace QwertzBridge.Core.Domain;

/// <summary>Root configuration: an ordered list of profiles.</summary>
public sealed class BridgeConfig
{
    /// <summary>All profiles. Process-specific profiles win over the catch-all profile.</summary>
    public List<Profile> Profiles { get; init; } = [];

    /// <summary>
    /// Builds the built-in default configuration:
    /// AltGr + comma → "&lt;", AltGr + period → "&gt;", AltGr + slash key (German "-") → "|".
    /// </summary>
    public static BridgeConfig CreateDefault() => new()
    {
        Profiles =
        [
            new Profile
            {
                Name = "Default",
                ProcessNames = [],
                Rules =
                [
                    new RemapRule { ScanCode = ScanCodes.Comma, AltGr = true, Output = "<" },
                    new RemapRule { ScanCode = ScanCodes.Period, AltGr = true, Output = ">" },
                    new RemapRule { ScanCode = ScanCodes.Slash, AltGr = true, Output = "|" },
                ],
            },
        ],
    };
}
