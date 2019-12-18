using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Runtime.InteropServices;
using Paint.interfaces;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Paint
{
    public class CustomLine : CustomLine_Base, IMyObject
    {
        private CustomLine() : base()
        {
            CacheDefiningGeometry();
        }

        public CustomLine(Brush brush) : base()
        {
            _disp = Dispatcher;
            this.Stroke = brush;
            CacheDefiningGeometry();
        }

        public CustomLine(Brush brush, Point3D p1, Point3D p2) : base()
        {
            _disp = Dispatcher;
            this.Stroke = brush;
            X1 = p1.X;
            Y1 = p1.Y;
            Z1 = p1.Z;
            X2 = p2.X;
            Y2 = p2.Y;
            Z2 = p2.Z;
            CacheDefiningGeometry();
        }

        public CustomLine(Dispatcher disp, Brush brush, Point3D p1, Point3D p2) : base()
        {
            _disp = disp;
            this.Stroke = brush;
            X1 = p1.X;
            Y1 = p1.Y;
            Z1 = p1.Z;
            X2 = p2.X;
            Y2 = p2.Y;
            Z2 = p2.Z;
            CacheDefiningGeometry();
        }

        Dispatcher _disp;

        public bool IsFake = false;

        public Point3D Point1 => new Point3D(X1, Y1, Z1);
        public Point3D Point2 => new Point3D(X2, Y2, Z2);

        public bool IsPressed_Point_1 = false;
        public bool IsPressed_Point_2 = false;
        private bool isSelected = false;

        public bool isOnLine = true;

        Brush brush;
        public Brush OriginalBrush => brush;
        Brush selectedBrush;

        public double z1;
        public double Z1 {
            get { return z1; }
            set { z1 = value; InvalidateVisual(); }
        }
        public double z2;
        public double Z2
        {
            get { return z2; }
            set { z2 = value; InvalidateVisual(); }
        }

        public static Matrix3D DisplayProjectionMatrix;

        public Point3D originalPoint1;
        public Point3D originalPoint2;

        public new Brush Stroke
        {
            get
            {
                return GetStroke();
            }
            set
            {
                SetStroke(value);
            }
        }

        public void SetStroke(Brush brush)
        {
            if (brush != null)
            {
                _disp.Invoke(() =>
                {
                    Color valueColor = ((SolidColorBrush)brush).Color;
                    this.brush = new SolidColorBrush(valueColor);
                    this.selectedBrush = new SolidColorBrush(valueColor.InvertedColorNotVisualEqual());
                });
            }

            base.Stroke = GetStroke();
        }

        public Brush GetStroke()
        {
            return isSelected ? selectedBrush : brush;
        }

        private Geometry _Geometry;

        private IMyObject parentObj;
        public IMyObject GetParent()
        {
            return parentObj;
        }

        public void SetParent(IMyObject parent)
        {
            var existedParent = this.GetParent();
            parentObj = parent;
            if (existedParent != null && existedParent is IMyGroup)
            {
                (existedParent as IMyGroup).Remove(this);
            }
        }

        public Equation Equation => GetEquation();

        protected override Geometry DefiningGeometry
        {
            get
            {
                return this._Geometry;
            }
        }

        public override void CacheDefiningGeometry()
        {
            var x1 = this.X1;//+ MainWindow.CordCenter.X;
            var x2 = this.X2;//+ MainWindow.CordCenter.X;
            var y1 = this.Y1;//+ MainWindow.CordCenter.Y;
            var y2 = this.Y2;//+ MainWindow.CordCenter.Y;

            var matrix = DisplayProjectionMatrix;

            matrix.Scale(new Vector3D(MainWindow.Scale, MainWindow.Scale, MainWindow.Scale));

            var first = matrix.Transform(new Point3D(x1, y1, Z1));
            var second = matrix.Transform(new Point3D(x2, y2, Z2));

            x1 = first.X + MainWindow.CordCenter.X;
            x2 = second.X + MainWindow.CordCenter.X;
            y1 = first.Y+ MainWindow.CordCenter.Y;
            y2 = second.Y + MainWindow.CordCenter.Y;

            var line = new LineGeometry(new Point(x1, y1), new Point(x2, y2));

            if (isSelected)
            {
                var point1 = new EllipseGeometry(new Point(x1, y1), this.StrokeThickness, this.StrokeThickness);
                var point2 = new EllipseGeometry(new Point(x2, y2), this.StrokeThickness, this.StrokeThickness);

                var gg = new GeometryGroup() { FillRule = FillRule.Nonzero};
                gg.Children.Add(point1);
                gg.Children.Add(point2);
                gg.Children.Add(line);

                Path p = new Path();
                p.Data = gg;

                this._Geometry = p.Data;
            } 
            else
            {
                this._Geometry = line;
            }
            this.Stroke = null;
        }

        public bool IsNearToPoint1(Point point)
        {
            return IsNearToPoint(point, X1, Y1, Z1);
        }
        public bool IsNearToPoint2(Point point)
        {
            return IsNearToPoint(point, X2, Y2, Z2);
        }
        private bool IsNearToPoint(Point point, double x, double y, double z)
        {
            Point3D p = new Point3D(x, y, z);

            var linePoint = CustomLine.DisplayProjectionMatrix.Transform(p);

            var x_ = linePoint.X;
            var y_ = linePoint.Y;

            return Math.Abs(point.X - x_) < StrokeThickness * 4 + 8 && Math.Abs(point.Y - y_) < StrokeThickness * 4 + 8;
        }


        public void Move(double deltaX, double deltaY)
        {
            var x1 = X1;
            var y1 = Y1;
            var x2 = X2;
            var y2 = Y2;

            //Point3D p1 = new Point3D(this.X1, this.Y1, Z1);
            //Point3D p2 = new Point3D(this.X2, this.Y2, Z1);

            //var first = CustomLine.DisplayProjectionMatrix.Transform(p1);
            //var second = CustomLine.DisplayProjectionMatrix.Transform(p2);

            //x1 = first.X;
            //y1 = first.Y;
            //x2 = second.X;
            //y2 = second.Y;

            this.MoveTo(x1 + deltaX, x2 + deltaX, y1 + deltaY, y2 + deltaY);
        }
        public void Move(double deltaX, double deltaY, double deltaZ)
        {
            this.MoveTo(this.X1 + deltaX, this.X2 + deltaX, this.Y1 + deltaY, this.Y2 + deltaY, this.Z1 + deltaZ, this.Z2 + deltaZ);
        }
        public void MoveTo(double x1, double x2, double y1, double y2)
        {
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
        }
        public void MoveTo(double x1, double x2, double y1, double y2, double z1, double z2)
        {
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
            Z2 = z2;
            Z2 = z2;
        }
        public void MoveTo(Point pos1, Point pos2)
        {
            this.MoveTo(pos1.X, pos1.Y, pos2.X, pos2.Y);
        }
        public void MoveTo(Point3D pos1, Point3D pos2)
        {
            this.MoveTo(pos1.X, pos1.Y, pos2.X, pos2.Y, pos1.Z, pos2.Z);
        }

        private Equation GetEquation()
        {
            return new Equation(new Point3D(X1,Y1, Z1), new Point3D(X2, Y2, Z2));
        }

        public bool IsSelected()
        {
            return this.isSelected;
        }
        public void Select()
        {
            this.isSelected = true;
            this.InvalidateVisual();
        }
        public void UnSelect()
        {
            this.isSelected = false;
            SaveNewCords();
            this.InvalidateVisual();
        }

        public Tuple<Point3D, Point3D> GetSizeShape()
        {
            var minX = Math.Min(X1, X2);
            var maxX = Math.Max(X1, X2);
            var minY = Math.Min(Y1, Y2);
            var maxY = Math.Max(Y1, Y2);
            var minZ = Math.Min(Z1, Z2);
            var maxZ = Math.Max(Z1, Z2);

            return new Tuple<Point3D, Point3D>(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, minZ));
        }

        public void ConvertByMatrix(Matrix3D matrix)
        {

            if (originalPoint1.Equals(new Point3D(int.MaxValue, int.MaxValue, int.MaxValue))
                && originalPoint2.Equals(new Point3D(int.MaxValue, int.MaxValue, int.MaxValue))) 
            {
                originalPoint1 = new Point3D(X1, Y1, Z1);
                originalPoint2 = new Point3D(X2, Y2, Z2);
            }

            var first = matrix.Transform(new Point3D(originalPoint1.X, originalPoint1.Y, originalPoint1.Z));
            var second = matrix.Transform(new Point3D(originalPoint2.X, originalPoint2.Y, originalPoint2.Z));

            X1 = first.X; 
            Y1 = first.Y; 
            Z1 = first.Z; 
            X2 = second.X;
            Y2 = second.Y;
            Z2 = second.Z;



            //var newX1 = X1 * matrix.M11 + Y1 * matrix.M21 + Z1 * matrix.M31 + matrix.OffsetX;
            //var newY1 = X1 * matrix.M12 + Y1 * matrix.M22 + Z1 * matrix.M32 + matrix.OffsetY;
            //var newZ1 = X1 * matrix.M13 + Y1 * matrix.M22 + Z1 * matrix.M33 + matrix.OffsetZ;

            //var newX2 = X2 * matrix.M11 + Y2 * matrix.M21 + Z2 * matrix.M31 + matrix.OffsetX;
            //var newY2 = X2 * matrix.M12 + Y2 * matrix.M22 + Z2 * matrix.M32 + matrix.OffsetY;
            //var newZ2 = X2 * matrix.M13 + Y2 * matrix.M22 + Z2 * matrix.M33 + matrix.OffsetZ;

            //X1 = newX1;
            //Y1 = newY1;
            //Z1 = newZ1;

            //X2 = newX2;
            //Y2 = newY2;
            //Z2 = newZ2;
        }

        public void SaveNewCords()
        {
            originalPoint1 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
            originalPoint2 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
        }
        public void ResetOriginalCords()
        {
            if (originalPoint1.Equals(new Point3D(int.MaxValue, int.MaxValue, int.MaxValue))
                && originalPoint2.Equals(new Point3D(int.MaxValue, int.MaxValue, int.MaxValue)))
            {
                return;
            }

            X1 = originalPoint1.X;
            Y1 = originalPoint1.Y;
            Z1 = originalPoint1.Z;
            X2 = originalPoint2.X;
            Y2 = originalPoint2.Y;
            Z2 = originalPoint2.Z;

            originalPoint1 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
            originalPoint2 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
        }
    }

    public class Equation
    {
        public double A;
        public double B;
        public double C;
        public double D;

        public Equation(double a, double b,double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public Equation(Point3D p1, Point3D p2)
        {
            double X1 = p1.X - MainWindow.CanvasMargin;
            double Y1 = p1.Y - MainWindow.CanvasMargin;
            double Z1 = p1.Z - MainWindow.CanvasMargin;
            double X2 = p2.X - MainWindow.CanvasMargin;
            double Y2 = p2.Y - MainWindow.CanvasMargin;
            double Z2 = p2.Z - MainWindow.CanvasMargin;

            A = Y1 - Y2;
            B = X2 - X1;
            C = X1 * Y2 - X2 * Y1;
        }

        public override string ToString()
        {
            string b = B >= 0 ? "+ " + B.ToString("###0.0#") : B.ToString("###0.0#");
            string c = C >= 0 ? "+ " + C.ToString("###0.0#") : C.ToString("###0.0#");

            return $"{A:###0.0#}x {b}y {c} = 0";
        }

        public string ToNotRightString()
        {
            if (B == 0)
            {
                return "vertical";
            }

            double k = A/B;

            double b = C/B;


            string b_str = k >= 0 ? "+ " + k.ToString("###0.0#") : k.ToString("###0.0#");
            string k_str = b >= 0 ? "+ " + b.ToString("###0.0#") : b.ToString("###0.0#");

            return $"y = {k_str:###0.0#}x {b_str:###0.0#}";
        }
    }
}
