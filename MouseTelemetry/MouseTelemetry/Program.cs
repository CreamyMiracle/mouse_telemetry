using System;
using System.IO;
using System.Windows.Forms;
using Common.Helpers;
using Fclp;
using MouseTelemetry.Hooks;
using MouseTelemetry.Model;

namespace MouseTelemetry
{
    class Program
    {
        private static MouseHook _mh;
        private static WindowHook _wh;
        private static DataCollector _collector;
        private static string _dbDir = Constants.DefaultDatabasePath;
        private static string _dbName = Constants.DefaultDatabaseName;
        private static bool _parsingOk = true;

        static void Main(string[] args)
        {
#if DEBUG
            Run(args);
#else
            if (Parse(args))
            {
                Run(args);
            }
#endif
            Application.Run();
        }

        private static bool Parse(string[] args)
        {
            var p = new FluentCommandLineParser();

            p.Setup<string>('d', "dir")
             .Callback(value => _dbDir = value)
             .SetDefault(Constants.DefaultDatabasePath)
             .WithDescription("Directory to which an output database file is saved");

            p.Setup<string>('n', "name")
             .Callback(value => _dbName = value)
             .SetDefault(Constants.DefaultDatabaseName)
             .WithDescription("Name for database file");

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

        public static void Run(string[] args)
        {
            string dbPath = Path.Combine(_dbDir, _dbName + ".db");
            Console.WriteLine("Saving mouse events to '{0}'", dbPath);
            Console.SetWindowSize(50, 10);

            _collector = new DataCollector(dbPath);

            _mh = new MouseHook();
            _mh.SetHook();
            _mh.MouseEvent += mh_MouseEvent;

            _wh = new WindowHook();
            _wh.SetHook();
            _wh.WindowChanged += wh_WindowEvent;

            _collector.ActiveWindowChanged(_wh.GetActiveWindowTitle());
        }

        #region Mouse stuff
        private static void mh_MouseEvent(object sender, MouseEvent me)
        {
            try
            {
                _collector.CollectMouseEvent(me);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private static void wh_WindowEvent(object sender, string name)
        {
            try
            {
                _collector.ActiveWindowChanged(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion
    }
}
