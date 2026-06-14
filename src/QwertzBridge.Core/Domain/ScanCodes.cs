namespace QwertzBridge.Core.Domain;

// Hardware scan codes (Set 1) used by the default rules and the engine.
public static class ScanCodes
{
    public const ushort Ctrl = 0x1D;   // non-extended = LCtrl, extended = RCtrl
    public const ushort Alt = 0x38;    // non-extended = LAlt, extended = RAlt (AltGr)
    public const ushort LeftShift = 0x2A;
    public const ushort RightShift = 0x36;
    public const ushort LeftWin = 0x5B;
    public const ushort RightWin = 0x5C;
    public const ushort Comma = 0x33;  // ANSI ","
    public const ushort Period = 0x34; // ANSI "."
    public const ushort Slash = 0x35;  // ANSI "/" (German "-"); extended = numpad divide
}
