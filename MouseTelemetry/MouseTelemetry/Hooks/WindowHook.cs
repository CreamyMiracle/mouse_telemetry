using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTelemetry.Hooks
{
    public class WindowHook
    {
        public struct ActiveWindowInfoEventArgs
        {
            public Rectangle Rect;
            public string Title;
        }


        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private IntPtr hHook;
        Win32API.WindowHookProc hProc;
        public IntPtr SetHook()
        {
            hProc = new Win32API.WindowHookProc(WinEventProc);
            hHook = Win32API.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, hProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            return hHook;
        }
        public void UnHook()
        {
            Win32API.UnhookWindowsHookEx(hHook.ToInt32());
        }
        public string GetActiveWindowTitle()
        {
            return Win32API.GetActiveWindowTitle();
        }
        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            string title = Win32API.GetActiveWindowTitle();
            Rectangle rect = Win32API.GetActiveWindowRect();
            ActiveWindowInfoEventArgs args = new ActiveWindowInfoEventArgs() { Rect = rect, Title = title };
            WindowChanged(this, args);
        }

        public delegate void WindowChangedHandler(object sender, ActiveWindowInfoEventArgs e);
        public event WindowChangedHandler WindowChanged;
    }
}
