namespace QwertzBridge.Core.Domain;

// What the engine decided for one event: whether to swallow the original,
// and what replacement text (if any) to send.
public readonly record struct KeyDecision(bool Suppress, string? Output)
{
    public static KeyDecision PassThrough { get; } = new(false, null);
    public static KeyDecision SuppressOnly { get; } = new(true, null);
    public static KeyDecision SuppressWith(string output) => new(true, output);
}
