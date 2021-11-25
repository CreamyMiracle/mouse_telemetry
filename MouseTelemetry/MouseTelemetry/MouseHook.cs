using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MouseTelemetry.Model;

namespace dummy_project2
{
    public class MouseHook
    {
        private Point point;
        private Point Point
        {
            get { return point; }
            set
            {
                if (point != value)
                {
                    point = value;
                    if (MouseClickEvent != null)
                    {
                        var e = new MouseEvent("none", "move", point.X, point.Y, 0);
                        MouseClickEvent(this, e);
                    }
                }
            }
        }
        private int hHook;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_MBUTTONROLL = 0x20A;
        public const int WH_MOUSE_LL = 14;
        public Win32API.HookProc hProc;
        public MouseHook()
        {
            this.Point = new Point();
        }
        public int SetHook()
        {
            hProc = new Win32API.HookProc(MouseHookProc);
            hHook = Win32API.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public void UnHook()
        {
            Win32API.UnhookWindowsHookEx(hHook);
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Win32API.MouseHookStruct MyMouseHookStruct = (Win32API.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32API.MouseHookStruct));
            if (nCode < 0)
            {
                return Win32API.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                if (MouseClickEvent != null)
                {
                    MouseButtons button = MouseButtons.None;
                    int clickCount = 0;
                    MouseEvent e = null;
                    switch ((Int32)wParam)
                    {
                        case WM_LBUTTONDOWN:
                            button = MouseButtons.Left;
                            clickCount = 1;
                            e = new MouseEvent("left", "down", point.X, point.Y, 0);
                            break;
                        case WM_RBUTTONDOWN:
                            button = MouseButtons.Right;
                            clickCount = 1;
                            e = new MouseEvent("right", "down", point.X, point.Y, 0);
                            break;
                        case WM_MBUTTONDOWN:
                            button = MouseButtons.Middle;
                            clickCount = 1;
                            e = new MouseEvent("middle", "down", point.X, point.Y, 0);
                            break;
                        case WM_LBUTTONUP:
                            button = MouseButtons.Left;
                            clickCount = 1;
                            e = new MouseEvent("left", "up", point.X, point.Y, 0);
                            break;
                        case WM_RBUTTONUP:
                            button = MouseButtons.Right;
                            clickCount = 1;
                            e = new MouseEvent("right", "up", point.X, point.Y, 0);
                            break;
                        case WM_MBUTTONUP:
                            button = MouseButtons.Middle;
                            clickCount = 1;
                            e = new MouseEvent("middle", "up", point.X, point.Y, 0);
                            break;
                        case WM_MBUTTONROLL:
                            button = MouseButtons.Middle;
                            clickCount = 1;
                            e = new MouseEvent("middle", "scroll", point.X, point.Y, 120);
                            break;
                    }
                    MouseClickEvent(this, e);
                }
                this.Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                return Win32API.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }

        public delegate void MouseClickHandler(object sender, MouseEvent e);
        public event MouseClickHandler MouseClickEvent;
    }
}
