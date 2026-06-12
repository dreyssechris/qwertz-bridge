# Contributing to QWERTZ-Bridge

Thanks for your interest. This project is intentionally small; please keep contributions
in that spirit: as much as necessary, as little as possible.

## Dev setup

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- Any editor (Visual Studio, Rider, VS Code with C# Dev Kit)

```powershell
git clone https://github.com/chrisdreysse/qwertz-bridge.git
cd qwertz-bridge
dotnet build
dotnet test
```

Run the app from source: `dotnet run --project src/QwertzBridge.App`
Run the pipeline self-test: `dotnet run --project src/QwertzBridge.App -- --selftest`
Build the portable EXE: `./publish.ps1`

## Project layout

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). The short version: `Core` is pure,
testable logic, `Infrastructure` is all Win32, `App` is the tray shell. Keep it that
way; new OS calls go behind an interface in `Core/Abstractions`.

## Branches and commits

- Branch from `main`, name it `feat/...`, `fix/...` or `docs/...`
- Use [Conventional Commits](https://www.conventionalcommits.org/) in English:
  `feat: add double-tap trigger`, `fix: swallow key-up after AltGr release`
- One logical change per commit

## Pull requests

1. `dotnet build` and `dotnet test` must pass; CI runs both on every PR
2. Add or adjust unit tests for engine and config changes. Edge cases matter here:
   AltGr detection, key repeat, and foreign key combos must stay untouched
3. If you changed the remap pipeline, verify `--selftest` still passes
4. Fill in the PR template, including how you tested with real key presses

## Reporting bugs

Use the bug report template and include your Windows version, keyboard model, physical
layout (ANSI/ISO) and the active Windows keyboard layout. Keyboard issues are almost
always layout-specific.
