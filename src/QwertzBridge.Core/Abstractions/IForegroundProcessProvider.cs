namespace QwertzBridge.Core.Abstractions;

/// <summary>Resolves the process name of the current foreground window.</summary>
public interface IForegroundProcessProvider
{
    /// <summary>Returns the foreground process name without ".exe" (e.g. "devenv"), or null if unknown.</summary>
    string? GetForegroundProcessName();
}
