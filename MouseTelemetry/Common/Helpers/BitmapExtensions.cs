using MouseTelemetry.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Helpers.Constants;

namespace Common.Helpers
{
    public static class BitmapExtensions
    {
        public static void SaveBitmap(this Bitmap bitmap, string path)
        {
            using (FileStream f = File.Open(path, FileMode.Create))
            {
                bitmap.Save(f, ImageFormat.Png);
            }
        }
        public static void DrawLine(this Bitmap bmp, IEnumerable<MouseEvent> events)
        {
            List<MouseEvent> orderedEvents = events.OrderBy(p => p.Timestamp).ToList();

            using (var graphics = Graphics.FromImage(bmp))
            {
                Pen blackPen = new Pen(Color.Black, 2);
                MouseEvent nextEvent = null;
                int currInd = 0;
                foreach (MouseEvent currEvent in orderedEvents)
                {
                    nextEvent = currInd + 1 < orderedEvents.Count ? orderedEvents.ElementAt(currInd + 1) : null;
                    if (nextEvent == null) { break; }

                    graphics.DrawLine(blackPen, currEvent.X, currEvent.Y, nextEvent.X, nextEvent.Y);
                    currInd++;
                }
            }
        }
    }
}
