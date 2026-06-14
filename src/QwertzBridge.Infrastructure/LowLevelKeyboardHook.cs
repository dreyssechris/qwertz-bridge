using System.ComponentModel;
using System.Runtime.InteropServices;
using QwertzBridge.Core.Domain;

namespace QwertzBridge.Infrastructure;

// Installs a global WH_KEYBOARD_LL hook and forwards every key event to Handler.
// Must be installed on a thread with a running message loop (the WinForms UI thread).
public sealed class LowLevelKeyboardHook : IDisposable
{
    private readonly NativeMethods.LowLevelKeyboardProc _callback;
    private IntPtr _handle;

    public LowLevelKeyboardHook()
    {
        // Held in a field so the GC cannot collect the delegate while the hook is installed.
        _callback = HookCallback;
    }

    // Receives each key event; return true to suppress the original event.
    public Func<KeyInput, bool>? Handler { get; set; }

    public bool IsInstalled => _handle != IntPtr.Zero;

    public void Install()
    {
        if (IsInstalled)
            return;

        _handle = NativeMethods.SetWindowsHookExW(
            NativeMethods.WH_KEYBOARD_LL, _callback, NativeMethods.GetModuleHandleW(null), 0);

        if (_handle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SetWindowsHookEx(WH_KEYBOARD_LL) failed.");
    }

    public void Uninstall()
    {
        if (!IsInstalled)
            return;

        NativeMethods.UnhookWindowsHookEx(_handle);
        _handle = IntPtr.Zero;
    }

    public void Dispose() => Uninstall();

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && Handler is { } handler)
        {
            var msg = (int)wParam;
            var isDown = msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN;
            var isUp = msg is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP;

            if (isDown || isUp)
            {
                var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                var input = new KeyInput(
                    // Masking to the low byte normalizes AltGr's synthetic LCtrl (0x21D) to 0x1D.
                    ScanCode: (ushort)(data.scanCode & 0xFF),
                    IsExtended: (data.flags & NativeMethods.LLKHF_EXTENDED) != 0,
                    IsKeyDown: isDown,
                    IsInjected: (data.flags & NativeMethods.LLKHF_INJECTED) != 0);

                bool suppress;
                try
                {
                    suppress = handler(input);
                }
                catch
                {
                    // A failing handler must never break global keyboard input.
                    suppress = false;
                }

                if (suppress)
                    return 1;
            }
        }

        return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }
}
