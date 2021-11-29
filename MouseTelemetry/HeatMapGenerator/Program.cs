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
        private static string _dbPath = @"D:\Dev\pc_telemetry\MouseTelemetry\mouse_events_29112021Utc.db";
        private static string _sqlQuery = string.Format("SELECT * FROM \"{0}\"", nameof(MouseEvent));
        private static IEnumerable<MouseAction> _actions;
        private static IEnumerable<MouseButton> _buttons;
        private static List<MouseAction> _defaultActions = Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>().ToList();
        private static List<MouseButton> _defaultButtons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList();
        private static bool _parsingOk = true;

        static void Main(string[] args)
        {
#if DEBUG
            _actions = new List<MouseAction>() { MouseAction.Click, MouseAction.DoubleClick, MouseAction.Drag };
            _buttons = new List<MouseButton>() { MouseButton.Left };
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

        private static async Task GenerateHeatMap(IEnumerable<MouseAction> actions, IEnumerable<MouseButton> buttons)
        {
            List<MouseEvent> originalTestData = await ReadMouseEvents();

            while (true)
            {
                List<IMouseEvent> testData = new List<IMouseEvent>();
                Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> buttonActions = new Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>>();

                foreach (MouseButton button in _defaultButtons)
                {
                    Dictionary<MouseAction, IEnumerable<IMouseEvent>> actionEvents = new Dictionary<MouseAction, IEnumerable<IMouseEvent>>();

                    List<SecondaryMouseEvent> singleClicks = ClickAnalyzer.FindConsecutiveActions(originalTestData.Cast<IMouseEvent>(), button, MouseAction.Down, MouseAction.Up, MouseAction.Click, TimeSpan.FromMilliseconds(500), new Size(5, 5));
                    List<SecondaryMouseEvent> doubleClicks = ClickAnalyzer.FindConsecutiveActions(singleClicks.Cast<IMouseEvent>(), button, MouseAction.Click, MouseAction.Click, MouseAction.DoubleClick, TimeSpan.FromMilliseconds(500), SystemInformation.DoubleClickSize);
                    List<SecondaryMouseEvent> drags = ClickAnalyzer.FindActionsWithMovementInBetween(originalTestData.Cast<IMouseEvent>(), button, MouseAction.Down, MouseAction.Up, MouseAction.Drag, SystemInformation.DragSize);
                    double distance = ClickAnalyzer.FindTotalMovementDistance(originalTestData, button);

                    actionEvents[MouseAction.Click] = singleClicks.Cast<IMouseEvent>();
                    actionEvents[MouseAction.DoubleClick] = doubleClicks.Cast<IMouseEvent>();
                    actionEvents[MouseAction.Drag] = drags.Cast<IMouseEvent>();

                    // More here
                    buttonActions[button] = actionEvents;

                    testData.AddRange(singleClicks.Cast<IMouseEvent>());
                    testData.AddRange(doubleClicks.Cast<IMouseEvent>());
                    testData.AddRange(drags.Cast<IMouseEvent>());
                }


                testData.AddRange(originalTestData.Where(p => buttons.Contains(p.Button) && actions.Contains(p.Action)));

                if (testData.Count == 0)
                {
                    Console.WriteLine("No data points found");
                    continue;
                }
                Console.WriteLine(string.Format("{0} points found", testData.Count));

                DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
                string basePath = workDir.Parent.Parent.Parent.FullName;
                string dbPath = Path.Combine(basePath, "heatmap_" + TimeExtensions.GetCurrentTimeStampPrecise() + ".png");

                //Set up the factory and run the GetHeatMap function.
                HeatmapFactory map = new HeatmapFactory(originalTestData);
                map.OpenOnComplete = true;
                map.SaveLocation = dbPath;
                //map.ColorFunction = HeatmapFactory.GrayScale;
                map.ColorFunction = HeatmapFactory.BasicColorMapping;
                map.GetHeatMap(testData.Cast<MouseEvent>());
                Console.WriteLine("Exit? y/n");
                if (Console.ReadKey().ToString() == "y")
                {
                    break;
                }
            }
        }
    }
}
