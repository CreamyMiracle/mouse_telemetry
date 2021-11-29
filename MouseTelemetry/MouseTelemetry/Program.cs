using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Helpers;
using Fclp;
using MouseTelemetry.Model;

namespace MouseTelemetry
{
    class Program
    {
        private static MouseHook _mh;
        private static DataCollector _collector;
        private static string _dbDir;
        private static string _dbName;

        static void Main(string[] args)
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

            p.Parse(args);

            Run(args);

            Application.Run();
        }

        public static void Run(string[] args)
        {
            string dbPath = Path.Combine(_dbDir, _dbName + ".db");
            Console.WriteLine("Saving mouse events to '{0}'", dbPath);
            Console.SetWindowSize(50, 10);

            _mh = new MouseHook();
            _mh.SetHook();
            _mh.MouseEvent += mh_MouseEvent;

            _collector = new DataCollector(dbPath);
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
        #endregion
    }
}
