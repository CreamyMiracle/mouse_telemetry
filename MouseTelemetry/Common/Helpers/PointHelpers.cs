using MouseTelemetry.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class PointHelpers
    {
        private static int NormalizeX(IEnumerable<MouseEvent> points)
        {
            double valueMax = points.Max(p => p.X);
            double valueMin = points.Min(p => p.X);
            double valueMinAbs = Math.Abs(valueMin);

            double transformedValueMax = valueMax + valueMinAbs;
            double transformedValueMin = valueMin + valueMinAbs; // should be 0

            double scaleMin = 0; //the normalized minimum desired
            double scaleMax = transformedValueMax; //the normalized maximum desired

            double valueRange = valueMax - valueMin;
            double scaleRange = scaleMax - scaleMin;

            foreach (MouseEvent point in points)
            {
                point.X += Convert.ToInt32(valueMinAbs);
            }

            return Convert.ToInt32(scaleMax);
        }

        private static int NormalizeY(IEnumerable<MouseEvent> points)
        {
            double valueMax = points.Max(p => p.Y);
            double valueMin = points.Min(p => p.Y);
            double valueMinAbs = Math.Abs(valueMin);

            double transformedValueMax = valueMax + valueMinAbs;
            double transformedValueMin = valueMin + valueMinAbs; // should be 0

            double scaleMin = 0; //the normalized minimum desired
            double scaleMax = transformedValueMax; //the normalized maximum desired

            double valueRange = valueMax - valueMin;
            double scaleRange = scaleMax - scaleMin;

            foreach (MouseEvent point in points)
            {
                point.Y += Convert.ToInt32(valueMinAbs);
            }

            return Convert.ToInt32(scaleMax);
        }

        public static Point NormalizeData(IEnumerable<MouseEvent> points)
        {
            // Normalizes point to non-negative scale and returns the new scaled X and Y values. Minimum values are 0
            int maxInputX = points.Max(p => p.X);
            int maxInputY = points.Max(p => p.Y);
            int minInputX = points.Min(p => p.X);
            int minInputY = points.Min(p => p.Y);

            int maxX = NormalizeX(points);
            int maxY = NormalizeY(points);

            return new Point(maxX, maxY);
        }
    }
}
