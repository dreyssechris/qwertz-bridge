namespace QwertzBridge.Core.Domain;

/// <summary>The engine's verdict for a single keyboard event.</summary>
/// <param name="Suppress">Whether the original event must be swallowed.</param>
/// <param name="Output">Replacement text to send via keyboard injection, or null.</param>
public readonly record struct KeyDecision(bool Suppress, string? Output)
{
    /// <summary>Let the event pass through unchanged.</summary>
    public static KeyDecision PassThrough { get; } = new(false, null);

    /// <summary>Swallow the event without producing output (e.g. key-up of a remapped key).</summary>
    public static KeyDecision SuppressOnly { get; } = new(true, null);

    /// <summary>Swallow the event and emit <paramref name="output"/> instead.</summary>
    /// <param name="output">The replacement text.</param>
    public static KeyDecision SuppressWith(string output) => new(true, output);
}
