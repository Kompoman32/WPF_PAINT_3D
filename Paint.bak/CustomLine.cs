using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Runtime.InteropServices;

namespace Paint
{
    class CustomLine : Shape
    {
        public CustomLine()
        {
            CacheDefiningGeometry();
        }

        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine.X1" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine.X1" />.
        /// </returns>
        public static readonly DependencyProperty X1Property = DependencyProperty.Register(nameof(X1), typeof(double), typeof(CustomLine), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine.Y1" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine.Y1" />.
        /// </returns>
        public static readonly DependencyProperty Y1Property = DependencyProperty.Register(nameof(Y1), typeof(double), typeof(CustomLine), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine.X2" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine.X2" />.
        /// </returns>
        public static readonly DependencyProperty X2Property = DependencyProperty.Register(nameof(X2), typeof(double), typeof(CustomLine), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine.Y2" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine.Y2" />.
        /// </returns>
        public static readonly DependencyProperty Y2Property = DependencyProperty.Register(nameof(Y2), typeof(double), typeof(CustomLine), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        private Geometry _Geometry;
        private Brush _brush;

        public bool IsSelected = false;
        public bool IsPressed_Point_1 = false;
        public bool IsPressed_Point_2 = false;
        public bool IsNotEntered = true;

        /// <summary>
        ///   Возвращает или задает координату начальной точки <see cref="T:System.Windows.Shapes.CustomLine" /> по оси X.
        /// </summary>
        /// <returns>
        ///   Координата начальной точки линии по оси X.
        ///    Значение по умолчанию — 0.
        /// </returns>
        [TypeConverter(typeof(LengthConverter))]
        public double X1
        {
            get
            {
                return (double)this.GetValue(CustomLine.X1Property);
            }
            set
            {
                this.SetValue(CustomLine.X1Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        /// <summary>
        ///   Получает или задает координату по оси Y для начальной точки <see cref="T:System.Windows.Shapes.CustomLine" />.
        /// </summary>
        /// <returns>
        ///   Координата по оси Y для начальной точки линии.
        ///    Значение по умолчанию — 0.
        /// </returns>
        [TypeConverter(typeof(LengthConverter))]
        public double Y1
        {
            get
            {
                return (double)this.GetValue(CustomLine.Y1Property);
            }
            set
            {
                this.SetValue(CustomLine.Y1Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        /// <summary>
        ///   Получает или задает координату конечной точки <see cref="T:System.Windows.Shapes.CustomLine" /> по оси X.
        /// </summary>
        /// <returns>
        ///   Координата конечной точки линии по оси X.
        ///    Значение по умолчанию — 0.
        /// </returns>
        [TypeConverter(typeof(LengthConverter))]
        public double X2
        {
            get
            {
                return (double)this.GetValue(CustomLine.X2Property);
            }
            set
            {
                this.SetValue(CustomLine.X2Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        /// <summary>
        ///   Получает или задает координату конечной точки <see cref="T:System.Windows.Shapes.CustomLine" /> по оси Y.
        /// </summary>
        /// <returns>
        ///   Координата конечной точки линии по оси Y.
        ///    Значение по умолчанию — 0.
        /// </returns>
        [TypeConverter(typeof(LengthConverter))]
        public double Y2
        {
            get
            {
                return (double)this.GetValue(CustomLine.Y2Property);
            }
            set
            {
                this.SetValue(CustomLine.Y2Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                return this._Geometry;
            }
        }

        public void CacheDefiningGeometry()
        {
            var line = new LineGeometry(new Point(this.X1, this.Y1), new Point(this.X2, this.Y2));

            if (IsSelected)
            {
                var color = GetInvertedColor(((SolidColorBrush)this.Stroke).Color);
                var brush = new SolidColorBrush(color);

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

        }

        public Color GetInvertedColor(Color c)
        {
            var r = (byte)(255 - c.R);
            var g = (byte)(255 - c.G);
            var b = (byte)(255 - c.B);

            if (r > 245 && g > 245 && b > 245)
            {
                r -= 10;
                g -= 10;
                b -= 10;
            }

            c.R = r;
            c.G = g;
            c.B = b;

            return c;
        }

        internal static bool IsDoubleFinite(object o)
        {
            double d = (double)o;
            if (!double.IsInfinity(d))
                return !IsNaN(d);
            return false;
        }

        public static bool IsNaN(double value)
        {
            NanUnion nanUnion = new NanUnion();
            nanUnion.DoubleValue = value;
            ulong num1 = nanUnion.UintValue & 18442240474082181120UL;
            ulong num2 = nanUnion.UintValue & 4503599627370495UL;
            if (num1 == 9218868437227405312UL || num1 == 18442240474082181120UL)
                return num2 > 0UL;
            return false;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            [FieldOffset(0)]
            internal double DoubleValue;
            [FieldOffset(0)]
            internal ulong UintValue;
        }

        public new void InvalidateVisual()
        {
            CacheDefiningGeometry();
            base.InvalidateVisual();
        }
    }
}
