using MouseTelemetry.Helpers;
using MouseTelemetry.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Common.Helpers.Constants;
using static MouseTelemetry.Hooks.WindowHook;

namespace MouseTelemetry
{
    public class DataCollector
    {
        private int _totalEventsSaved = 0;
        private string _currentWindow = "";
        private string _windowOfInterest = "";

        Rectangle currentWindowRect = default;
        public DataCollector(string dbPath, string _windowOfInterest)
        {
            Task.Run(async () => { db_async = await InitDatabase(dbPath); }).Wait();
        }

        #region Public Methods
        public void ActiveWindowChanged(ActiveWindowInfoEventArgs e)
        {
            _currentWindow = e.Title;
            currentWindowRect = e.Rect;

            // To set max and min corner point to the data
            string title = Win32API.GetActiveWindowTitle();

            int maxLeft = Screen.AllScreens.ToList().Max(sc => sc.Bounds.Left);
            int minLeft = Screen.AllScreens.ToList().Min(sc => sc.Bounds.Left);

            int maxTop = Screen.AllScreens.ToList().Max(sc => sc.Bounds.Top);
            int minTop = Screen.AllScreens.ToList().Min(sc => sc.Bounds.Top);

            int maxRight = Screen.AllScreens.ToList().Max(sc => sc.Bounds.Right);
            int minRight = Screen.AllScreens.ToList().Min(sc => sc.Bounds.Right);

            int maxBottom = Screen.AllScreens.ToList().Max(sc => sc.Bounds.Bottom);
            int minBottom = Screen.AllScreens.ToList().Min(sc => sc.Bounds.Bottom);

            int left = Math.Abs(maxLeft) > Math.Abs(minLeft) ? maxLeft : minLeft;
            int top = Math.Abs(maxTop) > Math.Abs(minTop) ? maxTop : minTop;
            int right = Math.Abs(maxRight) > Math.Abs(minRight) ? maxRight : minRight;
            int bottom = Math.Abs(maxBottom) > Math.Abs(minBottom) ? maxBottom : minBottom;

            Console.WriteLine(left.ToString() + " " + top.ToString() + " " + right.ToString() + " " + bottom.ToString());

            Rectangle maxRect = new Rectangle(left, top, Math.Abs(left) + Math.Abs(right), Math.Abs(top) + Math.Abs(bottom));

            CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, maxRect.Left, maxRect.Top, 0, _currentWindow));
            CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, maxRect.Left, maxRect.Bottom, 0, _currentWindow));
            CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, maxRect.Right, maxRect.Top, 0, _currentWindow));
            CollectMouseEvent(new MouseEvent(MouseButton.WindowInit, MouseAction.WindowInit, maxRect.Right, maxRect.Bottom, 0, _currentWindow));
        }
        public void CollectMouseEvent(MouseEvent me)
        {
            // Block events from all other windows
            if (!_currentWindow.ToLower().Contains(_windowOfInterest.ToLower()))
            {
                return;
            }

            me.Window = _currentWindow;
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
            _totalEventsSaved += saveBatch.Count;
            saveBatch.Clear();
            Console.WriteLine(_totalEventsSaved);
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
