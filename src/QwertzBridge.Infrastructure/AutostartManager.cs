using Microsoft.Win32;
using QwertzBridge.Core.Abstractions;

namespace QwertzBridge.Infrastructure;

/// <summary>
/// Registers the app in the per-user Run key (HKCU), which requires no admin rights.
/// Because the EXE is portable, <see cref="SyncPathIfEnabled"/> rewrites the registered
/// path on startup in case the file was moved.
/// </summary>
public sealed class AutostartManager : IAutostartManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "QwertzBridge";

    private static string QuotedExePath => $"\"{Environment.ProcessPath}\"";

    /// <inheritdoc />
    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is not null;
        }
    }

    /// <inheritdoc />
    public void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        key.SetValue(ValueName, QuotedExePath);
    }

    /// <inheritdoc />
    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    /// <inheritdoc />
    public void SyncPathIfEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key?.GetValue(ValueName) is string registered && registered != QuotedExePath)
            key.SetValue(ValueName, QuotedExePath);
    }
}
