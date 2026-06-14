using Microsoft.Win32;

namespace QwertzBridge.Infrastructure;

// Registers the app in the per-user Run key (HKCU), no admin rights required. Because
// the EXE is portable, SyncPathIfEnabled rewrites the stored path on startup in case
// the file was moved.
public sealed class AutostartManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "QwertzBridge";

    private static string QuotedExePath => $"\"{Environment.ProcessPath}\"";

    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is not null;
        }
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        key.SetValue(ValueName, QuotedExePath);
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    public void SyncPathIfEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        
        if (key?.GetValue(ValueName) is string registered && registered != QuotedExePath)
            key.SetValue(ValueName, QuotedExePath);
    }
}
