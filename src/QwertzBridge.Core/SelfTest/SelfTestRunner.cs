using QwertzBridge.Core.Abstractions;
using QwertzBridge.Core.Config;
using QwertzBridge.Core.Domain;
using QwertzBridge.Core.Engine;

namespace QwertzBridge.Core.SelfTest;

/// <summary>Result of a self-test run.</summary>
/// <param name="Success">True if every check passed.</param>
/// <param name="Lines">One human-readable line per check.</param>
public sealed record SelfTestResult(bool Success, IReadOnlyList<string> Lines);

/// <summary>
/// Drives the remap pipeline with simulated keyboard events (no hook, no SendInput)
/// and verifies the expected decisions. Used by the <c>--selftest</c> CLI mode.
/// </summary>
public static class SelfTestRunner
{
    /// <summary>Runs all checks and returns the aggregated result.</summary>
    public static SelfTestResult Run()
    {
        var lines = new List<string>();
        var success = true;

        void Check(string name, bool passed, string detail)
        {
            success &= passed;
            lines.Add($"[{(passed ? "PASS" : "FAIL")}] {name} ({detail})");
        }

        // --- Config pipeline ---------------------------------------------------
        var defaults = ConfigLoader.Parse(ConfigLoader.SerializeDefault());
        Check("Default config round-trips", !defaults.UsedFallback && defaults.Config.Profiles.Count == 1,
            "serialize + parse without fallback");

        var broken = ConfigLoader.Parse("{ this is not json");
        Check("Broken config falls back to defaults", broken.UsedFallback && broken.Config.Profiles.Count == 1,
            $"error: {broken.Error}");

        // --- Remap pipeline ----------------------------------------------------
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

        // --- Per-process profiles ----------------------------------------------
        var config = new BridgeConfig
        {
            Profiles =
            [
                new Profile
                {
                    Name = "VS",
                    ProcessNames = ["devenv"],
                    Rules = [new RemapRule { ScanCode = ScanCodes.Comma, Output = "X" }],
                },
                .. BridgeConfig.CreateDefault().Profiles,
            ],
        };
        var foreground = new FakeForeground { Name = "devenv" };
        engine = new RemapEngine(config, foreground);
        PressAltGr(engine);
        var inVs = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, false));
        foreground.Name = "notepad";
        var elsewhere = engine.ProcessKey(new KeyInput(ScanCodes.Comma, false, true));
        Check("Per-process profile wins over default", inVs.Output == "X" && elsewhere.Output == "<",
            "devenv → \"X\", notepad → \"<\"");

        return new SelfTestResult(success, lines);
    }

    private static RemapEngine NewEngine() =>
        new(BridgeConfig.CreateDefault(), new FakeForeground());

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

    private sealed class FakeForeground : IForegroundProcessProvider
    {
        public string? Name { get; set; }

        public string? GetForegroundProcessName() => Name;
    }
}
