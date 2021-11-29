﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MouseTelemetry.Model;
using static Common.Helpers.Constants;

namespace MouseTelemetry
{
    public class MouseHook
    {
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
                if (MouseEvent != null)
                {
                    int x = MyMouseHookStruct.pt.x;
                    int y = MyMouseHookStruct.pt.y;
                    MouseEvent e = null;
                    switch ((Int32)wParam)
                    {
                        case WM_MOUSEMOVE:
                            e = new MouseEvent(MouseButton.None, MouseAction.Move, x, y, 0);
                            break;
                        case WM_LBUTTONDOWN:
                            e = new MouseEvent(MouseButton.Left, MouseAction.Down, x, y, 0);
                            break;
                        case WM_RBUTTONDOWN:
                            e = new MouseEvent(MouseButton.Right, MouseAction.Down, x, y, 0);
                            break;
                        case WM_MBUTTONDOWN:
                            e = new MouseEvent(MouseButton.Middle, MouseAction.Down, x, y, 0);
                            break;
                        case WM_LBUTTONUP:
                            e = new MouseEvent(MouseButton.Left, MouseAction.Up, x, y, 0);
                            break;
                        case WM_RBUTTONUP:
                            e = new MouseEvent(MouseButton.Right, MouseAction.Up, x, y, 0);
                            break;
                        case WM_MBUTTONUP:
                            e = new MouseEvent(MouseButton.Middle, MouseAction.Up, x, y, 0);
                            break;
                        case WM_MBUTTONROLL:
                            e = new MouseEvent(MouseButton.Middle, MouseAction.Scroll, x, y, 120);
                            break;
                        default:
                            e = new MouseEvent(MouseButton.Other, MouseAction.Other, x, y, 0);
                            break;
                    }
                    MouseEvent(this, e);
                }
                return Win32API.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }

        public delegate void MouseClickHandler(object sender, MouseEvent e);
        public event MouseClickHandler MouseEvent;
    }
}
