using System.Runtime.InteropServices;
using QwertzBridge.Core.Abstractions;
using QwertzBridge.Core.Domain;

namespace QwertzBridge.Infrastructure;

/// <summary>
/// Sends text via SendInput with KEYEVENTF_UNICODE. While AltGr is physically held,
/// the modifiers are temporarily released so the target application receives the plain
/// character instead of a Ctrl+Alt chord, then restored to match the physical key state.
/// </summary>
public sealed class SendInputTextOutput : ITextOutput
{
    /// <inheritdoc />
    public void Send(string text, bool altGrIsDown)
    {
        if (text.Length == 0)
            return;

        var inputs = new List<NativeMethods.INPUT>(text.Length * 2 + 4);

        if (altGrIsDown)
        {
            inputs.Add(ScanKey(ScanCodes.Alt, extended: true, keyUp: true));
            inputs.Add(ScanKey(ScanCodes.Ctrl, extended: false, keyUp: true));
        }

        foreach (var c in text)
        {
            inputs.Add(UnicodeKey(c, keyUp: false));
            inputs.Add(UnicodeKey(c, keyUp: true));
        }

        if (altGrIsDown)
        {
            inputs.Add(ScanKey(ScanCodes.Ctrl, extended: false, keyUp: false));
            inputs.Add(ScanKey(ScanCodes.Alt, extended: true, keyUp: false));
        }

        NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<NativeMethods.INPUT>());
    }

    private static NativeMethods.INPUT ScanKey(ushort scanCode, bool extended, bool keyUp) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wScan = scanCode,
                dwFlags = NativeMethods.KEYEVENTF_SCANCODE
                    | (extended ? NativeMethods.KEYEVENTF_EXTENDEDKEY : 0)
                    | (keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0),
            },
        },
    };

    private static NativeMethods.INPUT UnicodeKey(char c, bool keyUp) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wScan = c,
                dwFlags = NativeMethods.KEYEVENTF_UNICODE
                    | (keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0),
            },
        },
    };
}
