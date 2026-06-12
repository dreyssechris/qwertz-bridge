using QwertzBridge.Core.Abstractions;
using QwertzBridge.Core.Domain;
using QwertzBridge.Core.Engine;

namespace QwertzBridge.Core.Tests;

internal sealed class FakeForeground : IForegroundProcessProvider
{
    public string? Name { get; set; }

    public string? GetForegroundProcessName() => Name;
}

internal static class TestHelpers
{
    public static RemapEngine CreateEngine(BridgeConfig? config = null, string? processName = null) =>
        new(config ?? BridgeConfig.CreateDefault(), new FakeForeground { Name = processName });

    /// <summary>Simulates AltGr going down the way Windows reports it: LCtrl first, then extended RAlt.</summary>
    public static void PressAltGr(this RemapEngine engine)
    {
        engine.ProcessKey(Down(ScanCodes.Ctrl));
        engine.ProcessKey(Down(ScanCodes.Alt, extended: true));
    }

    public static void ReleaseAltGr(this RemapEngine engine)
    {
        engine.ProcessKey(Up(ScanCodes.Ctrl));
        engine.ProcessKey(Up(ScanCodes.Alt, extended: true));
    }

    public static KeyInput Down(ushort scanCode, bool extended = false, bool injected = false) =>
        new(scanCode, extended, IsKeyDown: true, IsInjected: injected);

    public static KeyInput Up(ushort scanCode, bool extended = false, bool injected = false) =>
        new(scanCode, extended, IsKeyDown: false, IsInjected: injected);
}
