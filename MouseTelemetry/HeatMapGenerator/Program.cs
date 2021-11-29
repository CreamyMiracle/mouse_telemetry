using Common.Helpers;
using Fclp;
using MouseTelemetry.Helpers;
using MouseTelemetry.Model;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Common.Helpers.Constants;

namespace Heatmap
{
    class Program
    {
        private static SQLiteAsyncConnection db_async;
        private static string _dbPath = @"F:\Dev\pc_telemetry\MouseTelemetry\mouse_events_29112021Utc.db";
        private static string _sqlQuery = string.Format("SELECT * FROM \"{0}\"", nameof(MouseEvent));
        private static IEnumerable<MouseAction> _actions;
        private static IEnumerable<MouseButton> _buttons;
        private static List<MouseAction> _defaultActions = Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>().ToList();
        private static List<MouseButton> _defaultButtons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList();
        private static bool _parsingOk = true;

        static void Main(string[] args)
        {
#if DEBUG
            _actions = new List<MouseAction>() { MouseAction.Click };//Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>().ToList();
            _buttons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList();
            Run(args);
#else
            if (Parse(args))
            {
                Run(args);
            }
#endif

        }
        private static bool Parse(string[] args)
        {
            var p = new FluentCommandLineParser();

            p.Setup<string>('p', "path")
             .Callback(value => _dbPath = value)
             .Required()
             .WithDescription("Path from which a database file is read");

            p.Setup<string>('q', "query")
             .Callback(value => _sqlQuery = value)
             .SetDefault("SELECT * FROM \"MouseEvent\"")
             .WithDescription("SQL query to select mouse events from database file");

            p.Setup<List<MouseButton>>('b', "buttons")
             .Callback(value => _buttons = value)
             .SetDefault(_defaultButtons)
             .WithDescription("Mouse buttons that are taken into consideration. Values: " + "{" + string.Join("|", _defaultButtons) + "}");

            p.Setup<List<MouseAction>>('a', "actions").Callback(value => _actions = value)
             .SetDefault(_defaultActions)
             .WithDescription("Actions of given buttons that are taken into consideration. Values: " + "{" + string.Join("|", _defaultActions) + "}");

            p.SetupHelp("?", "help")
             .Callback(text => { _parsingOk = false; Console.WriteLine(text); });

            var parseResult = p.Parse(args);
            if (parseResult.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
                _parsingOk = false;
            }

            return _parsingOk;
        }

        private static void Run(string[] args)
        {
            db_async = new SQLiteAsyncConnection(_dbPath);

            Task.Run(async () =>
            {
                await GenerateHeatMap(_actions, _buttons);
            }).Wait();
        }

        private static async Task<List<MouseEvent>> ReadMouseEvents()
        {
            List<MouseEvent> events = await db_async.QueryAsync<MouseEvent>(_sqlQuery);//await db_async.GetAllWithChildrenAsync<MouseEvent>();
            return events;
        }

        //private static async List<IMouseEvent> SelectPoints(IEnumerable<MouseAction> actions, IEnumerable<MouseButton> buttons)
        //{
        //    List<MouseEvent> originalTestData = await ReadMouseEvents();
        //}

        private static async Task GenerateHeatMap(IEnumerable<MouseAction> actions, IEnumerable<MouseButton> buttons)
        {
            List<MouseEvent> originalTestData = await ReadMouseEvents();
            Point maxPoint = PointHelpers.NormalizeData(originalTestData);

            while (true)
            {
                List<IMouseEvent> selectedData = new List<IMouseEvent>();

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

                foreach (MouseButton button in buttons)
                {
                    foreach (MouseAction action in actions)
                    {
                        List<MouseEvent> primaryEvents = originalTestData.Where(p => button == p.Button && action == p.Action).ToList();
                        selectedData.AddRange(primaryEvents);
                        buttonActions[button][action] = primaryEvents.Cast<IMouseEvent>();
                    }
                }

                foreach (MouseButton button in buttons)
                {
                    List<SecondaryMouseEvent> singleClicks = ClickAnalyzer.FindConsecutiveActions(originalTestData.Cast<IMouseEvent>(), button, MouseAction.Down, MouseAction.Up, MouseAction.Click, TimeSpan.FromMilliseconds(500), new Size(5, 5));
                    List<SecondaryMouseEvent> doubleClicks = ClickAnalyzer.FindConsecutiveActions(singleClicks.Cast<IMouseEvent>(), button, MouseAction.Click, MouseAction.Click, MouseAction.DoubleClick, TimeSpan.FromMilliseconds(500), SystemInformation.DoubleClickSize);
                    List<SecondaryMouseEvent> drags = ClickAnalyzer.FindActionsWithMovementInBetween(originalTestData.Cast<IMouseEvent>(), button, MouseAction.Down, MouseAction.Up, MouseAction.Drag, SystemInformation.DragSize);
                    double distance = ClickAnalyzer.FindTotalMovementDistance(originalTestData.Cast<MouseEvent>(), button);

                    buttonActions[button][MouseAction.Click] = singleClicks;
                    buttonActions[button][MouseAction.DoubleClick] = doubleClicks;
                    buttonActions[button][MouseAction.Drag] = drags;

                    selectedData.AddRange(singleClicks.Cast<IMouseEvent>());
                    selectedData.AddRange(doubleClicks.Cast<IMouseEvent>());
                    selectedData.AddRange(drags.Cast<IMouseEvent>());
                }

                Console.WriteLine("Total of " + string.Format("{0} points found", selectedData.Count));
                foreach (MouseButton button in buttonActions.Keys)
                {
                    foreach (MouseAction action in buttonActions[button].Keys)
                    {
                        string buttonString = Enum.GetName(typeof(MouseButton), button);
                        string actionString = Enum.GetName(typeof(MouseAction), action);
                        int count = buttonActions[button][action].Count();
                        if (count == 0)
                        {
                            continue;
                        }
                        Console.WriteLine(string.Format("Button: {0} Action: {1} Count: {2}", buttonString, actionString, buttonActions[button][action].Count()));
                    }
                }

                DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
                string basePath = workDir.Parent.Parent.Parent.FullName;
                string dbPath = Path.Combine(basePath, "heatmap_" + TimeExtensions.GetCurrentTimeStampPrecise() + ".png");

                //Set up the factory and run the GetHeatMap function.
                HeatmapFactory map = new HeatmapFactory(originalTestData, maxPoint);
                //map.ColorFunction = HeatmapFactory.GrayScale;
                map.ColorFunction = HeatmapFactory.BasicColorMapping;
                Bitmap heatMap = map.GetHeatMap(selectedData.Cast<MouseEvent>());
                heatMap.DrawLine(selectedData.Cast<MouseEvent>());
                heatMap.SaveBitmap(@"F:\Dev\pc_telemetry\MouseTelemetry\heatmappi.png");

                Console.WriteLine("Exit? y/n");
                if (Console.ReadKey().ToString() == "y")
                {
                    break;
                }
            }
        }
    }
}
