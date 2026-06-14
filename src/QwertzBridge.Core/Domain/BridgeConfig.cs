namespace QwertzBridge.Core.Domain;

// Root configuration: a flat list of remap rules.
public sealed class BridgeConfig
{
    public List<RemapRule> Rules { get; init; } = [];

    // Built-in defaults: AltGr + comma -> "<", AltGr + period -> ">", AltGr + slash key -> "|".
    public static BridgeConfig CreateDefault() => new()
    {
        Rules =
        [
            new RemapRule { ScanCode = ScanCodes.Comma, AltGr = true, Output = "<" },
            new RemapRule { ScanCode = ScanCodes.Period, AltGr = true, Output = ">" },
            new RemapRule { ScanCode = ScanCodes.Slash, AltGr = true, Output = "|" },
        ],
    };
}
