using MouseTelemetry.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class Constants
    {
        public enum MouseButton
        {
            None,
            Left,
            Right,
            Middle,
            Other,
        }

        public enum MouseAction
        {
            //Primary actions
            Move,
            Up,
            Down,
            Scroll,
            Other,
            //Secondary actions
            Click,
            DoubleClick,
            Drag,
        }

        public static string DefaultDatabasePath
        {
            get
            {
                DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
                return workDir.Parent.Parent.Parent.FullName;
            }
        }

        public static string DefaultDatabaseName
        {
            get
            {
                return "mouse_events_" + TimeExtensions.GetCurrentTimeStamp();
            }
        }
    }
}
