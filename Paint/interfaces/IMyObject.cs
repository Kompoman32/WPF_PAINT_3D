using System;
using System.Windows;
using System.Windows.Media;

namespace Paint.interfaces
{
    public interface IMyObject: ISelected
    {
        void CacheDefiningGeometry();

        void InvalidateVisual();

        void Move(double deltaX, double deltaY);

        void MoveTo(double x1, double x2, double y1, double y2);
        void MoveTo(Point positionDot1,Point positionDot2);

        IMyObject GetParent();
        void SetParent(IMyObject parent);

        void SetStroke(Brush brush);

        Brush GetStroke();

        Tuple<Point, Point> GetSizeShape();
    }
}
