using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTelemetry.Hooks
{
    public class WindowHook
    {
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
        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            string juu = Win32API.GetActiveWindowTitle();
            WindowChanged(this, juu);
        }

        public delegate void WindowChangedHandler(object sender, string e);
        public event WindowChangedHandler WindowChanged;
    }
}
