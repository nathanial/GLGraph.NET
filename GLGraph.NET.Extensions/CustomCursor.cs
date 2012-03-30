using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GLGraph.NET.Extensions {
    struct IconInfo {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    static class NativeMethods {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);
    }

    static class CustomCursor {
        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot) {
            var ptr = bmp.GetHicon();
            var tmp = new IconInfo();
            NativeMethods.GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = NativeMethods.CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
    }
}
