using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTelemetry.Model
{
    public class MouseEvent
    {
        public MouseEvent()
        {

        }
        public MouseEvent(string button, string action, int x, int y, int delta)
        {
            Button = button;
            Action = action;
            X = x;
            Y = y;
            Delta = delta;
            Timestamp = DateTime.Now;
        }
        public string Button { get; set; }
        public string Action { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Delta { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
