using MouseTelemetry.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Common.Helpers.Constants;

namespace MouseTelemetry.Helpers
{
    public static class ClickAnalyzer
    {
        public static List<SecondaryMouseEvent> FindConsecutiveActions(IEnumerable<IMouseEvent> events, MouseButton firstButton, MouseButton secondButton, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, TimeSpan actionSpan)
        {
            List<SecondaryMouseEvent> suitableEvents = new List<SecondaryMouseEvent>();

            TimeSpan clickDuration = TimeSpan.MaxValue;
            events = events.OrderBy(p => p.Timestamp).ToList();
            MouseEvent firstActionEvent = null;
            MouseEvent secondActionEvent = null;
            foreach (MouseEvent point in events)
            {
                if (point.Action == firstAction && point.Button == firstButton)
                {
                    if (firstActionEvent != null)
                    {
                        if (secondActionEvent != null)
                        {
                            firstActionEvent = point;
                        }
                    }
                    else
                    {
                        firstActionEvent = point;
                    }
                }

                if (point.Action == secondAction && point.Button == secondButton)
                {
                    if (firstActionEvent != null && firstActionEvent.Timestamp.Ticks != point.Timestamp.Ticks)
                    {
                        secondActionEvent = point;
                    }
                }

                if (firstActionEvent != null && secondActionEvent != null)
                {
                    if (secondActionEvent.Timestamp - firstActionEvent.Timestamp <= actionSpan)
                    {
                        suitableEvents.Add(new SecondaryMouseEvent(firstActionEvent, secondActionEvent) { Action = actionName, X = secondActionEvent.X, Y = secondActionEvent.Y, Timestamp = secondActionEvent.Timestamp, Delta = secondActionEvent.Delta });
                    }
                    firstActionEvent = null;
                    secondActionEvent = null;
                }
            }
            return suitableEvents;
        }

        public static List<SecondaryMouseEvent> FindConsecutiveActions(IEnumerable<IMouseEvent> events, MouseButton button, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, TimeSpan actionSpan)
        {
            return FindConsecutiveActions(events, button, button, firstAction, secondAction, actionName, actionSpan);
        }

        public static List<SecondaryMouseEvent> FindActionsWithMovementInBetween(IEnumerable<IMouseEvent> events, MouseButton firstButton, MouseButton secondButton, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, double distanceThreshold)
        {
            List<SecondaryMouseEvent> suitableEvents = new List<SecondaryMouseEvent>();

            events = events.OrderBy(p => p.Timestamp).ToList();
            MouseEvent firstActionEvent = null;
            MouseEvent secondActionEvent = null;
            foreach (MouseEvent point in events)
            {
                if (point.Action == firstAction && point.Button == firstButton)
                {
                    // if first event is already found
                    if (firstActionEvent != null)
                    {
                        // if second event is already found
                        if (secondActionEvent != null)
                        {
                            firstActionEvent = point;
                        }
                    }
                    else
                    {
                        firstActionEvent = point;
                    }
                }
                if (point.Action == secondAction && point.Button == secondButton)
                {
                    // if first event is already found and the second event is not the same event
                    if (firstActionEvent != null && firstActionEvent != point)
                    {
                        secondActionEvent = point;
                    }
                }





                if (firstActionEvent != null && secondActionEvent != null)
                {
                    if (firstActionEvent.Distance(secondActionEvent) >= distanceThreshold)
                    {
                        suitableEvents.Add(new SecondaryMouseEvent(firstActionEvent, secondActionEvent) { Action = actionName, Location = secondActionEvent.Location, Timestamp = secondActionEvent.Timestamp, Delta = secondActionEvent.Delta });
                    }
                    firstActionEvent = null;
                    secondActionEvent = null;
                }
            }
            return suitableEvents;
        }

        public static List<SecondaryMouseEvent> FindActionsWithMovementInBetween(IEnumerable<IMouseEvent> events, MouseButton button, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, double distanceThreshold)
        {
            return FindActionsWithMovementInBetween(events, button, button, firstAction, secondAction, actionName, distanceThreshold);
        }
    }
}
