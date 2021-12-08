using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using MouseTelemetry.Model;

namespace Heatmap
{
    class HeatmapFactory
    {
        public HeatmapFactory(IEnumerable<MouseEvent> points, Point maxPoint)
        {
            InputMax = maxPoint;
            OutputResolution = maxPoint;
        }

        //Represents the maximum values of inputs to the heat function.  If there are larger values, they will be scaled down.
        public Point InputMax { get; set; }

        //Resolution of the image that gets produced by the heat map.
        public Point OutputResolution { get; set; }

        //This function maps from the heat score to a value. Input: a value from 0 - 1.  Outut: Modified value.
        public Func<float, float> HeatFunction { get; set; }

        //Maps from a heat value to the actual color used for the image.
        public Func<float, Color> ColorFunction { get; set; }

        public HeatMask HeatMask { get; set; }

        public Bitmap GetHeatMap(IEnumerable<MouseEvent> points)
        {
            if (InputMax == null)
            {
                InputMax = new Point(1920, 1080);
            }

            //TODO Scale down the input to output resolution if it is larger.
            if (OutputResolution == null)
            {
                OutputResolution = new Point(1920, 1080);
            }

            if (HeatFunction == null)
            {
                HeatFunction = (f => f);
            }

            if (ColorFunction == null)
            {
                ColorFunction = HeatmapFactory.GrayScale;
            }

            if (HeatMask == null)
            {
                HeatMask = HeatmapFactory.LinearFalloff(40, 40);
            }

            //Heatbin is used as a value store for heat resutling from point inputs.
            int[][] Heatbin = new int[OutputResolution.X][];
            for (int i = 0; i < OutputResolution.X; i++)
            {
                Heatbin[i] = new int[OutputResolution.Y];
            }

            //Initialize Heatbin
            for (int i = 0; i < OutputResolution.X; i++)
            {
                for (int j = 0; j < OutputResolution.Y; j++)
                {
                    Heatbin[i][j] = 0;
                }
            }

            //Test the input to make sure its in the right format.
            CheckInput(points);

            AssignHeat(Heatbin, points);

            float[][] normalized = NormalizedHeatbin(Heatbin);

            Bitmap output = GetBitmap(normalized);

            return output;
        }

        //This function checks the value of every point in the input list.  If the point is larger than our max value, we scale it back down.
        //The max value in the list will be mapped to the max value of the input range.
        private void CheckInput(IEnumerable<MouseEvent> points)
        {
            // yes
        }

        private void AssignHeat(int[][] bin, IEnumerable<MouseEvent> points)
        {
            //TODO: Because we store the information as ints, we are bound by the size of an int.
            //This shouldn;t be a problem as we can have something of the order of 20 million at +100.

            foreach (MouseEvent p in points)
            {

                for (int i = -HeatMask.HeatMapXRadius; i < HeatMask.HeatMapXRadius; i++)
                {
                    if ((p.X + i) > -1 && p.X + i < OutputResolution.X)
                    {
                        for (int j = -HeatMask.HeatMapYRadius; j < HeatMask.HeatMapYRadius; j++)
                        {
                            if ((p.Y + j) > -1 && (p.Y + j) < OutputResolution.Y)
                            {

                                bin[p.X + i][p.Y + j] += (int)(100 * HeatMask.value(i, j));

                            }
                        }
                    }
                }
            }
        }

        //Takes a heatbin and normalizes it by the maximum value.
        private float[][] NormalizedHeatbin(int[][] heatbin)
        {
            //We need to convert the int values to floats and so need another array.
            float[][] NormalizedHeatbin = new float[OutputResolution.X][];
            for (int i = 0; i < OutputResolution.X; i++)
            {
                NormalizedHeatbin[i] = new float[OutputResolution.Y];
            }

            int maxHeat = 0;
            //We need the maximum values for the purposes of Normalization.
            for (int i = 0; i < OutputResolution.X; i++)
            {
                for (int j = 0; j < OutputResolution.Y; j++)
                {
                    if (heatbin[i][j] > maxHeat)
                    {
                        maxHeat = heatbin[i][j];
                    }
                }
            }

            //And now we need to normalize

            for (int i = 0; i < OutputResolution.X; i++)
            {
                for (int j = 0; j < OutputResolution.Y; j++)
                {
                    NormalizedHeatbin[i][j] = HeatFunction((float)heatbin[i][j] / (float)maxHeat);
                }
            }

            return NormalizedHeatbin;
        }

        private Bitmap GetBitmap(float[][] normalizedValues)
        {

            Bitmap heatmap = new Bitmap(OutputResolution.X, OutputResolution.Y, PixelFormat.Format32bppArgb);

            for (int i = 0; i < OutputResolution.X; i++)
            {
                for (int j = 0; j < OutputResolution.Y; j++)
                {
                    int i2 = 0;
                    try
                    {
                        heatmap.SetPixel(i, j, ColorFunction(normalizedValues[i][j]));
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e);
                    }

                }
            }


            return heatmap;
        }

        /// <summary>
        /// A heat mask where the heat intensity falls off as 1 / distance from the center point.
        /// </summary>
        /// <param name="xRadius"></param>
        /// <param name="yRadius"></param>
        /// <returns></returns>
        public static HeatMask LinearFalloff(int xRadius, int yRadius)
        {
            HeatMask heatMask;

            float[][] heatMaskArray = new float[(xRadius * 2) + 1][];

            for (int i = 0; i < (xRadius * 2) + 1; i++)
            {
                heatMaskArray[i] = new float[(yRadius * 2) + 1];
            }

            for (int i = -xRadius; i <= xRadius; i++)
            {
                for (int j = -yRadius; j <= yRadius; j++)
                {
                    float distance;

                    if (i == 0 && j == 0)
                    {
                        distance = 1;
                    }
                    else
                    {
                        distance = (float)Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2));

                    }

                    heatMaskArray[i + xRadius][j + xRadius] = (1 / distance);
                }
            }

            heatMask = new HeatMask(xRadius, yRadius, heatMaskArray);
            return heatMask;
        }

        public static Color BasicColorMapping(float f)
        {
            Color color;
            if (f < 0.5)
            {
                color = Color.FromArgb(255, 0, (int)(255 * 2 * f), (int)(255 * (1 - (2 * f))));
            }
            else
            {
                color = Color.FromArgb(255, (int)(255 * f), (int)(255 * (2 - (2 * f))), 0);
            }

            return color;
        }

        public static Color GrayScale(float f)
        {
            Color color = Color.FromArgb(255, (int)(f * 255), (int)(f * 255), (int)(f * 255));

            return color;
        }

    }
}
