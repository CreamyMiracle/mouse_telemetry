using MouseTelemetry.Common;
using MouseTelemetry.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MouseTelemetry
{
    public class DataCollector
    {
        int totalEventsSaved = 0;
        public DataCollector()
        {
            Task.Run(async () => { db_async = await InitDatabase(); }).Wait();
        }

        #region Public Methods
        public void CollectMouseEvent(MouseEvent me)
        {
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
        private async Task<SQLiteAsyncConnection> InitDatabase()
        {
            DirectoryInfo workDir = new DirectoryInfo(Environment.CurrentDirectory);
            string basePath = workDir.Parent.Parent.Parent.FullName;
            string dbPath = Path.Combine(basePath, "mouse_events_" + TimeExtensions.GetCurrentTimeStamp() + ".db");
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
