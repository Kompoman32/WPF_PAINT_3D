﻿using System;
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
            this.Stroke = brush;
            CacheDefiningGeometry();
        }


        public bool IsPressed_Point_1 = false;
        public bool IsPressed_Point_2 = false;
        private bool isSelected = false;

        Brush brush;
        public Brush OriginalBrush => brush;
        Brush selectedBrush;

        public double Z1;
        public double Z2;

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
                var valueColor = ((SolidColorBrush)brush).Color;
                this.brush = new SolidColorBrush(valueColor);
                this.selectedBrush = new SolidColorBrush(valueColor.InvertedColorNotVisualEqual());
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
            var x1 = this.X1 + MainWindow.CordCenter.X;
            var x2 = this.X2 + MainWindow.CordCenter.X;
            var y1 = this.Y1 + MainWindow.CordCenter.Y;
            var y2 = this.Y2 + MainWindow.CordCenter.Y;

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

        public void Move(double deltaX, double deltaY)
        {
            this.MoveTo(this.X1 + deltaX, this.X2 + deltaX, this.Y1 + deltaY, this.Y2 + deltaY);
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
            return new Equation(new Point(X1,Y1), new Point(X2, Y2));
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

            X1 = originalPoint1.X;
            Y1 = originalPoint1.Y;
            Z1 = originalPoint1.Z;
            X2 = originalPoint2.X;
            Y2 = originalPoint2.Y;
            Z2 = originalPoint2.Z;

            var newX1 = X1 * matrix.M11 + Y1 * matrix.M21 + Z1 * matrix.M31 + matrix.OffsetX;
            var newY1 = X1 * matrix.M12 + Y1 * matrix.M22 + Z1 * matrix.M32 + matrix.OffsetY;
            var newZ1 = X1 * matrix.M13 + Y1 * matrix.M22 + Z1 * matrix.M33 + matrix.OffsetZ;

            var newX2 = X2 * matrix.M11 + Y2 * matrix.M21 + Z2 * matrix.M31 + matrix.OffsetX;
            var newY2 = X2 * matrix.M12 + Y2 * matrix.M22 + Z2 * matrix.M32 + matrix.OffsetY;
            var newZ2 = X2 * matrix.M13 + Y2 * matrix.M22 + Z2 * matrix.M33 + matrix.OffsetZ;

            X1 = newX1;
            Y1 = newY1;
            Z1 = newZ1;

            X2 = newX2;
            Y2 = newY2;
            Z2 = newZ2;
        }

        public void SaveNewCords()
        {
            originalPoint1 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
            originalPoint2 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
        }
        public void ResetOriginalCords()
        {
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

        public Equation(double a, double b,double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Equation(Point p1, Point p2)
        {
            double X1 = p1.X - MainWindow.CanvasMargin;
            double Y1 = p1.Y - MainWindow.CanvasMargin;
            double X2 = p2.X - MainWindow.CanvasMargin;
            double Y2 = p2.Y - MainWindow.CanvasMargin;

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
