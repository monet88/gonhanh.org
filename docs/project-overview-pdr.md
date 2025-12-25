# Gõ Nhanh: Project Overview & Product Development Requirements

## Project Vision

Gõ Nhanh is a **high-performance Vietnamese input method engine** (IME) for macOS, Windows, and Linux with a platform-agnostic Rust core. It enables fast, accurate Vietnamese text input with minimal system overhead. The project demonstrates production-grade system software design: Rust core for performance and safety, native UI (SwiftUI on macOS, WPF on Windows, Fcitx5 on Linux), and validation-first transformation pipeline for Vietnamese phonology.

## Product Goals

1. **Performance**: Sub-millisecond keystroke latency (<1ms) - achieved at ~0.2-0.5ms
2. **Reliability**: Validation-first architecture (phonology rules checked BEFORE transformation)
3. **Cross-Platform**: macOS + Windows + Linux (production-ready) with consistent core engine
4. **User Experience**: Seamless platform integration (CGEventTap on macOS, SetWindowsHookEx on Windows, Fcitx5 on Linux)
5. **Memory Efficiency**: ~5MB memory footprint with optimized binary packaging

## Target Users

- **Primary**: Vietnamese professionals and students who type Vietnamese daily
- **Secondary**: Vietnamese diaspora, bilingual professionals
- **Requirement**: macOS 10.15+, Windows 10+, or Linux with Fcitx5

## Core Functional Requirements

### Input Methods
- **Telex**: Vietnamese keyboard layout (VIQR-style: a's → á)
- **VNI**: Alternative numeric layout support
- **Shortcuts**: User-defined abbreviations with priority matching

### Keystroke Processing
1. Buffer management: Maintain context for multi-keystroke transforms (64 characters)
2. Validation: Check syllable against Vietnamese phonology rules (6 rules)
3. Transformation: Apply diacritics (sắc, huyền, hỏi, ngã, nặng) and tone modifiers (circumflex, horn)
4. Output: Send backspace + replacement characters or pass through

### Platform Integration
- macOS: CGEventTap keyboard hook intercepts keyDown events system-wide
- Windows: WH_KEYBOARD_LL hook intercepts events system-wide
- Linux: Fcitx5 addon integration for seamless input
- Smart text replacement: Backspace method (Terminal) + Selection method (Chrome/Excel)
- Ctrl+Space global hotkey for Vietnamese/English toggle
- Application detection: Specialized handling for autocomplete apps

## Non-Functional Requirements

### Performance
- Keystroke latency: <1ms measured end-to-end (typically 0.2-0.5ms)
- CPU usage: <1% during normal typing
- Memory: ~5MB resident set size
- No input delay under sustained high-speed typing

### Reliability
- 2100+ lines of tests covering integration and edge cases
- Validation-first pattern: Reject invalid Vietnamese before transforming
- Graceful fallback: Pass through on disable or invalid input
- Thread-safe global engine instance via Mutex

### Compatibility
- macOS 10.15 Catalina and later (Apple Silicon & Intel)
- Windows 10/11 (.NET 8)
- Linux (Fcitx5 supported distributions)
- Works with all major applications: Terminal, VS Code, Chrome, Safari, Office, JetBrains IDEs

### Security
- No internet access required (offline-first)
- BSD-3-Clause license (free and open source)
- Accessibility/Hook permissions: Required for keyboard processing
- No telemetry or analytics

## Architecture Overview

```
User Keystroke (CGEventTap/WH_KEYBOARD_LL/Fcitx5)
        ↓
   Platform Bridge (RustBridge.swift / RustBridge.cs / RustBridge.cpp)
        ↓
   Rust Engine (ime_key) - Validation-First 7-Stage Pipeline
    ├─ Stage 1: Stroke Detection (đ/Đ)
    ├─ Stage 2: Tone Mark Detection (sắc/huyền/hỏi/ngã/nặng)
    ├─ Stage 3: Vowel Mark Detection (circumflex/horn/breve)
    ├─ Stage 4: Mark Removal (revert previous marks)
    ├─ Stage 5: W-Vowel Handling (Telex-specific, "w"→"ư")
    ├─ Stage 6: Normal Letter Processing (pass-through)
    └─ Stage 7: Shortcut Expansion (user-defined abbreviations)
        ↓
   Validation (6 Vietnamese Phonology Rules) - Applied BEFORE Transform
    ├─ Rule 1: Must have vowel
    ├─ Rule 2: Valid initial consonants only
    ├─ Rule 3: All characters parsed
    ├─ Rule 4: Spelling rules (c/k/g/ng restrictions)
    ├─ Rule 5: Valid final consonants only
    └─ Rule 6: Valid vowel patterns (Inclusion check)
        ↓
   Transform & Output (apply diacritics, tone marks)
        ↓
   Result (action, backspace count, output chars)
        ↓
   Platform UI (macOS, Windows, Linux - Send text or pass through)
```

## Success Metrics

| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| Keystroke latency | <1ms | ~0.2-0.5ms | ✓ Exceeds |
| Memory usage | <10MB | ~5MB | ✓ Exceeds |
| Test count | >2000 lines | 2100+ lines | ✓ Met |
| macOS compatibility | 10.15+ | 10.15+ universal binary | ✓ Met |
| Windows compatibility | 10+ | 10/11 .NET 8 | ✓ Met |
| Linux compatibility | Fcitx5 | Fcitx5 support | ✓ Met |
| Code quality | Zero warnings | `cargo clippy -D warnings` | ✓ Met |
| Cross-platform | macOS+Win+Linux | All production-ready | ✓ Met |

## Roadmap

### Phase 1: macOS (Complete)
- Telex + VNI input methods
- Menu bar app with settings
- Auto-launch on login
- Update checker via GitHub releases
- Validation-first architecture
- Shortcut system with priority matching

### Phase 2: Cross-Platform (Complete)

**Windows 10/11 (Production Ready)**
- WH_KEYBOARD_LL keyboard hook
- WPF/.NET 8 UI with system tray
- Feature parity with macOS version

**Linux (Production Ready)**
- Fcitx5 addon integration
- C++ bridge to Rust core
- X11/Wayland support

### Phase 3: Enhanced Features (Ongoing)
- Cloud sync for user preferences
- Machine learning for shortcut suggestions
- Dictionary lookup integration
- Advanced diacritics editor
- Mobile support (iOS/Android)

## Development Standards

### Code Organization
- **Core** (`core/src/`): Rust engine, pure logic, zero platform dependencies
- **Platform** (`platforms/`): Platform-specific UI and hooks
- **Scripts** (`scripts/`): Build automation
- **Tests** (`core/tests/`): Integration and unit tests (2100+ lines)

### Quality Gates
- Format: `cargo fmt`
- Lint: `cargo clippy -- -D warnings`
- Tests: `cargo test` (All tests must pass)
- Build: Platform-specific package creation

### Commit Message Format
Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): subject

body

footer
```

Examples:
- `feat(engine): add shortcut expansion for common abbreviations`
- `fix(transform): correct diacritic placement for ư vowel`
- `docs(ffi): update RustBridge interface documentation`
- `test(validation): add edge cases for invalid syllables`

## Dependencies

### Rust
- Zero production dependencies (pure stdlib)
- Dev: `rstest`, `serial_test`

### Platform Specifics
- macOS: SwiftUI, AppKit, Foundation
- Windows: WPF, .NET 8, P/Invoke
- Linux: C++, Fcitx5, CMake

### Build Tools
- `cargo` (Rust toolchain)
- `make` (build automation)
- Platform-specific compilers (clang, msvc, etc.)

## Maintenance & Support

### Release Schedule
- Patch releases: Bug fixes and small improvements
- Minor releases: New features
- Major releases: Breaking changes

### Community
- GitHub Issues: Bug reports and feature requests
- GitHub Discussions: Questions and community support
- License: BSD-3-Clause

---

**Last Updated**: 2025-12-24
**Status**: Production
**Version**: v1.0.88
**Repository**: https://github.com/khaphanspace/gonhanh.org
