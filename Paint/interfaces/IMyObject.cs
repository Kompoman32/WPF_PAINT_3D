using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Paint.interfaces
{

    public interface IMyObject: ISelected
    {
        void CacheDefiningGeometry();

        void InvalidateVisual();

        void Move(double deltaX, double deltaY);
        void Move(double deltaX, double deltaY, double deltaZ);
        void MoveTo(double x1, double x2, double y1, double y2);
        void MoveTo(double x1, double x2, double y1, double y2, double z1, double z2);
        void MoveTo(Point positionDot1, Point positionDot2);
        void MoveTo(Point3D positionDot1, Point3D positionDot2);

        IMyObject GetParent();
        void SetParent(IMyObject parent);

        void SetStroke(Brush brush);

        Brush GetStroke();

        Tuple<Point3D, Point3D> GetSizeShape();

        void ConvertByMatrix(Matrix3D matrix);
        void SaveNewCords();
        void ResetOriginalCords();
    }
}
