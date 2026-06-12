namespace QwertzBridge.Core.Domain;

/// <summary>Well-known hardware scan codes (Set 1) used by the default profile and the engine.</summary>
public static class ScanCodes
{
    /// <summary>Ctrl key. Non-extended = left Ctrl, extended = right Ctrl.</summary>
    public const ushort Ctrl = 0x1D;

    /// <summary>Alt key. Non-extended = left Alt, extended = right Alt (AltGr on German layouts).</summary>
    public const ushort Alt = 0x38;

    /// <summary>Left Shift.</summary>
    public const ushort LeftShift = 0x2A;

    /// <summary>Right Shift.</summary>
    public const ushort RightShift = 0x36;

    /// <summary>Windows key (extended). Non-extended 0x5B does not occur for Win keys.</summary>
    public const ushort LeftWin = 0x5B;

    /// <summary>Right Windows key (extended).</summary>
    public const ushort RightWin = 0x5C;

    /// <summary>Physical comma key (ANSI ","), German layout: ",".</summary>
    public const ushort Comma = 0x33;

    /// <summary>Physical period key (ANSI "."), German layout: ".".</summary>
    public const ushort Period = 0x34;

    /// <summary>Physical slash key (ANSI "/"), German layout: "-". Extended variant is numpad divide.</summary>
    public const ushort Slash = 0x35;
}
