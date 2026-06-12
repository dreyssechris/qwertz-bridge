using System.Diagnostics;
using QwertzBridge.Core.Abstractions;

namespace QwertzBridge.Infrastructure;

/// <summary>
/// Resolves the foreground process name via Win32. The result is cached per window handle,
/// so repeated calls while the same window is focused are cheap (relevant for the hook hot path).
/// </summary>
public sealed class ForegroundProcessProvider : IForegroundProcessProvider
{
    private IntPtr _lastWindow;
    private string? _lastName;

    /// <inheritdoc />
    public string? GetForegroundProcessName()
    {
        var window = NativeMethods.GetForegroundWindow();
        if (window == IntPtr.Zero)
            return null;

        if (window == _lastWindow)
            return _lastName;

        string? name = null;
        try
        {
            NativeMethods.GetWindowThreadProcessId(window, out var pid);
            if (pid != 0)
            {
                using var process = Process.GetProcessById((int)pid);
                name = process.ProcessName;
            }
        }
        catch
        {
            // Process may have exited or be inaccessible; treat as unknown.
        }

        _lastWindow = window;
        _lastName = name;
        return name;
    }
}
