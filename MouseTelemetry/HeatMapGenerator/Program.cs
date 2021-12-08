using Common.Helpers;
using Fclp;
using MouseTelemetry.Helpers;
using MouseTelemetry.Model;
using SQLite;
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
        private static string _dbPath = @"D:\Dev\pc_telemetry\MouseTelemetry\mouse_events_08122021Utc.db";
        private static string _sqlQuery = string.Format("SELECT * FROM \"{0}\"", nameof(MouseEvent));
        private static IEnumerable<MouseAction> _actions;
        private static IEnumerable<MouseButton> _buttons;
        private static IEnumerable<string> _windows;
        private static List<MouseAction> _defaultActions = Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>().ToList();
        private static List<MouseButton> _defaultButtons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList();
        private static List<string> _defaultWindows = new List<string>() { };
        private static bool _parsingOk = true;

        static void Main(string[] args)
        {
#if DEBUG
            _windows = _defaultWindows;
            _actions = _defaultActions;
            _buttons = _defaultButtons;
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

            p.Setup<List<string>>('w', "windows").Callback(value => _windows = value)
            .SetDefault(_defaultWindows)
            .WithDescription("Full or partial titles of windows that are taken into consideration. Values: " + "{" + string.Join("|", _defaultWindows) + "}");

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
                Bitmap heatMap = await GenerateHeatMap(_actions, _buttons, _windows);
                Bitmap baseImg = new Bitmap(@"D:\Dev\pc_telemetry\MouseTelemetry\baseimg.PNG");
                Bitmap overlayed = BitmapExtensions.OverlayWith(baseImg, heatMap);
                overlayed.SaveBitmap(@"D:\Dev\pc_telemetry\MouseTelemetry\heatmap_with_overlay_" + TimeExtensions.GetCurrentTimeStampPrecise() + ".png");
            }).Wait();
        }

        private static async Task<List<MouseEvent>> ReadMouseEvents()
        {
            List<MouseEvent> events = await db_async.QueryAsync<MouseEvent>(_sqlQuery);//await db_async.GetAllWithChildrenAsync<MouseEvent>();
            return events;
        }

        private static async Task<Bitmap> GenerateHeatMap(IEnumerable<MouseAction> actions, IEnumerable<MouseButton> buttons, IEnumerable<string> windows)
        {
            List<MouseEvent> originalTestData = await ReadMouseEvents();

            Point maxPoint = PointHelpers.NormalizeData(originalTestData);

            Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> btnActs = EventExtractor.ExtractButtonActions(originalTestData);
            List<IMouseEvent> selectedData = EventExtractor.SelectCertain(btnActs, actions, buttons, _windows);
            EventExtractor.PrintButtonActions(btnActs, _windows);


            DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
            string basePath = workDir.Parent.Parent.Parent.FullName;
            string dbPath = Path.Combine(basePath, "heatmap_" + TimeExtensions.GetCurrentTimeStampPrecise() + ".png");

            //Set up the factory and run the GetHeatMap function.
            HeatmapFactory map = new HeatmapFactory(originalTestData, maxPoint);
            //map.ColorFunction = HeatmapFactory.GrayScale;
            map.ColorFunction = HeatmapFactory.BasicColorMapping;
            Bitmap heatMap = map.GetHeatMap(selectedData.Cast<MouseEvent>());
            heatMap.DrawLine(selectedData.Cast<MouseEvent>());

            return heatMap;
        }
    }
}
