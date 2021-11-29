using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Helpers.Constants;

namespace MouseTelemetry.Model
{
    public interface IMouseEvent
    {
        MouseButton Button { get; set; }
        MouseAction Action { get; set; }
        int Delta { get; set; }
        DateTime Timestamp { get; set; }
    }

    public class MouseEvent : IMouseEvent
    {
        public MouseEvent()
        {

        }
        public MouseEvent(MouseButton button, MouseAction action, int x, int y, int delta)
        {
            Button = button;
            Action = action;
            X = x;
            Y = y;
            Delta = delta;
            Timestamp = DateTime.Now;
        }
        public double Distance(MouseEvent destination)
        {
            return Math.Sqrt(Math.Pow((destination.X - X), 2) + Math.Pow((destination.Y - Y), 2));
        }
        public MouseButton Button { get; set; }
        public MouseAction Action { get; set; }
        public int Delta { get; set; }
        public DateTime Timestamp { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
    public class SecondaryMouseEvent : MouseEvent
    {
        public SecondaryMouseEvent(MouseEvent firstEvent, MouseEvent secondEvent) : base(secondEvent.Button, secondEvent.Action, secondEvent.X, secondEvent.Y, secondEvent.Delta)
        {
            FirstEvent = firstEvent;
            SecondEvent = secondEvent;
        }
        public MouseEvent FirstEvent { get; set; }
        public MouseEvent SecondEvent { get; set; }
    }
}
