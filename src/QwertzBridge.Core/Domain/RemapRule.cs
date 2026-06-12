namespace QwertzBridge.Core.Domain;

/// <summary>Maps a physical key (matched by scan code) to a replacement text.</summary>
public sealed class RemapRule
{
    /// <summary>Hardware scan code of the physical key, e.g. 0x33 for the comma key.</summary>
    public ushort ScanCode { get; init; }

    /// <summary>
    /// Whether the extended-key flag must be set. Default false, which deliberately
    /// excludes e.g. numpad divide (0x35 + extended) when targeting the slash key (0x35).
    /// </summary>
    public bool Extended { get; init; }

    /// <summary>If true (default) the rule fires only while AltGr is held; if false it fires on the plain key.</summary>
    public bool AltGr { get; init; } = true;

    /// <summary>Text that is sent instead of the original key.</summary>
    public string Output { get; init; } = "";
}
