using QwertzBridge.Core.Abstractions;
using QwertzBridge.Core.Domain;

namespace QwertzBridge.Core.Engine;

/// <summary>
/// The remap pipeline. Receives normalized keyboard events, tracks the AltGr state
/// (Windows represents AltGr as LCtrl + RAlt) and decides per event whether it passes
/// through or is replaced by configured output text.
/// </summary>
/// <remarks>
/// <see cref="ProcessKey"/> is called from the low-level hook thread and must stay fast;
/// it performs no I/O. Config updates from other threads are safe (immutable snapshot swap).
/// </remarks>
public sealed class RemapEngine
{
    private readonly IForegroundProcessProvider _foreground;
    private volatile BridgeConfig _config;
    private volatile bool _enabled = true;
    private volatile string _activeProfileName;

    private bool _lCtrlDown;
    private bool _rAltDown;
    private readonly HashSet<int> _suppressedKeys = [];

    /// <summary>Creates an engine with the given configuration.</summary>
    /// <param name="config">Initial configuration.</param>
    /// <param name="foreground">Provider for the foreground process name (profile matching).</param>
    public RemapEngine(BridgeConfig config, IForegroundProcessProvider foreground)
    {
        _config = config;
        _foreground = foreground;
        _activeProfileName = ProfileResolver.Resolve(config, null).Name;
    }

    /// <summary>Master switch. When false, every event passes through.</summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>True while AltGr is held (both LCtrl and RAlt are down).</summary>
    public bool IsAltGrActive => _lCtrlDown && _rAltDown;

    /// <summary>Name of the most recently resolved profile (for display in the tray menu).</summary>
    public string ActiveProfileName => _activeProfileName;

    /// <summary>Atomically swaps in a new configuration (e.g. after a hot reload).</summary>
    /// <param name="config">The new configuration.</param>
    public void UpdateConfig(BridgeConfig config)
    {
        _config = config;
        _activeProfileName = ProfileResolver.Resolve(config, null).Name;
    }

    /// <summary>Processes one keyboard event and returns what to do with it.</summary>
    /// <param name="input">The normalized keyboard event.</param>
    public KeyDecision ProcessKey(KeyInput input)
    {
        // Our own SendInput output, including the modifier release/restore,
        // must never be remapped or change the tracked modifier state.
        if (input.IsInjected)
            return KeyDecision.PassThrough;

        TrackModifiers(input);

        if (!input.IsKeyDown)
        {
            // If we suppressed the key-down, swallow the key-up as well,
            // even if AltGr was released or the engine disabled in between.
            return _suppressedKeys.Remove(KeyId(input))
                ? KeyDecision.SuppressOnly
                : KeyDecision.PassThrough;
        }

        if (!_enabled || IsModifier(input))
            return KeyDecision.PassThrough;

        var profile = ProfileResolver.Resolve(_config, _foreground.GetForegroundProcessName());
        _activeProfileName = profile.Name;

        var rule = FindRule(profile, input, IsAltGrActive);
        if (rule is null)
            return KeyDecision.PassThrough;

        _suppressedKeys.Add(KeyId(input));
        return KeyDecision.SuppressWith(rule.Output);
    }

    private void TrackModifiers(KeyInput input)
    {
        if (input.ScanCode == ScanCodes.Ctrl && !input.IsExtended)
            _lCtrlDown = input.IsKeyDown;
        else if (input.ScanCode == ScanCodes.Alt && input.IsExtended)
            _rAltDown = input.IsKeyDown;
    }

    private static RemapRule? FindRule(Profile profile, KeyInput input, bool altGrActive)
    {
        foreach (var rule in profile.Rules)
        {
            if (rule.AltGr == altGrActive
                && rule.ScanCode == input.ScanCode
                && rule.Extended == input.IsExtended
                && rule.Output.Length > 0)
            {
                return rule;
            }
        }

        return null;
    }

    private static bool IsModifier(KeyInput input) => input.ScanCode is
        ScanCodes.Ctrl or ScanCodes.Alt or
        ScanCodes.LeftShift or ScanCodes.RightShift or
        ScanCodes.LeftWin or ScanCodes.RightWin;

    private static int KeyId(KeyInput input) => input.ScanCode | (input.IsExtended ? 0x100 : 0);
}
