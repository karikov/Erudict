using System.Runtime.InteropServices;

namespace ERuDict;

internal static class Win32
{
    public const int VkControl = 0x11;
    public const int VkLControl = 0xA2;
    public const int VkRControl = 0xA3;
    public const int VkShift = 0x10;
    public const int VkC = 0x43;
    public const int VkEscape = 0x1B;
    public const int VkLButton = 0x01;
    public const int VkRButton = 0x02;

    public const int WhKeyboardLl = 13;
    public const int WmKeyDown = 0x0100;
    public const int WmKeyUp = 0x0101;
    public const int WmSysKeyDown = 0x0104;
    public const int InputKeyboard = 1;
    public const uint KeyeventfKeyup = 0x0002;
    public const uint LlkhfInjected = 0x10;

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KbdLlHookStruct
    {
        public uint VkCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public int Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MouseInput Mi;
        [FieldOffset(0)] public KeybdInput Ki;
        [FieldOffset(0)] public HardwareInput Hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public UIntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeybdInput
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public UIntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint UMsg;
        public ushort WParamL;
        public ushort WParamH;
    }

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vkCode);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    public static bool IsKeyDown(int vkCode) => (GetAsyncKeyState(vkCode) & 0x8000) != 0;

    public static bool IsControl(uint vkCode) =>
        vkCode is VkControl or VkLControl or VkRControl;

    public static void SendCopyShortcut()
    {
        bool ctrlAlreadyDown = IsKeyDown(VkControl) || IsKeyDown(VkLControl) || IsKeyDown(VkRControl);
        var keys = ctrlAlreadyDown
            ? new ushort[] { VkC, VkC }
            : new ushort[] { VkControl, VkC, VkC, VkControl };
        var downs = ctrlAlreadyDown
            ? new[] { true, false }
            : new[] { true, true, false, false };

        var inputs = new Input[keys.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            inputs[i] = new Input
            {
                Type = InputKeyboard,
                U = new InputUnion
                {
                    Ki = new KeybdInput
                    {
                        WVk = keys[i],
                        WScan = (ushort)MapVirtualKey(keys[i], 0),
                        DwFlags = downs[i] ? 0 : KeyeventfKeyup
                    }
                }
            };
        }

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
    }
}
