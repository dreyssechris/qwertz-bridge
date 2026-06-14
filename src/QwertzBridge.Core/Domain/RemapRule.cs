namespace QwertzBridge.Core.Domain;

// Maps a physical key (matched by scan code) to replacement text.
public sealed class RemapRule
{
    public ushort ScanCode { get; init; }

    // Must match the event's extended flag. Default false keeps numpad divide
    // (0x35 + extended) from colliding with the slash key (0x35).
    public bool Extended { get; init; }

    // True (default): fires only while AltGr is held. False: fires on the plain key.
    public bool AltGr { get; init; } = true;

    public string Output { get; init; } = "";
}
