using QwertzBridge.Core.Domain;
using static QwertzBridge.Core.Tests.TestHelpers;

namespace QwertzBridge.Core.Tests;

public class RemapEngineTests
{
    [Theory]
    [InlineData(ScanCodes.Comma, "<")]
    [InlineData(ScanCodes.Period, ">")]
    [InlineData(ScanCodes.Slash, "|")]
    public void AltGrPlusDefaultKeys_AreRemapped(ushort scanCode, string expected)
    {
        var engine = CreateEngine();
        engine.PressAltGr();

        var decision = engine.ProcessKey(Down(scanCode));

        Assert.True(decision.Suppress);
        Assert.Equal(expected, decision.Output);
    }

    [Theory]
    [InlineData(ScanCodes.Comma)]
    [InlineData(ScanCodes.Period)]
    [InlineData(ScanCodes.Slash)]
    public void WithoutAltGr_DefaultKeysPassThrough(ushort scanCode)
    {
        var engine = CreateEngine();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(scanCode)));
    }

    [Fact]
    public void LCtrlAlone_DoesNotActivateAltGr()
    {
        var engine = CreateEngine();
        engine.ProcessKey(Down(ScanCodes.Ctrl));

        Assert.False(engine.IsAltGrActive);
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma)));
    }

    [Fact]
    public void RAltAlone_DoesNotActivateAltGr()
    {
        // Windows always reports AltGr as LCtrl + RAlt; RAlt by itself must not trigger.
        var engine = CreateEngine();
        engine.ProcessKey(Down(ScanCodes.Alt, extended: true));

        Assert.False(engine.IsAltGrActive);
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma)));
    }

    [Fact]
    public void LCtrlPlusLAlt_DoesNotActivateAltGr()
    {
        // Left Alt is non-extended; Ctrl + left Alt must not be mistaken for AltGr.
        var engine = CreateEngine();
        engine.ProcessKey(Down(ScanCodes.Ctrl));
        engine.ProcessKey(Down(ScanCodes.Alt, extended: false));

        Assert.False(engine.IsAltGrActive);
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma)));
    }

    [Fact]
    public void RCtrl_DoesNotCountAsAltGrCtrl()
    {
        // RCtrl is extended 0x1D; only LCtrl participates in AltGr.
        var engine = CreateEngine();
        engine.ProcessKey(Down(ScanCodes.Ctrl, extended: true));
        engine.ProcessKey(Down(ScanCodes.Alt, extended: true));

        Assert.False(engine.IsAltGrActive);
    }

    [Fact]
    public void AltGrPlusUnmappedKey_PassesThrough()
    {
        // AltGr + Q produces '@' on German layouts and must stay untouched.
        var engine = CreateEngine();
        engine.PressAltGr();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(0x10)));
    }

    [Fact]
    public void AltGrPlusNumpadDivide_PassesThrough()
    {
        // Numpad divide shares scan code 0x35 with the slash key but has the extended flag.
        var engine = CreateEngine();
        engine.PressAltGr();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Slash, extended: true)));
    }

    [Fact]
    public void KeyUpOfRemappedKey_IsSuppressedWithoutOutput()
    {
        var engine = CreateEngine();
        engine.PressAltGr();
        engine.ProcessKey(Down(ScanCodes.Comma));

        var up = engine.ProcessKey(Up(ScanCodes.Comma));

        Assert.True(up.Suppress);
        Assert.Null(up.Output);
    }

    [Fact]
    public void KeyUpOfRemappedKey_IsSuppressedEvenAfterAltGrWasReleased()
    {
        // AltGr-up can arrive before the remapped key's key-up; no orphan key-up may leak out.
        var engine = CreateEngine();
        engine.PressAltGr();
        engine.ProcessKey(Down(ScanCodes.Comma));
        engine.ReleaseAltGr();

        Assert.True(engine.ProcessKey(Up(ScanCodes.Comma)).Suppress);
    }

    [Fact]
    public void KeyUpOfNormalKey_PassesThrough()
    {
        var engine = CreateEngine();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Up(ScanCodes.Comma)));
    }

    [Fact]
    public void AutoRepeat_KeepsRemappingWhileAltGrHeld()
    {
        var engine = CreateEngine();
        engine.PressAltGr();

        Assert.Equal("<", engine.ProcessKey(Down(ScanCodes.Comma)).Output);
        Assert.Equal("<", engine.ProcessKey(Down(ScanCodes.Comma)).Output);
        Assert.Equal("<", engine.ProcessKey(Down(ScanCodes.Comma)).Output);
    }

    [Fact]
    public void AfterAltGrRelease_KeysPassThroughAgain()
    {
        var engine = CreateEngine();
        engine.PressAltGr();
        engine.ReleaseAltGr();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma)));
    }

    [Fact]
    public void InjectedEvents_PassThroughAndDoNotAffectModifierState()
    {
        var engine = CreateEngine();

        // Injected modifier events (our own SendInput release/restore) must not change state.
        engine.ProcessKey(Down(ScanCodes.Ctrl, injected: true));
        engine.ProcessKey(Down(ScanCodes.Alt, extended: true, injected: true));

        Assert.False(engine.IsAltGrActive);
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma, injected: true)));
    }

    [Fact]
    public void DisabledEngine_PassesEverythingThrough()
    {
        var engine = CreateEngine();
        engine.Enabled = false;
        engine.PressAltGr();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma)));
    }

    [Fact]
    public void ReenabledEngine_RemapsAgain()
    {
        var engine = CreateEngine();
        engine.Enabled = false;
        engine.PressAltGr();
        engine.ProcessKey(Down(ScanCodes.Comma));

        engine.Enabled = true;

        Assert.Equal("<", engine.ProcessKey(Down(ScanCodes.Comma)).Output);
    }

    [Fact]
    public void ModifierKeysThemselves_AreNeverSuppressed()
    {
        var engine = CreateEngine();

        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Ctrl)));
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Alt, extended: true)));
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Up(ScanCodes.Alt, extended: true)));
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Up(ScanCodes.Ctrl)));
    }

    [Fact]
    public void UpdateConfig_TakesEffectImmediately()
    {
        var engine = CreateEngine();
        engine.PressAltGr();
        Assert.Equal("<", engine.ProcessKey(Down(ScanCodes.Comma)).Output);
        engine.ProcessKey(Up(ScanCodes.Comma));

        engine.UpdateConfig(new BridgeConfig
        {
            Rules = [new RemapRule { ScanCode = ScanCodes.Comma, Output = "≤" }],
        });

        Assert.Equal("≤", engine.ProcessKey(Down(ScanCodes.Comma)).Output);
    }

    [Fact]
    public void PlainRule_RemapsWithoutAltGr()
    {
        var config = new BridgeConfig
        {
            Rules = [new RemapRule { ScanCode = ScanCodes.Comma, AltGr = false, Output = ";" }],
        };
        var engine = CreateEngine(config);

        Assert.Equal(";", engine.ProcessKey(Down(ScanCodes.Comma)).Output);

        // With AltGr held, the plain rule must not fire.
        engine.ProcessKey(Up(ScanCodes.Comma));
        engine.PressAltGr();
        Assert.Equal(KeyDecision.PassThrough, engine.ProcessKey(Down(ScanCodes.Comma)));
    }
}
