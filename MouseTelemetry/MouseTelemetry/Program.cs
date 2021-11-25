using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using SQLite;
using MouseTelemetry.Model;
using MouseTelemetry.Common;

namespace dummy_project2
{
    class Program
    {
        private static MouseHook _mh;
        private static SQLiteAsyncConnection db_async;
        private static List<MouseEvent> saveBatch = new List<MouseEvent>();
        private static List<MouseEvent> midSaveBatch = new List<MouseEvent>();
        private static Task saveBatchTask = null;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                db_async = await InitDatabase();
            }).Wait();

            _mh = new MouseHook();
            _mh.SetHook();
            _mh.MouseClickEvent += mh_MouseClickEvent;

            System.Windows.Forms.Application.Run();
        }
        #region Database stuff
        private static async Task<SQLiteAsyncConnection> InitDatabase()
        {
            #region Init
            DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
            string basePath = workDir.Parent.Parent.Parent.FullName;
            string dbPath = Path.Combine(basePath, "mouse_events_" + TimeExtensions.GetCurrentTimeStamp() + ".db");
            SQLiteAsyncConnection con = new SQLiteAsyncConnection(dbPath);

            await con.CreateTableAsync<MouseEvent>();
            return con;
            #endregion
        }
        private static async Task SaveBatch()
        {
            await db_async.InsertAllAsync(saveBatch);
            saveBatch.Clear();
            return;
        }
        private static bool BatchSaved()
        {
            if (saveBatchTask != null)
            {
                //Console.WriteLine("Checking if batch is saved: " + saveBatchTask.IsCompleted);
                return saveBatchTask.IsCompleted;
            }
            //Console.WriteLine("Checking if batch is saved: " + true);
            return true;
        }
        #endregion

        #region Mouse stuff
        private static void mh_MouseClickEvent(object sender, MouseEvent me)
        {
            if (me == null)
            {
                return;
            }

            if (BatchSaved())
            {
                if (midSaveBatch.Count > 0)
                {
                    //Console.WriteLine("Copying mid batch to batch");
                    Console.WriteLine(string.Format("Copying {0} events to batch", midSaveBatch.Count));
                    saveBatch = new List<MouseEvent>(midSaveBatch);
                    midSaveBatch.Clear();
                }

                //Console.WriteLine("Saving to batch");
                saveBatch.Add(me);
                if (saveBatch.Count >= 1000)
                {
                    Console.WriteLine(string.Format("Writing {0} events to DB", saveBatch.Count));
                    saveBatchTask = SaveBatch();
                }
            }
            else
            {
                //Console.WriteLine("Saving to mid batch");
                midSaveBatch.Add(me);
            }
        }
        #endregion
    }
}
