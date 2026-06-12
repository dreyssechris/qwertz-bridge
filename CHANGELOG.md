# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-06-12

### Added

- Remap engine matching physical keys by scan code, with correct AltGr
  (LCtrl + RAlt) detection
- Default profile: AltGr + `,` → `<`, AltGr + `.` → `>`, AltGr + `-` → `|`
- JSON configuration next to the EXE with hot reload and safe fallback to
  defaults on invalid input
- Per-application profiles via process name matching
- Tray icon with enable/disable toggle, active profile display, config
  shortcut and autostart option (per-user, no admin rights)
- `--selftest` CLI mode that verifies the remap pipeline with simulated events
- Portable single-file publish via `publish.ps1` and GitHub Actions release
  workflow on tag push

[Unreleased]: https://github.com/chrisdreysse/qwertz-bridge/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/chrisdreysse/qwertz-bridge/releases/tag/v1.0.0
