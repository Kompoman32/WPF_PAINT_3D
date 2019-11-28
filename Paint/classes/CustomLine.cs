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

namespace Paint
{
    public class CustomLine : CustomLine_Base, IMyObject
    {
        public CustomLine() : base()
        {
            CacheDefiningGeometry();
        }


        public bool IsPressed_Point_1 = false;
        public bool IsPressed_Point_2 = false;
        public bool IsNotEntered = true;
        public bool isSelected = false;

        Brush brush;
        Brush selectedBrush;

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
            var line = new LineGeometry(new Point(this.X1, this.Y1), new Point(this.X2, this.Y2));

            if (isSelected)
            {
                var point1 = new EllipseGeometry(new Point(X1, Y1), this.StrokeThickness, this.StrokeThickness);
                var point2 = new EllipseGeometry(new Point(X2, Y2), this.StrokeThickness, this.StrokeThickness);

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
            var x1 = this.X1;
            var x2 = this.X2;
            var y1 = this.Y1;
            var y2 = this.Y2;

            this.MoveTo(this.X1 + deltaX, this.X2 + deltaX, this.Y1 + deltaY, this.Y2 + deltaY);
        }
        public void MoveTo(double x1, double x2, double y1, double y2)
        {
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
        }
        public void MoveTo(Point pos1, Point pos2)
        {
            this.MoveTo(pos1.X, pos1.Y, pos2.X, pos2.Y);
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
            this.InvalidateVisual();
        }

        public Tuple<Point, Point> GetSizeShape()
        {
            var minX = Math.Min(X1, X2);
            var minY = Math.Min(Y1, Y2);
            var maxX = Math.Max(X1, X2);
            var maxY = Math.Max(Y1, Y2);

            return new Tuple<Point, Point>(new Point(minX, minY), new Point(maxX, maxY));
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

            return $"{A:###0.0#}x {b}y {c}";
        }
    }
}
