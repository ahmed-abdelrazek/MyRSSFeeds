using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;

namespace MyRSSFeeds.WinUI.Helpers
{
    // AppWindow.SetIcon covers the taskbar, but the standard title bar's small
    // icon is unreliable in WinUI 3 (WindowsAppSDK #1914) - set it with Win32
    // WM_SETICON, loading the .ico at the DPI-correct small/big icon sizes.
    public static class WindowIconHelper
    {
        private const uint WM_SETICON = 0x0080;
        private const nuint ICON_SMALL = 0;
        private const nuint ICON_BIG = 1;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x0010;
        private const int SM_CXICON = 11;
        private const int SM_CXSMICON = 49;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hInstance, string name, uint type, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, nuint wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        public static void SetIcon(Window window, string icoPath)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            var small = GetSystemMetrics(SM_CXSMICON);
            var big = GetSystemMetrics(SM_CXICON);

            var hSmall = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, small, small, LR_LOADFROMFILE);
            if (hSmall != IntPtr.Zero)
            {
                SendMessage(hwnd, WM_SETICON, ICON_SMALL, hSmall);
            }

            var hBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, big, big, LR_LOADFROMFILE);
            if (hBig != IntPtr.Zero)
            {
                SendMessage(hwnd, WM_SETICON, ICON_BIG, hBig);
            }
        }
    }
}
