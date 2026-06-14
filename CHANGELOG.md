# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2026-06-15

### Changed

- Simplified the configuration to a single flat list of rules. Per-application
  profiles and the foreground-process lookup were removed.
- Removed unused abstraction interfaces (`ITextOutput`, `IAutostartManager`,
  `IForegroundProcessProvider`); the concrete classes stayed.
- Trimmed the XML documentation comments and stopped generating the docs file.
- Upgraded the target framework from .NET 8 to .NET 10 (LTS). No user-facing
  change, the published EXE remains self-contained.

### Added

- Executable metadata (product, company, copyright, version) and an application icon.

### Fixed

- A held modifier could stay stuck after a remap, so the keyboard then triggered
  shortcuts instead of typing. The replacement character is now injected with
  KEYEVENTF_UNICODE without releasing and re-pressing the AltGr modifiers.

### Note

- The config format changed: rules now live at the top level
  (`{ "rules": [ ... ] }`) instead of inside profiles. Existing profile-based
  configs fall back to the built-in defaults.

## [1.0.0] - 2026-06-12

### Added

- Remap engine matching physical keys by scan code, with correct AltGr
  (LCtrl + RAlt) detection
- Default rules: AltGr + `,` → `<`, AltGr + `.` → `>`, AltGr + `-` → `|`
- JSON configuration next to the EXE with hot reload and safe fallback to
  defaults on invalid input
- Tray icon with enable/disable toggle, config shortcut and autostart option
  (per-user, no admin rights)
- `--selftest` CLI mode that verifies the remap pipeline with simulated events
- Portable single-file publish via `publish.ps1` and GitHub Actions release
  workflow on tag push

[Unreleased]: https://github.com/dreyssechris/qwertz-bridge/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/dreyssechris/qwertz-bridge/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/dreyssechris/qwertz-bridge/releases/tag/v1.0.0
