namespace QwertzBridge.Core.Abstractions;

/// <summary>Manages the per-user autostart registration (no admin rights required).</summary>
public interface IAutostartManager
{
    /// <summary>Whether autostart is currently registered.</summary>
    bool IsEnabled { get; }

    /// <summary>Registers the current executable for autostart.</summary>
    void Enable();

    /// <summary>Removes the autostart registration.</summary>
    void Disable();

    /// <summary>Updates the registered path if the (portable) executable has been moved.</summary>
    void SyncPathIfEnabled();
}
