using QwertzBridge.Core.Config;
using QwertzBridge.Core.Domain;
using QwertzBridge.Core.Engine;

namespace QwertzBridge.Core.SelfTest;

public sealed record SelfTestResult(bool Success, IReadOnlyList<string> Lines);

// Drives the remap pipeline with simulated events (no hook, no SendInput) and checks
// the expected decisions. Backs the --selftest CLI mode.
public static class SelfTestRunner
{
    public static SelfTestResult Run()
    {
        var lines = new List<string>();
        var success = true;

        void Check(string name, bool passed, string detail)
        {
            success &= passed;
            lines.Add($"[{(passed ? "PASS" : "FAIL")}] {name} ({detail})");
        }

        var defaults = ConfigLoader.Parse(ConfigLoader.SerializeDefault());
        Check("Default config round-trips", !defaults.UsedFallback && defaults.Config.Rules.Count == 3,
            "serialize + parse without fallback");

        var broken = ConfigLoader.Parse("{ this is not json");
        Check("Broken config falls back to defaults", broken.UsedFallback && broken.Config.Rules.Count == 3,
            $"error: {broken.Error}");

        Check("AltGr + comma produces \"<\"", RemapWithAltGr(ScanCodes.Comma) == "<", "scan code 0x33");
        Check("AltGr + period produces \">\"", RemapWithAltGr(ScanCodes.Period) == ">", "scan code 0x34");
        Check("AltGr + slash key produces \"|\"", RemapWithAltGr(ScanCodes.Slash) == "|", "scan code 0x35, German layout '-'");

        var engine = NewEngine();
        PressAltGr(engine);
        var down = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        var up = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, false));
        Check("Key-up of a remapped key is swallowed", down.Suppress && up.Suppress && up.Output is null,
            "no orphan key-up reaches the system");

        engine = NewEngine();
        var plain = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        Check("Comma without AltGr passes through", !plain.Suppress, "normal typing is untouched");

        engine = NewEngine();
        engine.ProcessKey(new KeyInput(ScanCodes.Ctrl, false, true));
        var ctrlOnly = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        Check("LCtrl + comma passes through", !ctrlOnly.Suppress, "AltGr requires LCtrl AND RAlt");

        engine = NewEngine();
        engine.ProcessKey(new KeyInput(ScanCodes.Alt, true, true));
        var rAltOnly = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        Check("RAlt without LCtrl passes through", !rAltOnly.Suppress, "AltGr requires LCtrl AND RAlt");

        engine = NewEngine();
        PressAltGr(engine);
        var altGrQ = engine.ProcessKey(new KeyInput(0x10, false, true));
        Check("AltGr + Q passes through (standard '@')", !altGrQ.Suppress, "standard AltGr characters keep working");

        engine = NewEngine();
        PressAltGr(engine);
        var numpadDivide = engine.ProcessKey(new KeyInput(ScanCodes.Slash, true, true));
        Check("AltGr + numpad divide passes through", !numpadDivide.Suppress, "extended 0x35 is not the slash key");

        engine = NewEngine();
        PressAltGr(engine);
        var injected = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true, IsInjected: true));
        Check("Injected events pass through", !injected.Suppress, "no feedback loop with SendInput");

        engine = NewEngine();
        engine.Enabled = false;
        PressAltGr(engine);
        var disabled = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        Check("Disabled engine passes everything through", !disabled.Suppress, "tray toggle off");

        engine = NewEngine();
        PressAltGr(engine);
        var first = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        var repeat = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        Check("Key auto-repeat keeps remapping", first.Output == "<" && repeat.Output == "<",
            "holding the key repeats the output");

        return new SelfTestResult(success, lines);
    }

    private static RemapEngine NewEngine() => new(BridgeConfig.CreateDefault());

    private static void PressAltGr(RemapEngine engine)
    {
        engine.ProcessKey(new KeyInput(ScanCodes.Ctrl, false, true));
        engine.ProcessKey(new KeyInput(ScanCodes.Alt, true, true));
    }

    private static string? RemapWithAltGr(ushort scanCode)
    {
        var engine = NewEngine();
        PressAltGr(engine);
        return engine.ProcessKey(new KeyInput(scanCode, false, true)).Output;
    }
}
