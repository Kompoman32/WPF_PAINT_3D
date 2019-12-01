using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint
{
    public static class Extention
    {
        public static Color InvertedColor(this Color c)
        {
            var r = (byte)(255 - c.R);
            var g = (byte)(255 - c.G);
            var b = (byte)(255 - c.B);

            return Color.FromRgb(r,g,b);
        }

        public static Color InvertedColorNotVisualEqual(this Color c)
        {
            var r = (byte)(255 - c.R);
            var g = (byte)(255 - c.G);
            var b = (byte)(255 - c.B);

            if (r > 205 && g > 205 && b > 205)
            {
                r -= 50;
                g -= 50;
                b -= 50;
            } else
            {
                if (Math.Abs(r- c.R) < 10 && Math.Abs(g - c.G) < 10 && Math.Abs(b - c.B) < 10)
                {
                    var valR = r > c.R ? 50 : -50;
                    r += (byte)valR;
                    var valG = g > c.G ? 50 : -50;
                    g += (byte)valG;
                    var valB = b > c.B ? 50 : -50;
                    b += (byte)valB;
                }
            }

            return Color.FromRgb(r, g, b);
        }

        public static bool IsOnTheLine(this Point point, CustomLine line)
        {
            double distance = point.GetDistanceToLine(line);

            if (distance > line.StrokeThickness)
                return false;

            //double line_X1 = line.X1;
            //double line_X2 = line.X2;

            //double line_Y1 = line.Y1;
            //double line_Y2 = line.Y2;
            return true;

        }

        public static double GetDistanceToLine(this Point point, CustomLine line)
        {
            return Math.Abs(((line.X2 - line.X1)*(point.Y - line.Y1) - (line.Y2- line.Y1)*(point.X - line.X1))/Math.Sqrt(Math.Pow(line.X2 - line.X1,2) + Math.Pow(line.Y2 - line.Y1, 2)));
        }

        public static Point GetProjectionPointOnLine(this Point point, CustomLine line)
        {
            var equation = line.Equation;

            // line                 = Ax + By + C = 0
            // perpendicular line   = -Bx + Ay + C1 = 0

            double A = equation.A, B = equation.B, C = equation.C;
            double D = -B, E = A, F = -D * point.X - E* point.Y;

            double det = A * E - B * D;
            double X = (C * E - B * F) / det;
            double Y = (A * F - C * D) / det;

            return new Point(-X, Y);
        }

        public static void Move(this Line line, Point point1, Point point2)
        {
            line.X1 = point1.X;
            line.Y1 = point1.Y;

            line.X2 = point2.X;
            line.Y2 = point2.Y;
        }

        public static Point G(double A, double B, double C, double D, double E, double F)
        {
            double det = A * E - B * D;
            double X = (C * E - B * F) / det;
            double Y = (A * F - C * D) / det;

            return new Point(-X, -Y);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
    }
}
