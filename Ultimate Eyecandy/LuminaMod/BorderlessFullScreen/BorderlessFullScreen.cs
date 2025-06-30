using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class BorderlessFullscreen : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;

    static readonly IntPtr HWND_TOP = IntPtr.Zero;
    const uint SWP_FRAMECHANGED = 0x0020;
    const uint SWP_SHOWWINDOW = 0x0040;

    // Optional: call it once on startup (not needed if calling statically)
    void Start()
    {
        SetBorderless();
    }

    // ✅ Static method to call from anywhere
    public static void SetBorderless()
    {
        IntPtr hWnd = GetActiveWindow();

        int width = Display.main.systemWidth;
        int height = Display.main.systemHeight;

        Screen.SetResolution(width, height, false); // Switch to windowed mode

        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowPos(hWnd, HWND_TOP, 0, 0, width, height, SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }
}
