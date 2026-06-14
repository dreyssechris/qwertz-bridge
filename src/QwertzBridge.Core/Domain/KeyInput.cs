namespace QwertzBridge.Core.Domain;

// A low-level keyboard event, normalized for the engine. ScanCode is the low byte
// only (e.g. 0x33 for the comma key); IsInjected marks events we sent ourselves.
public readonly record struct KeyInput(
    ushort ScanCode,
    bool IsExtended, 
    bool IsKeyDown, 
    bool IsInjected = false
);
