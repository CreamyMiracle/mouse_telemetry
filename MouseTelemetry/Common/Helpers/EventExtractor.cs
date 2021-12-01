using MouseTelemetry.Helpers;
using MouseTelemetry.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Common.Helpers.Constants;

namespace Common.Helpers
{
    public static class EventExtractor
    {
        public static Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> ExtractButtonActions(List<MouseEvent> originalTestData)
        {
            Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> buttonActions = new Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>>();
            foreach (MouseButton btn in Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>())
            {
                Dictionary<MouseAction, IEnumerable<IMouseEvent>> actionEvents = new Dictionary<MouseAction, IEnumerable<IMouseEvent>>();
                foreach (MouseAction act in Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>())
                {
                    actionEvents[act] = new List<IMouseEvent>();
                }
                buttonActions[btn] = actionEvents;
            }
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>())
            {
                foreach (MouseAction action in Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>())
                {
                    List<MouseEvent> primaryEvents = originalTestData.Where(p => button == p.Button && action == p.Action).ToList();
                    buttonActions[button][action] = primaryEvents.Cast<IMouseEvent>();
                }

                List<SecondaryMouseEvent> singleClicks = ClickAnalyzer.FindConsecutiveActions(originalTestData.Cast<IMouseEvent>(), button, MouseAction.Down, MouseAction.Up, MouseAction.Click, TimeSpan.FromMilliseconds(500), new Size(5, 5));
                List<SecondaryMouseEvent> doubleClicks = ClickAnalyzer.FindConsecutiveActions(singleClicks.Cast<IMouseEvent>(), button, MouseAction.Click, MouseAction.Click, MouseAction.DoubleClick, TimeSpan.FromMilliseconds(500), SystemInformation.DoubleClickSize);
                List<SecondaryMouseEvent> drags = ClickAnalyzer.FindActionsWithMovementInBetween(originalTestData.Cast<IMouseEvent>(), button, MouseAction.Down, MouseAction.Up, MouseAction.Drag, SystemInformation.DragSize);
                double distance = ClickAnalyzer.FindTotalMovementDistance(originalTestData.Cast<MouseEvent>(), button);

                buttonActions[button][MouseAction.Click] = singleClicks;
                buttonActions[button][MouseAction.DoubleClick] = doubleClicks;
                buttonActions[button][MouseAction.Drag] = drags;
            }
            return buttonActions;
        }
        public static List<IMouseEvent> SelectCertain(Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> btnActs, IEnumerable<MouseAction> actions, IEnumerable<MouseButton> buttons, IEnumerable<string> windows)
        {
            List<IMouseEvent> selectedData = new List<IMouseEvent>();
            foreach (var acts in btnActs.Where(btn => buttons.Contains(btn.Key)))
            {
                foreach (var evnts in acts.Value.Where(act => actions.Contains(act.Key)))
                {
                    selectedData.AddRange(evnts.Value);
                }
            }
            if (windows.Count() == 0)
            {
                return selectedData;
            }
            return selectedData.Where(e => windows.Any(w => e.Window != null && e.Window.ToLower().Contains(w.ToLower()))).ToList();
        }
        public static void PrintButtonActions(Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> btnActs, IEnumerable<string> windows)
        {
            List<IMouseEvent> totalEvents = SelectCertain(btnActs, Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>(), Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>(), windows);
            Console.WriteLine("Total of " + string.Format("{0} points found", totalEvents.Count));
            foreach (MouseButton button in btnActs.Keys)
            {
                foreach (MouseAction action in btnActs[button].Keys)
                {
                    string buttonString = Enum.GetName(typeof(MouseButton), button);
                    string actionString = Enum.GetName(typeof(MouseAction), action);
                    int count = btnActs[button][action].Count();
                    if (count == 0)
                    {
                        continue;
                    }
                    Console.WriteLine(string.Format("Button: {0} Action: {1} Count: {2}", buttonString, actionString, btnActs[button][action].Count()));
                }
            }
        }
    }
}
