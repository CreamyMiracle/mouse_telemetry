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
        private static string _dbPath = "";
        private static string _baseImgPath = "";
        private static bool _connectPoints = false;
        private static string _sqlQuery = string.Format("SELECT * FROM \"{0}\"", nameof(MouseEvent));
        private static IEnumerable<MouseAction> _actions;
        private static IEnumerable<MouseButton> _buttons;
        private static IEnumerable<string> _windows;
        private static List<MouseAction> _defaultActions = Enum.GetValues(typeof(MouseAction)).Cast<MouseAction>().ToList();
        private static List<MouseButton> _defaultButtons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList();
        private static List<string> _defaultWindows = new List<string>() { };
        private static bool _parsingOk = true;
        private static string mWorkDir = "";

        static void Main(string[] args)
        {
            DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
            mWorkDir = workDir.FullName;
#if DEBUG
            mWorkDir = workDir.Parent.Parent.Parent.FullName;
            _baseImgPath = @"F:\Dev\pc_telemetry\MouseTelemetry\baseimg.png";
            _dbPath = @"F:\Dev\pc_telemetry\MouseTelemetry\mouse_events_08122021Utc.db";

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

            p.Setup<string>('d', "data")
             .Callback(value => _dbPath = value)
             .Required()
             .WithDescription("Path from which a database file is read");

            p.Setup<string>('i', "image")
             .Callback(value => _baseImgPath = value)
             .WithDescription("Path to the image used as base for heatmap");

            p.Setup<bool>('c', "connect")
             .Callback(value => _connectPoints = value)
             .WithDescription("True points are connected, False if not");

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
                // Read all mouse events recorded
                List<MouseEvent> allEvents = await ReadMouseEvents();

                // XY-coordinates may be anything f.ex. (-1980,0) or (1080,-567)
                // This normalizez the XY-coordinates so that they are non-negative
                Point maxPoint = PointHelpers.NormalizeData(allEvents);

                // Extracts primary mouse events (like LeftDown, LeftUp, MiddleDown) and secondary events like (Click, DoubleClick, Drag)
                // Then prints those. This all is just for fun.
                Dictionary<MouseButton, Dictionary<MouseAction, IEnumerable<IMouseEvent>>> btnActs = EventExtractor.ExtractButtonActions(allEvents);
                EventExtractor.PrintButtonActions(btnActs, _windows);

                // Filters just those events we are interested in
                List<IMouseEvent> selectedData = EventExtractor.SelectCertain(btnActs, _actions, _buttons, _windows);

                // Generates a heatmap bitmap
                Bitmap heatMap = GenerateHeatMap(selectedData.Cast<MouseEvent>(), maxPoint);

                // Connect points
                if (_connectPoints) { heatMap.DrawLine(selectedData.Cast<MouseEvent>()); }

                // Get image that is used as a base for heatmap
                FileInfo baseImgFile = new FileInfo(_baseImgPath);
                Bitmap baseImg = baseImgFile.Exists ? new Bitmap(_baseImgPath) : new Bitmap(maxPoint.X, maxPoint.Y);

                // Overlay base image with heatmap
                Bitmap overlayed = BitmapExtensions.OverlayWith(baseImg, heatMap);

                // Save overlayed heatmap
                overlayed.SaveBitmap(Path.Combine(mWorkDir, "heatmap_with_overlay_" + TimeExtensions.GetCurrentTimeStampPrecise() + ".png"));

            }).Wait();
        }

        private static async Task<List<MouseEvent>> ReadMouseEvents()
        {
            List<MouseEvent> events = await db_async.QueryAsync<MouseEvent>(_sqlQuery);//await db_async.GetAllWithChildrenAsync<MouseEvent>();
            return events;
        }

        private static Bitmap GenerateHeatMap(IEnumerable<MouseEvent> events, Point maxPoint)
        {
            string dbPath = Path.Combine(mWorkDir, "heatmap_" + TimeExtensions.GetCurrentTimeStampPrecise() + ".png");

            //Set up the factory and run the GetHeatMap function.
            HeatmapFactory map = new HeatmapFactory(maxPoint);
            //map.ColorFunction = HeatmapFactory.GrayScale;
            map.ColorFunction = HeatmapFactory.BasicColorMapping;
            Bitmap heatMap = map.GetHeatMap(events);

            return heatMap;
        }
    }
}
