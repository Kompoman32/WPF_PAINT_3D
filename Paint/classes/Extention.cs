using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Paint
{
    public static class Extention
    {
        static Random rnd = new Random();

        public static Color InvertedColor(this Color c)
        {
            var r = (byte)(255 - c.R);
            var g = (byte)(255 - c.G);
            var b = (byte)(255 - c.B);

            return Color.FromRgb(r, g, b);
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
                if (Math.Abs(r - c.R) < 10 && Math.Abs(g - c.G) < 10 && Math.Abs(b - c.B) < 10)
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
            return Math.Abs(((line.X2 - line.X1) * (point.Y - line.Y1) - (line.Y2 - line.Y1) * (point.X - line.X1)) / Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)));
        }

        public static Point GetProjectionPointOnLine(this Point point, CustomLine line)
        {
            var equation = line.Equation;

            // line                 = Ax + By + C = 0
            // perpendicular line   = -Bx + Ay + C1 = 0

            double A = equation.A, B = equation.B, C = equation.C;
            double D = -B, E = A, F = -D * point.X - E * point.Y;

            double det = A * E - B * D;
            double X = (C * E - B * F) / det;
            double Y = (A * F - C * D) / det;

            return new Point(-X, -Y);
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

        public static double GetDistance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        public static Point GetPerpendicularPoint(this CustomLine line, Point point)
        {
            var A1 = line.Equation.A;
            var B1 = line.Equation.B;
            var C1 = line.Equation.C;
            var A2 = -B1;
            var B2 = A1;
            var C2 = -(A2 * point.X + B2 * point.Y);

            var X = B1 * C2 - B2 * C1;
            var Y = A2 * C1 - A1 * C2;

            var Z = A1 * B2 - A2 * B1;


            return new Point(X / Z, -Y / Z);
        }

        public static Point GetMedianPoint(this CustomLine line)
        {
            return new Point((line.X1 + line.X2) / 2, (line.Y1 + line.Y2) / 2);
        }

        public static bool CheckBisector(CustomLine firstLine, CustomLine secondLine)
        {
            var matrix = new Matrix3D(firstLine.X1 - firstLine.X2, firstLine.Y1 - firstLine.Y2, firstLine.Z1 - firstLine.Z2, 0,
                                      secondLine.X1 - secondLine.X2, secondLine.Y1 - secondLine.Y2, secondLine.Z1 - secondLine.Z2, 0,
                                      firstLine.X1 - secondLine.X1, firstLine.Y1 - secondLine.Y1, firstLine.Z1 - secondLine.Z1, 0,
                                      0, 0, 0, 1);
            if (matrix.Determinant != 0)
            {
                return false;
            };

            var ok = true;
            var first = (firstLine.X1 - firstLine.X2) * (secondLine.Y1 - secondLine.Y2) * (secondLine.Z1 - secondLine.Z2);
            var second = (secondLine.X1 - secondLine.X2) * (firstLine.Y1 - firstLine.Y2) * (secondLine.Z1 - secondLine.Z2);
            ok &= first == second;
            second = (secondLine.X1 - secondLine.X2) * (secondLine.Y1 - secondLine.Y2) * (firstLine.Z1 - firstLine.Z2);
            ok &= first == second;

            return ok;
        }

        public static Point GetIntersection(this CustomLine line, CustomLine to)
        {
            var A1 = line.Equation.A;
            var B1 = line.Equation.B;
            var C1 = line.Equation.C;
            var A2 = to.Equation.A;
            var B2 = to.Equation.B;
            var C2 = to.Equation.C;

            return GetIntersection(A1, B1, C1, A2, B2, C2);
        }

        public static Point GetIntersection(double A1, double B1, double C1, double A2, double B2, double C2)
        {
            var X = B1 * C2 - B2 * C1;
            var Y = A2 * C1 - A1 * C2;

            var Z = A1 * B2 - A2 * B1;


            return new Point(X / Z, Y / Z);
        }

        public static Tuple<Point, Point> GetBisectorPoint(CustomLine firstLine, CustomLine secondLine)
        {
            var A1 = firstLine.Equation.A;
            var B1 = firstLine.Equation.B;
            var C1 = firstLine.Equation.C;
            var A2 = secondLine.Equation.A;
            var B2 = secondLine.Equation.B;
            var C2 = secondLine.Equation.C;

            

            //var firstPossibleA = A1 * sqrt2 - A2 * sqrt1;
            //var secondPossibleA = A1 * sqrt2 + A2 * sqrt1;

            //var firstPossibleB = B1 * sqrt2 - B2 * sqrt1;
            //var secondPossibleB = B1 * sqrt2 + B2 * sqrt1;

            //var firstPossibleC = C1 * sqrt2 - C2 * sqrt1;
            //var secondPossibleC = C1 * sqrt2 + C2 * sqrt1;

            var intersection = GetIntersection(firstLine, secondLine);

            var length1 = Math.Sqrt(Math.Pow(intersection.X - firstLine.X1, 2) + Math.Pow(intersection.Y - firstLine.Y1,  2));
            var length2 = Math.Sqrt(Math.Pow(intersection.X - secondLine.X1, 2) + Math.Pow(intersection.Y - secondLine.Y1,  2));

            //var possiblePoint1 = new Point(0, -firstPossibleC / firstPossibleB);
            //var possiblePoint2 = new Point(-firstPossibleC / firstPossibleA, 0);
            //var possiblePoint3 = new Point(intersection.X, intersection.Y);

            //var x = rnd.Next(-2000, 2000);

            // y = -Bx -C / A 

            //var possiblePoint4 = new Point(x, (-secondPossibleB* x - secondPossibleC) / secondPossibleA);
            //x = -c/a

            var lambda = length1 / length2;

            var x = (firstLine.X1 + lambda * secondLine.X1) / (1 + lambda);
            var y = (firstLine.Y1 + lambda * secondLine.Y1) / (1 + lambda);

            return new Tuple<Point, Point>( new Point(intersection.X, intersection.Y), new Point(x,y));
        }

    }
}
