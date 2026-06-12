namespace QwertzBridge.Core.Abstractions;

/// <summary>Sends replacement text as synthetic keyboard input.</summary>
public interface ITextOutput
{
    /// <summary>Sends the given text as keyboard input.</summary>
    /// <param name="text">The text to type.</param>
    /// <param name="altGrIsDown">
    /// True if AltGr (LCtrl + RAlt) is physically held right now; the implementation then
    /// temporarily releases the modifiers so the text arrives unmodified, and restores them afterwards.
    /// </param>
    void Send(string text, bool altGrIsDown);
}
