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
        #region Consecutive
        public static List<SecondaryMouseEvent> FindConsecutiveActions(IEnumerable<IMouseEvent> events, MouseButton firstButton, MouseButton secondButton, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, TimeSpan actionSpan, Size maxActionAreaSize)
        {
            List<SecondaryMouseEvent> suitableEvents = new List<SecondaryMouseEvent>();

            List<MouseEvent> mEvents = events.Where(e => (e.Action == firstAction || e.Action == secondAction) && (e.Button == firstButton || e.Button == secondButton)).Cast<MouseEvent>().OrderBy(p => p.Timestamp).ToList();
            MouseEvent firstEvent = null;
            MouseEvent secondEvent = null;
            MouseEvent nextEvent = null;
            Rectangle firstActionArea = Rectangle.Empty;

            int currInd = 0;
            foreach (MouseEvent currEvent in mEvents)
            {
                // Get next event
                nextEvent = currInd + 1 < mEvents.Count ? mEvents.ElementAt(currInd + 1) : null;
                if (nextEvent == null) { break; }


                // Selecting firstEvent
                if (currEvent.Action == firstAction && currEvent.Button == firstButton)
                {
                    firstEvent = currEvent;
                    int rX = firstEvent.X - (maxActionAreaSize.Width / 2);
                    int rY = firstEvent.Y - (maxActionAreaSize.Height / 2);
                    firstActionArea = new Rectangle(new Point(rX, rY), maxActionAreaSize);
                }

                // Selecting secondEvent
                if (nextEvent.Action == secondAction && nextEvent.Button == secondButton)
                {
                    secondEvent = nextEvent;
                }

                if (firstEvent != null && secondEvent != null)
                {
                    if (secondEvent.Timestamp - firstEvent.Timestamp <= actionSpan)
                    {
                        if (firstActionArea.Contains(secondEvent.X, secondEvent.Y))
                        {
                            suitableEvents.Add(new SecondaryMouseEvent(firstEvent, secondEvent) { Action = actionName, Timestamp = secondEvent.Timestamp, Delta = secondEvent.Delta });
                        }
                    }
                }

                firstActionArea = Rectangle.Empty;
                nextEvent = null;
                firstEvent = null;
                secondEvent = null;
                currInd++;
            }
            return suitableEvents;
        }

        public static List<SecondaryMouseEvent> FindConsecutiveActions(IEnumerable<IMouseEvent> events, MouseButton button, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, TimeSpan actionSpan, Size maxActionAreaSize)
        {
            return FindConsecutiveActions(events, button, button, firstAction, secondAction, actionName, actionSpan, maxActionAreaSize);
        }
        #endregion

        #region Drag
        public static List<SecondaryMouseEvent> FindActionsWithMovementInBetween(IEnumerable<IMouseEvent> events, MouseButton firstButton, MouseButton secondButton, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, Size minActionAreaSize)
        {
            List<SecondaryMouseEvent> suitableEvents = new List<SecondaryMouseEvent>();

            List<MouseEvent> mEvents = events.Where(e => (e.Action == firstAction || e.Action == secondAction) && (e.Button == firstButton || e.Button == secondButton)).Cast<MouseEvent>().OrderBy(p => p.Timestamp).ToList();
            MouseEvent firstEvent = null;
            MouseEvent secondEvent = null;
            MouseEvent nextEvent = null;
            Rectangle firstActionArea = Rectangle.Empty;

            int currInd = 0;
            foreach (MouseEvent currEvent in mEvents)
            {
                // Get next event
                nextEvent = currInd + 1 < mEvents.Count ? mEvents.ElementAt(currInd + 1) : null;
                if (nextEvent == null) { break; }


                // Selecting firstEvent
                if (currEvent.Action == firstAction && currEvent.Button == firstButton)
                {
                    firstEvent = currEvent;
                    int rX = firstEvent.X - (minActionAreaSize.Width / 2);
                    int rY = firstEvent.Y - (minActionAreaSize.Height / 2);
                    firstActionArea = new Rectangle(new Point(rX, rY), minActionAreaSize);
                }

                // Selecting secondEvent
                if (nextEvent.Action == secondAction && nextEvent.Button == secondButton)
                {
                    secondEvent = nextEvent;
                }

                if (firstEvent != null && secondEvent != null)
                {
                    if (!firstActionArea.Contains(secondEvent.X, secondEvent.Y))
                    {
                        suitableEvents.Add(new SecondaryMouseEvent(firstEvent, secondEvent) { Action = actionName, Timestamp = secondEvent.Timestamp, Delta = secondEvent.Delta });
                    }
                }

                firstActionArea = Rectangle.Empty;
                nextEvent = null;
                firstEvent = null;
                secondEvent = null;
                currInd++;
            }
            return suitableEvents;
        }

        public static List<SecondaryMouseEvent> FindActionsWithMovementInBetween(IEnumerable<IMouseEvent> events, MouseButton button, MouseAction firstAction, MouseAction secondAction, MouseAction actionName, Size minActionAreaSize)
        {
            return FindActionsWithMovementInBetween(events, button, button, firstAction, secondAction, actionName, minActionAreaSize);
        }
        #endregion

        #region Scalars
        public static double FindTotalMovementDistance(IEnumerable<MouseEvent> events, MouseButton button)
        {
            double totalDistance = 0;
            List<MouseEvent> mEvents = events.Where(e => e.Action == MouseAction.Move && e.Button == button).OrderBy(p => p.Timestamp).ToList();
            MouseEvent prevEvent = mEvents.FirstOrDefault();
            if (prevEvent == null) { return totalDistance; }

            foreach (MouseEvent currEvent in mEvents.Skip(1))
            {
                totalDistance += prevEvent.Distance(currEvent);
            }
            return totalDistance;
        }
        #endregion
    }
}
