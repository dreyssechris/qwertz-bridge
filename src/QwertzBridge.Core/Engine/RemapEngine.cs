using QwertzBridge.Core.Domain;

namespace QwertzBridge.Core.Engine;

// The remap pipeline. Tracks the AltGr state (Windows reports AltGr as LCtrl + RAlt)
// and decides per event whether it passes through or is replaced by output text.
// ProcessKey runs on the hook thread and must stay fast: no I/O. The config reference
// is swapped atomically, so hot reloads from other threads are safe.
public sealed class RemapEngine
{
    private volatile BridgeConfig _config;
    private volatile bool _enabled = true;

    private bool _lCtrlDown;
    private bool _rAltDown;
    private readonly HashSet<int> _suppressedKeys = [];

    public RemapEngine(BridgeConfig config) => _config = config;

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public bool IsAltGrActive => _lCtrlDown && _rAltDown;

    public void UpdateConfig(BridgeConfig config) => _config = config;

    public KeyDecision ProcessKey(KeyInput input)
    {
        // Our own SendInput output (including the modifier release/restore) must never
        // be remapped or change the tracked modifier state.
        if (input.IsInjected)
            return KeyDecision.PassThrough;

        TrackModifiers(input);

        if (!input.IsKeyDown)
        {
            // If we suppressed the key-down, swallow the key-up too, even if AltGr was
            // released or the engine disabled in between. No orphan key-ups.
            return _suppressedKeys.Remove(KeyId(input))
                ? KeyDecision.SuppressOnly
                : KeyDecision.PassThrough;
        }

        if (!_enabled || IsModifier(input))
            return KeyDecision.PassThrough;

        var rule = FindRule(_config, input, IsAltGrActive);
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

    private static RemapRule? FindRule(BridgeConfig config, KeyInput input, bool altGrActive)
    {
        foreach (var rule in config.Rules)
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
