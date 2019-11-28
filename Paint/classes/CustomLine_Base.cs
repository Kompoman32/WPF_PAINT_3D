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
    public abstract class CustomLine_Base : Shape
    {
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.X1" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.X1" />.
        /// </returns>
        public static readonly DependencyProperty X1Property = DependencyProperty.Register(nameof(X1), typeof(double), typeof(CustomLine_Base), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.Y1" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.Y1" />.
        /// </returns>
        public static readonly DependencyProperty Y1Property = DependencyProperty.Register(nameof(Y1), typeof(double), typeof(CustomLine_Base), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.X2" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.X2" />.
        /// </returns>
        public static readonly DependencyProperty X2Property = DependencyProperty.Register(nameof(X2), typeof(double), typeof(CustomLine_Base), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));
        /// <summary>
        ///   Идентифицирует свойство зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.Y2" />.
        /// </summary>
        /// <returns>
        ///   Идентификатор для свойства зависимостей <see cref="P:System.Windows.Shapes.CustomLine_Base.Y2" />.
        /// </returns>
        public static readonly DependencyProperty Y2Property = DependencyProperty.Register(nameof(Y2), typeof(double), typeof(CustomLine_Base), (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(IsDoubleFinite));

        /// <summary>
        ///   Возвращает или задает координату начальной точки <see cref="T:System.Windows.Shapes.CustomLine_Base" /> по оси X.
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
                return (double)this.GetValue(CustomLine_Base.X1Property);
            }
            set
            {
                this.SetValue(CustomLine_Base.X1Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        /// <summary>
        ///   Получает или задает координату по оси Y для начальной точки <see cref="T:System.Windows.Shapes.CustomLine_Base" />.
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
                return (double)this.GetValue(CustomLine_Base.Y1Property);
            }
            set
            {
                this.SetValue(CustomLine_Base.Y1Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        /// <summary>
        ///   Получает или задает координату конечной точки <see cref="T:System.Windows.Shapes.CustomLine_Base" /> по оси X.
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
                return (double)this.GetValue(CustomLine_Base.X2Property);
            }
            set
            {
                this.SetValue(CustomLine_Base.X2Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        /// <summary>
        ///   Получает или задает координату конечной точки <see cref="T:System.Windows.Shapes.CustomLine_Base" /> по оси Y.
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
                return (double)this.GetValue(CustomLine_Base.Y2Property);
            }
            set
            {
                this.SetValue(CustomLine_Base.Y2Property, (object)value);
                CacheDefiningGeometry();
            }
        }

        public virtual void CacheDefiningGeometry() { }

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
