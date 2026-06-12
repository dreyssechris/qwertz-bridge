# CLAUDE.md

## Project overview

QWERTZ-Bridge is a portable Windows tray app (C#/.NET 8) that makes `<`, `>` and `|`
typeable on physical ANSI (US) keyboards used with the German QWERTZ layout, where the
ISO 102 key is missing. It installs a `WH_KEYBOARD_LL` hook, matches physical keys by
scan code and replaces configured AltGr combos with text sent via `SendInput`
(KEYEVENTF_UNICODE).

## Build and test commands

```powershell
dotnet build                                            # build the solution
dotnet test                                             # run all unit tests (xunit, Core only)
dotnet run --project src/QwertzBridge.App               # run the tray app from source
dotnet run --project src/QwertzBridge.App -- --selftest # CLI self-test of the remap pipeline
./publish.ps1                                           # tests + portable single-file EXE into ./dist
```

CI (`.github/workflows/ci.yml`) builds and tests every push and PR. A `v*` tag
additionally publishes the EXE, runs `--selftest` against it and creates a GitHub
release.

## Architecture

Three layers, dependencies point inwards (details in `docs/ARCHITECTURE.md`):

- `src/QwertzBridge.Core` (net8.0, no Windows deps): domain types, `RemapEngine`,
  `ProfileResolver`, `ConfigLoader`, `SelfTestRunner`, interfaces for all OS access.
- `src/QwertzBridge.Infrastructure` (net8.0-windows): keyboard hook, SendInput,
  foreground process lookup, registry autostart, config file store with hot reload.
- `src/QwertzBridge.App` (net8.0-windows, WinForms): composition root, tray UI,
  `--selftest` entry point.

## Key decisions, including pragmatic calls

- AltGr counts as active when LCtrl and RAlt are both down. Windows sends a synthetic
  LCtrl (scan code 0x21D in the LL hook) with AltGr; scan codes are masked to the low
  byte so it counts as a normal LCtrl. RAlt alone, LCtrl+LAlt and RCtrl combos do not
  trigger remaps.
- Rule matching uses scan code, extended flag and AltGr state. `extended` defaults to
  false, so numpad divide (0x35 + extended) never collides with the slash key (0x35).
- Injected events (`LLKHF_INJECTED`) always pass through. This prevents feedback loops
  with our own SendInput and avoids fighting other automation tools.
- Around text output, RAlt and LCtrl are released, the unicode char is sent, then both
  are pressed again because the user still holds them physically.
- Key-ups of suppressed key-downs are swallowed too, even after AltGr was released or
  the engine was disabled in between. No orphan key-up events.
- Config errors never crash: invalid JSON or validation failure means built-in defaults
  plus a tray balloon. Hot reload uses the same path. The parser accepts comments,
  trailing commas, hex (`"0x33"`) or decimal scan codes.
- Profile resolution: first profile whose `processNames` matches the foreground process
  wins, empty `processNames` is the catch-all, no match at all yields an empty profile
  (no remapping). Names compared case-insensitively, `.exe` suffix ignored.
- The tray menu shows the most recently resolved profile. Live resolution when the menu
  opens would be misleading because the foreground window is then the tray itself.
- UI and repo language is English per spec, `README.de.md` carries a German quickstart.
  License holder inferred from the local environment.
- Badge and release URLs assume `github.com/chrisdreysse/qwertz-bridge`. Adjust README,
  CHANGELOG and CONTRIBUTING if the repo lands elsewhere.

## Known limitations

- No remapping inside elevated (admin) windows. Windows blocks non-elevated LL hooks
  there by design; running QWERTZ-Bridge elevated would work but is not recommended.
- Games using raw input or exclusive fullscreen may bypass the hook.
- Hook and engine state live on the UI thread. `Enabled` and the config reference are
  safe to touch cross-thread, the rest is not (and does not need to be).
- A few exotic apps ignore synthetic unicode input (`VK_PACKET`).
- Per-device distinction (multiple keyboards) is out of scope.

## Ideas for "good first issue" (deliberately not implemented)

- Small window visualizing the active remaps on a keyboard graphic.
- Double-tap detection as an alternative trigger.
- Config schema (`$schema`) for editor auto-completion in `qwertzbridge.json`.
- Localized tray UI (German) based on the Windows display language.
