using System.Runtime.InteropServices;

namespace GoNhanh.Core;

/// <summary>
/// Sends text to the active window using Windows SendInput API
/// Handles backspace deletion and Unicode character insertion
/// </summary>
public static class TextSender
{
    #region Win32 Constants

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;

    #endregion

    #region Win32 Imports

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    #endregion

    #region Structures

    // INPUT struct for SendInput - must match Windows API exactly
    // On 64-bit Windows, sizeof(INPUT) = 40 bytes
    // The union must be the size of the largest member (MOUSEINPUT = 32 bytes on 64-bit)
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION u;
    }

    // Union must be sized to match MOUSEINPUT (the largest variant)
    // MOUSEINPUT: 4+4+4+4+4+8 = 28 bytes, padded to 32 bytes on 64-bit
    // KEYBDINPUT: 2+2+4+4+8 = 20 bytes
    // We use explicit layout with Size to ensure correct sizing
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    #endregion

    /// <summary>
    /// Send text replacement: delete characters and insert new text
    /// </summary>
    /// <param name="text">Text to insert</param>
    /// <param name="backspaces">Number of backspaces to send first</param>
    public static void SendText(string text, int backspaces)
    {
        if (string.IsNullOrEmpty(text) && backspaces == 0)
            return;

        var inputs = new List<INPUT>();
        var marker = KeyboardHook.GetInjectedKeyMarker();

        // Add backspaces
        for (int i = 0; i < backspaces; i++)
        {
            // Key down
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = KeyCodes.VK_BACK,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = marker
                    }
                }
            });

            // Key up
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = KeyCodes.VK_BACK,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = marker
                    }
                }
            });
        }

        // Add text characters (Unicode)
        foreach (char c in text)
        {
            // For Unicode characters, use wScan with KEYEVENTF_UNICODE flag
            // Key down
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = marker
                    }
                }
            });

            // Key up
            inputs.Add(new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = marker
                    }
                }
            });
        }

        if (inputs.Count > 0)
        {
            var inputArray = inputs.ToArray();
            SendInput((uint)inputArray.Length, inputArray, Marshal.SizeOf<INPUT>());
        }
    }
}
