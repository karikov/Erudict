using System.Runtime.InteropServices;
using System.Text;

namespace ERuDict;

/// <summary>
/// Работает только с CF_UNICODETEXT. Не трогает RTF/HTML/OLE Outlook —
/// их чтение через WinForms Clipboard часто роняет процесс.
/// </summary>
internal static class NativeClipboard
{
    private const uint CfUnicodeText = 13;
    private const uint GmemMoveable = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll")]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    public static uint SequenceNumber => GetClipboardSequenceNumber();

    public static async Task<bool> TryOpenAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 20; i++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                return true;
            }

            await Task.Delay(25, cancellationToken).ConfigureAwait(true);
        }

        return false;
    }

    public static void Close()
    {
        CloseClipboard();
    }

    public static string? ReadUnicode()
    {
        if (!IsClipboardFormatAvailable(CfUnicodeText))
        {
            return null;
        }

        var handle = GetClipboardData(CfUnicodeText);
        if (handle == IntPtr.Zero)
        {
            return null;
        }

        var pointer = GlobalLock(handle);
        if (pointer == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return Marshal.PtrToStringUni(pointer);
        }
        finally
        {
            GlobalUnlock(handle);
        }
    }

    public static bool TrySetUnicode(string text)
    {
        if (!EmptyClipboard())
        {
            return false;
        }

        var bytes = Encoding.Unicode.GetBytes(text + '\0');
        var handle = GlobalAlloc(GmemMoveable, (UIntPtr)bytes.Length);
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        var pointer = GlobalLock(handle);
        if (pointer == IntPtr.Zero)
        {
            GlobalFree(handle);
            return false;
        }

        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
        }
        finally
        {
            GlobalUnlock(handle);
        }

        if (SetClipboardData(CfUnicodeText, handle) == IntPtr.Zero)
        {
            GlobalFree(handle);
            return false;
        }

        return true;
    }
}
