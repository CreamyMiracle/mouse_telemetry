using MouseTelemetry.Helpers;
using MouseTelemetry.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using static Common.Helpers.Constants;
using static MouseTelemetry.Hooks.WindowHook;

namespace MouseTelemetry
{
    public class DataCollector
    {
        int totalEventsSaved = 0;
        string currentWindow = "";
        Rectangle currentWindowRect = default;
        public DataCollector(string dbPath)
        {
            Task.Run(async () => { db_async = await InitDatabase(dbPath); }).Wait();
        }

        #region Public Methods
        public void ActiveWindowChanged(ActiveWindowInfoEventArgs e)
        {
            currentWindow = e.Title;
            currentWindowRect = e.Rect;

            // To set max and min corner point to the data
            //CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, currentWindowRect.Left, currentWindowRect.Top, 0, currentWindow));
            //CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, currentWindowRect.Left, currentWindowRect.Bottom, 0, currentWindow));
            //CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, currentWindowRect.Right, currentWindowRect.Top, 0, currentWindow));
            //CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, currentWindowRect.Right, currentWindowRect.Bottom, 0, currentWindow));
        }
        public void CollectMouseEvent(MouseEvent me)
        {
            me.Window = currentWindow;
            if (BatchSaved())
            {
                if (midSaveBatch.Count > 0)
                {
                    saveBatch = new List<MouseEvent>(midSaveBatch);
                    midSaveBatch.Clear();
                }

                saveBatch.Add(me);
                if (saveBatch.Count >= batchSize)
                {
                    saveBatchTask = SaveBatch();
                }
            }
            else
            {
                midSaveBatch.Add(me);
            }
        }
        #endregion

        #region Private Methods
        private async Task<SQLiteAsyncConnection> InitDatabase(string dbPath)
        {
            SQLiteAsyncConnection con = new SQLiteAsyncConnection(dbPath);

            await con.CreateTableAsync<MouseEvent>();
            return con;
        }
        private async Task SaveBatch()
        {
            await db_async.InsertAllAsync(saveBatch);
            totalEventsSaved += saveBatch.Count;
            saveBatch.Clear();
            Console.WriteLine(totalEventsSaved);
            return;
        }
        private bool BatchSaved()
        {
            if (saveBatchTask != null)
            {
                return saveBatchTask.IsCompleted;
            }
            return true;
        }
        #endregion

        #region Private Fields
        private SQLiteAsyncConnection db_async;
        private List<MouseEvent> saveBatch = new List<MouseEvent>();
        private List<MouseEvent> midSaveBatch = new List<MouseEvent>();
        private Task saveBatchTask = null;
        private int batchSize = 1000;
        #endregion
    }
}
