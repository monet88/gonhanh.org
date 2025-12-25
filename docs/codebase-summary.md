# Gõ Nhanh: Codebase Summary

Complete directory structure, module responsibilities, and development entry points for the Gõ Nhanh Vietnamese Input Method Engine.

## Directory Structure

```
gonhanh.org/
├── core/                          # Rust engine (100% platform-agnostic)
│   ├── src/
│   │   ├── lib.rs                # FFI exports (ime_init, ime_key, ime_method, etc.)
│   │   ├── utils.rs              # Utility functions (char conversions, etc.)
│   │   │
│   │   ├── engine/               # Core processing pipeline
│   │   │   ├── mod.rs            # Main Engine struct + ime_key orchestration
│   │   │   ├── buffer.rs         # Circular typing buffer (64 chars)
│   │   │   ├── syllable.rs       # Syllable parsing (C+G+V+C pattern)
│   │   │   ├── validation.rs     # Vietnamese phonology rules (6 rules)
│   │   │   ├── transform.rs      # Diacritic + tone application (pattern-based)
│   │   │   └── shortcut.rs       # User-defined abbreviations with priority
│   │   │
│   │   ├── input/                # Input method strategies
│   │   │   ├── mod.rs            # Input trait + method registry
│   │   │   ├── telex.rs          # Telex method (a/e/o/w for tones, s/f/r/x/j for marks)
│   │   │   └── vni.rs            # VNI method (1-5 for marks, 6-8 for tones, 9 for đ)
│   │   │
│   │   └── data/                 # Static Vietnamese linguistic data
│   │       ├── mod.rs            # Data module exports
│   │       ├── keys.rs           # Telex/VNI keycode to transformation mappings
│   │       ├── chars.rs          # Character data (UTF-32 constants, casing)
│   │       ├── vowel.rs          # Vowel table (72 entries: 12 bases × 6 marks)
│   │       └── constants.rs      # Constants (consonants, valid clusters, etc.)
│   │
│   ├── tests/                    # Integration + unit tests (2100+ lines)
│   │   ├── common/mod.rs         # Test utilities (IME helper, test setup)
│   │   ├── unit_test.rs          # Unit tests for individual modules
│   │   ├── typing_test.rs        # Full keystroke sequences (Telex + VNI)
│   │   ├── engine_test.rs        # Engine initialization + state tests
│   │   ├── integration_test.rs   # End-to-end keystroke→output tests
│   │   └── paragraph_test.rs     # Multi-word paragraph typing tests
│   │
│   └── Cargo.toml               # Rust dependencies (zero production deps)
│
├── platforms/                    # Platform-specific implementations
│   │
│   ├── macos/                   # Production: SwiftUI app (~1700 LOC)
│   │   ├── App.swift            # AppDelegate + main application setup
│   │   ├── RustBridge.swift     # FFI bridge to Rust engine (CRITICAL)
│   │   ├── MenuBar.swift        # Status bar UI + menu items
│   │   │
│   │   ├── SettingsView.swift   # Input method selection + preferences
│   │   ├── OnboardingView.swift # Accessibility permission setup wizard
│   │   ├── AboutView.swift      # About window + version info
│   │   ├── UpdateView.swift     # Update notification UI
│   │   │
│   │   ├── LaunchAtLogin.swift  # SMAppService integration (auto-launch)
│   │   ├── UpdateManager.swift  # DMG download + version tracking
│   │   ├── UpdateChecker.swift  # GitHub API integration (version checking)
│   │   ├── AppMetadata.swift    # Shared app constants (version, names)
│   │   │
│   │   ├── libgonhanh_core.a    # Compiled universal Rust library (arm64 + x86_64)
│   │   ├── GoNhanh.xcodeproj/   # Xcode project + build settings
│   │   ├── Assets.xcassets/     # App icons (1024×1024 down to 16×16)
│   │   ├── dmg-resources/       # DMG installer background + resources
│   │   └── Tests/               # Swift unit tests (LaunchAtLoginTests.swift)
│   │
│   ├── windows/                 # Production: WPF/.NET 8 app (~1400 LOC)
│   │   ├── App.xaml.cs          # Application entry point + setup
│   │   ├── Core/
│   │   │   ├── RustBridge.cs    # FFI bridge to Rust engine
│   │   │   ├── KeyboardHook.cs  # WH_KEYBOARD_LL keyboard interception
│   │   │   ├── KeyCodes.cs      # Windows virtual keycodes mapping
│   │   │   └── TextSender.cs    # Text input simulation (SendInput)
│   │   ├── Services/
│   │   │   ├── SettingsService.cs # Registry-based settings persistence
│   │   │   └── UpdateService.cs   # Windows update checker
│   │   ├── Views/
│   │   │   ├── TrayIcon.cs      # System tray icon UI + menu
│   │   │   ├── OnboardingWindow.xaml.cs # Setup wizard
│   │   │   ├── AboutWindow.xaml.cs      # About dialog
│   │   │   └── SettingsWindow.xaml.cs   # Preferences window
│   │   └── libgonhanh_core.dll  # Compiled Rust DLL
│   │
│   └── linux/                   # Production: Fcitx5 addon (~500 LOC)
│       ├── src/
│       │   ├── Engine.h/cpp      # Fcitx5 InputMethodEngine implementation
│       │   ├── RustBridge.h/cpp  # C++ FFI wrapper to Rust core
│       │   └── KeycodeMap.h      # X11/Wayland keysym → keycode mapping
│       ├── data/
│       │   ├── gonhanh-addon.conf # Fcitx5 addon registration
│       │   └── gonhanh.conf      # Input method configuration
│       ├── scripts/
│       │   ├── build.sh          # CMake build script
│       │   └── install.sh        # User-local installation script
│       └── libgonhanh_core.so    # Compiled Rust shared library (x86_64)
│
├── scripts/                     # Build automation
│   ├── setup.sh                # Environment setup (installs Rust, arms cargo-nextest)
│   ├── build-core.sh           # Build universal Rust library (arm64 + x86_64)
│   ├── build-macos.sh          # Build macOS SwiftUI app + DMG
│   ├── build-windows.ps1       # PowerShell build script for Windows
│   └── generate-release-notes.sh # Release notes generator
│
├── Makefile                    # Main build targets
├── .github/workflows/          # CI/CD automation
│   ├── ci.yml                 # Run on push/PR: format, clippy, tests
│   └── release.yml            # Run on tags: build, create GitHub release
│
├── CLAUDE.md                   # Developer guidance (architecture, patterns, commands)
├── README.md                   # Project overview + quick start
└── docs/                       # Documentation (this folder)
```

## Core Module Responsibilities

### Engine Modules (core/src/engine/)

#### `engine/mod.rs` - Main Processing Pipeline
Central `Engine` struct orchestrating 7-stage keystroke processing:
1. **Stroke detection** (đ/Đ) - Single key transformation
2. **Tone mark detection** (sắc/huyền/hỏi/ngã/nặng) - Multi-key sequences
3. **Vowel mark detection** (circumflex/horn/breve) - Multi-key sequences
4. **Mark removal** (reverse vowel transformation) - Undo previous marks
5. **W-vowel handling** (Telex-specific "w"→"ư") - Context-aware substitution
6. **Normal letter processing** - Regular keystroke
7. **Shortcut expansion** (user-defined) - Abbreviation matching

**Result**: Returns `Result` struct with action (None/Send/Restore), backspace count, output chars

#### `engine/buffer.rs` - Circular Typing Buffer
Fixed 64-character circular buffer for multi-keystroke context. Tracks tone mark, vowel mark, and stroke for each character. Implements tone/mark repositioning (e.g., "hoaf" → "hoà").

#### `engine/syllable.rs` - Vietnamese Syllable Parsing
Parses buffer into syllable components: (C₁)(G)V(C₂)
- C₁ = initial consonant
- G = glide (y/w)
- V = vowel
- C₂ = final consonant

#### `engine/validation.rs` - Vietnamese Phonology Rules
**6 Validation Rules** (applied BEFORE transformation, validation-first approach):
1. **Must have vowel**: Every valid syllable contains at least one vowel
2. **Valid initials**: Only 16 single consonants + 11 pairs + ngh allowed at start
3. **All chars parsed**: Every character fits syllable pattern (C+G+V+C)
4. **Spelling rules**: Enforce c/k/g/ng restrictions
5. **Valid finals**: Only c,ch,m,n,ng,nh,p,t allowed at end
6. **Valid vowel patterns**: Inclusion check against VALID_VOWEL_PAIRS

#### `engine/transform.rs` - Diacritic & Tone Application
Pattern-based transformation. Applies tones and vowel marks with special handling for compounds like "ươ".

#### `engine/shortcut.rs` - User-Defined Abbreviations
Priority-based matching system. Supports arbitrary abbreviation → expansion (e.g., "vn" → "Việt Nam").

### Input Method Modules (core/src/input/)

#### `input/telex.rs` - Telex Input Method
Vietnamese VIQR-style: a+s → á, a+f → à, a+r → ả, a+x → ã, a+j → ạ.

#### `input/vni.rs` - VNI Input Method
Vietnamese numeric: a+1 → á, a+2 → à, etc.

### FFI Layer (core/src/lib.rs)
Exports 6 C ABI functions (thread-safe via Mutex). Critical for cross-platform interop.

## Test Coverage

| File | Purpose | Test Count |
|------|---------|-----------|
| `unit_test.rs` | Module unit tests | ~30 |
| `typing_test.rs` | Full keystroke sequences | ~60 |
| `engine_test.rs` | Engine initialization + state | ~20 |
| `integration_test.rs` | End-to-end keystroke→output | ~35 |
| `paragraph_test.rs` | Multi-word paragraph tests | ~15 |

**Total**: 2100+ lines of test code.

## Performance Characteristics

### Critical Path (ime_key execution)
1. Lock ENGINE mutex (1-2μs)
2. Validate keystroke (100-150μs)
3. Process transform (50-100μs)
4. Allocate + populate Result (20-30μs)
5. Unlock mutex (1-2μs)

**Total**: 170-285μs (0.17-0.28ms) - well under 1ms budget.

### Memory Usage
- Static data: ~150KB
- ENGINE global: ~500B
- Per keystroke: Stack-allocated arrays only (no heap)
- UI overhead: ~4.5MB

**Total app**: ~5MB resident.

---

**Last Updated**: 2025-12-24
**Version**: v1.0.88
**Total Lines**: ~16,000 (Rust + Swift + C# + C++)
**Platforms**: macOS, Windows, Linux (all Production Ready)
