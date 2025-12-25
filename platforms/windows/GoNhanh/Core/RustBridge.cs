using System.Runtime.InteropServices;
using System.Text;

namespace GoNhanh.Core;

/// <summary>
/// P/Invoke bridge to Rust core library (gonhanh_core.dll)
/// FFI contract matches core/src/lib.rs exports
/// </summary>
public static class RustBridge
{
    private const string DllName = "gonhanh_core.dll";

    #region Native Imports

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ime_init();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ime_clear();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ime_free(IntPtr result);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ime_method(byte method);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ime_enabled([MarshalAs(UnmanagedType.U1)] bool enabled);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ime_modern([MarshalAs(UnmanagedType.U1)] bool modern);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ime_key(ushort keycode, [MarshalAs(UnmanagedType.U1)] bool caps, [MarshalAs(UnmanagedType.U1)] bool ctrl);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ime_key_ext(ushort keycode, [MarshalAs(UnmanagedType.U1)] bool caps, [MarshalAs(UnmanagedType.U1)] bool ctrl, [MarshalAs(UnmanagedType.U1)] bool shift);

    #endregion

    #region Public API

    /// <summary>
    /// Initialize the IME engine. Call once at startup.
    /// </summary>
    public static void Initialize()
    {
        ime_init();
    }

    /// <summary>
    /// Clear the typing buffer.
    /// </summary>
    public static void Clear()
    {
        ime_clear();
    }

    /// <summary>
    /// Set input method (Telex=0, VNI=1)
    /// </summary>
    public static void SetMethod(InputMethod method)
    {
        ime_method((byte)method);
    }

    /// <summary>
    /// Enable or disable IME processing
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        ime_enabled(enabled);
    }

    /// <summary>
    /// Set tone style (modern=true: hòa, old=false: hoà)
    /// </summary>
    public static void SetModernTone(bool modern)
    {
        ime_modern(modern);
    }

    /// <summary>
    /// Process a keystroke and get the result
    /// </summary>
    /// <param name="keycode">Windows virtual keycode (VK_*)</param>
    /// <param name="shift">True if Shift is held</param>
    /// <param name="capslock">True if CapsLock is active</param>
    public static unsafe ImeResult ProcessKey(ushort keycode, bool shift, bool capslock)
    {
        // Convert Windows VK code to macOS keycode for Rust engine
        ushort macKey = VkToMacKey(keycode);

        // FFI: ime_key_ext(key, caps, ctrl, shift) - use extended version with shift
        IntPtr ptr = ime_key_ext(macKey, capslock, false, shift);
        if (ptr == IntPtr.Zero)
        {
            return ImeResult.Empty;
        }

        try
        {
            // Cast pointer directly to struct pointer for correct memory access
            NativeResult* native = (NativeResult*)ptr;
            return ImeResult.FromNative(native);
        }
        finally
        {
            ime_free(ptr);
        }
    }

    #endregion

    #region Key Mapping

    /// <summary>
    /// Convert Windows Virtual Key code to macOS keycode for Rust engine.
    /// Windows VK codes: A-Z = 0x41-0x5A, 0-9 = 0x30-0x39
    /// macOS keycodes use different mapping per key (non-sequential)
    /// </summary>
    private static ushort VkToMacKey(ushort vk)
    {
        return vk switch
        {
            // Letters (VK_A=0x41 to VK_Z=0x5A)
            0x41 => 0,   // A
            0x42 => 11,  // B
            0x43 => 8,   // C
            0x44 => 2,   // D
            0x45 => 14,  // E
            0x46 => 3,   // F
            0x47 => 5,   // G
            0x48 => 4,   // H
            0x49 => 34,  // I
            0x4A => 38,  // J
            0x4B => 40,  // K
            0x4C => 37,  // L
            0x4D => 46,  // M
            0x4E => 45,  // N
            0x4F => 31,  // O
            0x50 => 35,  // P
            0x51 => 12,  // Q
            0x52 => 15,  // R
            0x53 => 1,   // S
            0x54 => 17,  // T
            0x55 => 32,  // U
            0x56 => 9,   // V
            0x57 => 13,  // W
            0x58 => 7,   // X
            0x59 => 16,  // Y
            0x5A => 6,   // Z

            // Numbers (VK_0=0x30 to VK_9=0x39)
            0x30 => 29,  // 0
            0x31 => 18,  // 1
            0x32 => 19,  // 2
            0x33 => 20,  // 3
            0x34 => 21,  // 4
            0x35 => 23,  // 5
            0x36 => 22,  // 6
            0x37 => 26,  // 7
            0x38 => 28,  // 8
            0x39 => 25,  // 9

            // Special keys
            0x20 => 49,  // VK_SPACE
            0x08 => 51,  // VK_BACK (Backspace) -> DELETE
            0x09 => 48,  // VK_TAB
            0x0D => 36,  // VK_RETURN
            0x1B => 53,  // VK_ESCAPE

            // Arrow keys
            0x25 => 123, // VK_LEFT
            0x26 => 126, // VK_UP
            0x27 => 124, // VK_RIGHT
            0x28 => 125, // VK_DOWN

            // Punctuation (US keyboard layout)
            0xBE => 47,  // VK_OEM_PERIOD (.)
            0xBC => 43,  // VK_OEM_COMMA (,)
            0xBF => 44,  // VK_OEM_2 (/)
            0xBA => 41,  // VK_OEM_1 (;)
            0xDE => 39,  // VK_OEM_7 (')
            0xDB => 33,  // VK_OEM_4 ([)
            0xDD => 30,  // VK_OEM_6 (])
            0xDC => 42,  // VK_OEM_5 (\)
            0xBD => 27,  // VK_OEM_MINUS (-)
            0xBB => 24,  // VK_OEM_PLUS (=)
            0xC0 => 50,  // VK_OEM_3 (`)

            // Unknown key - return as-is (will fail gracefully in Rust)
            _ => vk
        };
    }

    #endregion
}

/// <summary>
/// Input method type
/// </summary>
public enum InputMethod : byte
{
    Telex = 0,
    VNI = 1
}

/// <summary>
/// IME action type
/// </summary>
public enum ImeAction : byte
{
    None = 0,    // No action needed
    Send = 1,    // Send text replacement
    Restore = 2  // Restore original text
}

/// <summary>
/// Native result structure from Rust (must match core/src/engine/mod.rs)
/// Uses unsafe fixed array for correct FFI memory layout
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeResult
{
    public fixed uint chars[64];  // inline fixed array, matches Rust [u32; 64]
    public byte action;
    public byte backspace;
    public byte count;
    public byte flags;
}

/// <summary>
/// Managed IME result
/// </summary>
public readonly struct ImeResult
{
    public readonly ImeAction Action;
    public readonly byte Backspace;
    public readonly byte Count;
    private readonly uint[] _chars;

    public static readonly ImeResult Empty = new(ImeAction.None, 0, 0, Array.Empty<uint>());

    private ImeResult(ImeAction action, byte backspace, byte count, uint[] chars)
    {
        Action = action;
        Backspace = backspace;
        Count = count;
        _chars = chars;
    }

    internal static unsafe ImeResult FromNative(NativeResult* native)
    {
        if (native == null) return Empty;

        // Copy fixed array to managed array
        int count = native->count;
        uint[] chars = new uint[count];
        for (int i = 0; i < count && i < 64; i++)
        {
            chars[i] = native->chars[i];
        }

        return new ImeResult(
            (ImeAction)native->action,
            native->backspace,
            native->count,
            chars
        );
    }

    /// <summary>
    /// Get the result text as a string
    /// </summary>
    public string GetText()
    {
        if (Count == 0 || _chars == null)
            return string.Empty;

        var sb = new StringBuilder(Count);
        for (int i = 0; i < Count && i < _chars.Length; i++)
        {
            if (_chars[i] > 0)
            {
                sb.Append(char.ConvertFromUtf32((int)_chars[i]));
            }
        }
        return sb.ToString();
    }
}
