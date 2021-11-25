using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace dummy_project2
{
    class Program
    {
        private static MouseHook _mh;
        static void Main(string[] args)
        {
            _mh = new MouseHook();
            _mh.SetHook();
            _mh.MouseMoveEvent += mh_MouseMoveEvent;
            _mh.MouseClickEvent += mh_MouseClickEvent;
            _mh.MouseDownEvent += mh_MouseDownEvent;
            _mh.MouseUpEvent += mh_MouseUpEvent;

            System.Windows.Forms.Application.Run();
        }
        private static void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Console.WriteLine("Left Button Press" + " at " + DateTime.Now.Ticks);
            }
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("Right Button Press" + " at " + DateTime.Now.Ticks);
            }
        }
        private static void mh_MouseUpEvent(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                Console.WriteLine("Left Button Release" + " at " + DateTime.Now.Ticks);
            }
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("Right Button Release" + " at " + DateTime.Now.Ticks);
            }

        }
        private static void mh_MouseClickEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                string sText = "(" + e.X.ToString() + "," + e.Y.ToString() + ")";
                Console.WriteLine(sText);
            }
        }
        private static void mh_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            int x = e.Location.X;
            int y = e.Location.Y;
            Console.WriteLine("Mouse move X:" + x + " Y:" + y + " at " + DateTime.Now.Ticks);
        }
    }
}
