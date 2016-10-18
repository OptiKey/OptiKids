using System;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKids.Native.Common.Enums;

namespace JuliusSweetland.OptiKids.Native.Common.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public AppBarEdge uEdge;
        public RECT rc;
        public int lParam;
    }
}
