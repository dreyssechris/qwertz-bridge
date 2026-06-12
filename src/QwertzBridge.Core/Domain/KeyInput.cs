namespace QwertzBridge.Core.Domain;

/// <summary>A single low-level keyboard event, normalized for the remap engine.</summary>
/// <param name="ScanCode">Hardware scan code of the physical key (low byte only, e.g. 0x33 for the comma key).</param>
/// <param name="IsExtended">Whether the extended-key flag is set (distinguishes e.g. RAlt from LAlt).</param>
/// <param name="IsKeyDown">True for key-down (including auto-repeat), false for key-up.</param>
/// <param name="IsInjected">True if the event was injected via SendInput (e.g. by this app itself).</param>
public readonly record struct KeyInput(ushort ScanCode, bool IsExtended, bool IsKeyDown, bool IsInjected = false);
