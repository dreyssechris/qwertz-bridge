using System.Runtime.InteropServices;

namespace QwertzBridge.Infrastructure;

// Sends text as synthetic keyboard input via KEYEVENTF_UNICODE (VK_PACKET), which injects
// the literal character independent of the active layout.
//
// We deliberately do NOT release or re-press the AltGr modifiers here. Windows manages a
// synthetic LCtrl as part of AltGr; toggling LCtrl ourselves around the send could clash
// with that and leave a modifier stuck down (every following key then triggers shortcuts).
// A VK_PACKET delivers the character regardless of the modifiers the user is holding.
public sealed class SendInputTextOutput
{
    public void Send(string text)
    {
        if (text.Length == 0)
            return;

        var inputs = new NativeMethods.INPUT[text.Length * 2];
        var i = 0;
        foreach (var c in text)
        {
            inputs[i++] = UnicodeKey(c, keyUp: false);
            inputs[i++] = UnicodeKey(c, keyUp: true);
        }

        NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
    }

    private static NativeMethods.INPUT UnicodeKey(char c, bool keyUp) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wScan = c,
                dwFlags = NativeMethods.KEYEVENTF_UNICODE | (keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0),
            },
        },
    };
}
