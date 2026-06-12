# Architecture

QWERTZ-Bridge uses a deliberately small layered structure:

```
src/
  QwertzBridge.Core            net10.0         pure logic, no Windows dependencies
    Domain/                    KeyInput, KeyDecision, RemapRule, Profile, BridgeConfig, ScanCodes
    Engine/                    RemapEngine, ProfileResolver
    Config/                    ConfigLoader (parse + validate, never throws), ScanCodeJsonConverter
    Abstractions/              IForegroundProcessProvider, ITextOutput, IAutostartManager
    SelfTest/                  SelfTestRunner (drives the pipeline with simulated events)

  QwertzBridge.Infrastructure  net10.0-windows all Win32 and OS access
    LowLevelKeyboardHook       WH_KEYBOARD_LL via SetWindowsHookEx
    SendInputTextOutput        SendInput with KEYEVENTF_UNICODE
    ForegroundProcessProvider  GetForegroundWindow + process name, cached per window handle
    AutostartManager           HKCU Run key
    ConfigStore                config file next to the EXE, FileSystemWatcher hot reload

  QwertzBridge.App             net10.0-windows WinForms tray app, composition root
    Program                    entry point, --selftest mode, single-instance mutex
    TrayApplicationContext     wires store, engine, hook and output; tray menu
    TrayIcons                  draws the active/paused icons at runtime

tests/
  QwertzBridge.Core.Tests      xunit; engine, config parsing, profile resolution, self-test
```

Dependencies point inwards only: App depends on Infrastructure and Core, Infrastructure
depends on Core. Core has no Windows dependency at all. The hook, SendInput and the
registry sit behind the interfaces in `Core/Abstractions`, which is what makes the
engine fully unit-testable.

## Data flow of a key press

Example: AltGr is held and the user presses the physical comma key.

1. Windows calls the `WH_KEYBOARD_LL` callback in `LowLevelKeyboardHook` on the UI
   thread, which runs the message loop.
2. The hook normalizes the raw `KBDLLHOOKSTRUCT` into a `KeyInput`: scan code masked
   to the low byte, extended flag, key down/up, injected flag.
3. `TrayApplicationContext.OnKeyEvent` passes the `KeyInput` to `RemapEngine.ProcessKey`.
4. The engine ignores injected events, tracks LCtrl/RAlt state (AltGr means both are
   down), resolves the active profile from the foreground process name and looks for a
   rule matching scan code, extended flag and AltGr state. For the comma it returns
   "suppress, output `<`" and remembers the key so the matching key-up is swallowed too.
5. The app calls `SendInputTextOutput.Send("<", altGrIsDown: true)`. One SendInput batch
   releases RAlt and LCtrl, types the unicode `<`, then presses LCtrl and RAlt again
   because the user is still physically holding them.
6. The hook callback returns 1 and Windows drops the original comma event.

## Design decisions

- **AltGr detection requires LCtrl down and RAlt down.** Windows synthesizes an LCtrl
  press for AltGr; it arrives in the hook with scan code 0x21D, and masking scan codes
  to the low byte turns it into a regular LCtrl (0x1D). Requiring both keys means left
  Ctrl+Alt, RAlt alone and RCtrl combos never trigger remaps.
- **Matching by scan code, not virtual key.** Scan codes identify the physical key
  independent of the active layout. That is the core requirement of this tool.
- **The extended flag is part of the match.** Numpad divide shares scan code 0x35 with
  the slash key but carries the extended flag. Rules default to `extended: false`, so
  AltGr+numpad-divide stays untouched.
- **Injected events pass through untouched.** Our own SendInput output, including the
  modifier release/restore, re-enters the hook flagged `LLKHF_INJECTED`. Skipping those
  events prevents feedback loops and keeps the engine's modifier state correct.
- **Modifiers are released and restored around the unicode send.** Otherwise the target
  app would see the output character with Ctrl+Alt held and might treat it as a chord.
- **Key-ups of remapped keys are swallowed**, even if AltGr was released in between.
  No application ever sees a key-up without the matching key-down.
- **No I/O in the hook callback.** Profile resolution uses a per-window cache; config
  reloads happen on watcher threads and swap an immutable config reference.
- **Config errors never crash.** `ConfigLoader.Parse` returns the defaults plus an error
  message and the tray shows a balloon. Hot reloads use the same path.
- **First matching profile wins; an empty `processNames` list is the catch-all.** If no
  profile applies at all, an empty profile without rules is used and remapping pauses.

## Adding a new remap

No code needed. Add a rule to `qwertzbridge.json` (see README); the app reloads it live.

## Adding a new feature

- Engine behavior: `Core/Engine`, covered by tests in `tests/`, plus a check in
  `SelfTestRunner` if it concerns the remap pipeline.
- New OS interaction: interface in `Core/Abstractions`, implementation in
  `Infrastructure`, wiring in `TrayApplicationContext`.
- Tray UI: `TrayApplicationContext` only.

## Release

`publish.ps1` produces `dist/QwertzBridge.exe` (self-contained single file, win-x64).
Pushing a `v*` tag runs CI (build + tests), publishes the EXE, runs `--selftest` against
the published binary and attaches it to a GitHub release.
