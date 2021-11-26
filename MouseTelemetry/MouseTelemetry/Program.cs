using System;
using System.Windows.Forms;
using MouseTelemetry.Model;

namespace MouseTelemetry
{
    class Program
    {
        private static MouseHook _mh;
        private static DataCollector _collector;

        static void Main(string[] args)
        {
            Console.WriteLine("Collecting mouse events..");
            Console.SetWindowSize(50, 10);

            _mh = new MouseHook();
            _mh.SetHook();
            _mh.MouseEvent += mh_MouseEvent;

            _collector = new DataCollector();

            Application.Run();
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
