using MouseTelemetry.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        public static Bitmap OverlayWith(Bitmap baseImg, Bitmap overlay)
        {
            Bitmap resizedImage = ResizeImage(baseImg, overlay.Width, overlay.Height);
            Bitmap opacityImg1 = SetImageOpacity(resizedImage, (float)0.50);
            Bitmap opacityImg2 = SetImageOpacity(overlay, (float)0.50);

            var target = new Bitmap(opacityImg1.Width, opacityImg1.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(opacityImg1, 0, 0);
            graphics.DrawImage(opacityImg2, 0, 0);

            return target;
        }

        public static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public static Bitmap SetImageOpacity(Bitmap image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                throw ex;
                //return null;
            }
        }
    }
}
