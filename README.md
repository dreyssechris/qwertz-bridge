# QWERTZ-Bridge

**Type `<`, `>` and `|` on an ANSI (US) keyboard running the German QWERTZ layout.**
A small portable Windows tray app for keyboards without the ISO 102 key, such as the
Ducky One 3 SF. It remaps free AltGr combos instead of forcing you to copy-paste
angle brackets.

[![CI](https://github.com/chrisdreysse/qwertz-bridge/actions/workflows/ci.yml/badge.svg)](https://github.com/chrisdreysse/qwertz-bridge/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/chrisdreysse/qwertz-bridge)](https://github.com/chrisdreysse/qwertz-bridge/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> Deutsche Kurzanleitung: [README.de.md](README.de.md)

![Tray menu screenshot placeholder](docs/screenshot-tray.png)
<!-- TODO: screenshot or GIF of the tray menu and a remap in action -->

## The problem

Physical ANSI (US) keyboards lack the ISO 102 key between left Shift and `Y`/`Z`.
On the German layout that key is the only source of `<`, `>` and `|`. Painful if you
write C#, shell pipes, generics or HTML all day.

## The solution

QWERTZ-Bridge installs a low-level keyboard hook and adds three AltGr combos that are
unused on the German layout:

| Key combo (physical key) | Output |
|---|---|
| AltGr + `,` (comma) | `<` |
| AltGr + `.` (period) | `>` |
| AltGr + `-` (right of period, ANSI `/`) | `\|` |

All standard AltGr characters (`@ € { } [ ] \ ~`) keep working. The defaults only use
combos that produce nothing on a stock German layout.

## Features

- Zero config: start the EXE, the default remaps work immediately
- Portable: one self-contained EXE, config lives next to it, no installer, no admin rights
- Per-application profiles via process name
- Hot reload: edit the JSON config, changes apply instantly. Invalid configs fall back
  to the defaults with a notification, the app never crashes over a typo
- Tray control: toggle on/off (the icon shows the state), open config, autostart, exit
- Autostart via per-user registry Run key that follows the EXE when you move it
- `QwertzBridge.exe --selftest` verifies the remap pipeline from the CLI

## Quickstart

1. Download `QwertzBridge.exe` from the [latest release](https://github.com/chrisdreysse/qwertz-bridge/releases/latest)
2. Run it. A tray notification shows the active key combos
3. Press `AltGr + ,` anywhere and you get `<`

Optional: enable *Start with Windows* in the tray menu.

## Configuration

On first run, `qwertzbridge.json` is created next to the EXE:

```json
{
  "profiles": [
    {
      "name": "Default",
      "processNames": [],
      "rules": [
        { "scanCode": "0x33", "altGr": true, "output": "<" },
        { "scanCode": "0x34", "altGr": true, "output": ">" },
        { "scanCode": "0x35", "altGr": true, "output": "|" }
      ]
    }
  ]
}
```

- `processNames` limits a profile to specific applications (`"devenv"`, `"code.exe"`).
  An empty list makes it the catch-all profile. The first matching profile wins.
- `scanCode` identifies the physical key independent of the Windows layout, as hex
  string (`"0x33"`) or decimal number (`51`). Comments and trailing commas are allowed.
- `altGr: true` (default) fires only while AltGr is held, `false` remaps the plain key.
- `output` is any text, including Unicode and multiple characters.

Example with a per-application profile:

```json
{
  "profiles": [
    {
      "name": "Terminal",
      "processNames": ["WindowsTerminal"],
      "rules": [
        { "scanCode": "0x35", "output": " | " }
      ]
    },
    {
      "name": "Default",
      "processNames": [],
      "rules": [
        { "scanCode": "0x33", "output": "<" },
        { "scanCode": "0x34", "output": ">" },
        { "scanCode": "0x35", "output": "|" }
      ]
    }
  ]
}
```

Useful scan codes:

| Physical key (ANSI) | Scan code |
|---|---|
| `,` | `0x33` |
| `.` | `0x34` |
| `/` (German: `-`) | `0x35` |
| `;` (German: `ö`) | `0x27` |
| `'` (German: `ä`) | `0x28` |
| `[` (German: `ü`) | `0x1A` |

## Why not AutoHotkey or PowerToys?

Both are excellent tools. This app exists because neither fits this exact niche well:

- **PowerToys Keyboard Manager** remaps keys or shortcuts to other shortcuts. Sending a
  layout-independent literal character on AltGr+key without breaking the existing AltGr
  assignments is exactly the case it handles poorly. It is also a large install.
- **AutoHotkey** can do it, but you carry a scripting runtime and a script per machine,
  and the AltGr edge cases (synthetic LCtrl, modifier handling around sends) are easy
  to get subtly wrong.
- **QWERTZ-Bridge** is one portable EXE with tested AltGr semantics, per-app profiles
  and a config file you can copy between machines.

## FAQ

**My antivirus or SmartScreen complains.**
The app installs a global keyboard hook (`WH_KEYBOARD_LL`), the same API every remapping
tool uses. Heuristic scanners sometimes flag unsigned single-file EXEs that hook the
keyboard. The code is open source, build it yourself with `./publish.ps1` if in doubt.
QWERTZ-Bridge never logs keys and never sends anything anywhere.

**Does it need admin rights?**
No. The hook, the config and the autostart entry are all per-user. As a consequence,
keystrokes are not remapped inside elevated (admin) windows, Windows blocks that
by design.

**Does it work in games?**
Games that read raw input bypass low-level hooks. Disable via tray double-click if a
game misbehaves.

**Why scan codes instead of letters?**
Scan codes identify the physical key regardless of the active Windows layout. That is
the whole point on a mismatched physical/logical layout setup.

**AltGr + comma types `<` twice or not at all.**
Check that no other remapping tool (AutoHotkey, PowerToys Keyboard Manager, vendor
software) touches the same combo.

## Building from source

```powershell
dotnet build          # build everything
dotnet test           # run the unit tests
./publish.ps1         # tests + portable single-file EXE into ./dist
```

Requires the .NET 8 SDK or newer. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for
the internals and [CONTRIBUTING.md](CONTRIBUTING.md) for how to contribute.

## License

[MIT](LICENSE)
