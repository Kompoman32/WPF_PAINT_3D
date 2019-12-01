using Paint.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Paint.classes
{
    class CustomGroup : IMyObject, IMyGroup
    {
        List<IMyObject> objects;

        IMyObject parentObj;

        bool isSelected;
        public CustomGroup()
        {
            objects = new List<IMyObject>();
        }

        public CustomGroup(List<IMyObject> objects)
        {
            this.objects = new List<IMyObject>(objects);

            foreach(var o in objects)
            {
                o.SetParent(this);
            }
        }

        ~CustomGroup()
        {
            UnGroup();
        }

        // IMyGroup

        public void Add(IMyObject obj)
        {
            objects.Add(obj);
            obj.SetParent(this);
        }

        public void Remove(IMyObject obj)
        {
            objects.Remove(obj);
            obj.SetParent(null);
        }

        public int GetCount()
        {
            var count = 0;

            foreach (var o in objects)
            {
                if (o is IMyGroup)
                {
                    count += (o as IMyGroup).GetCount();
                }
                else
                {
                    count++;
                }
            }

            return count;
        }

        public List<IMyObject> GetObjects()
        {
            return objects;
        }

        public List<Shape> GetShapes()
        {
            List<Shape> list = new List<Shape>(this.GetCount());
            foreach (var o in objects)
            {
                if (o is IMyGroup)
                {
                    list.AddRange((o as IMyGroup).GetShapes());
                } else
                {
                    list.Add(o as Shape);
                }
            }

            return list;
        }

        public void UnGroup()
        {
            foreach (var o in objects.ToArray())
            {
                o.SetParent(null);
            }

            objects.Clear();
        }

        // IMyObject

        public void CacheDefiningGeometry()
        {
            foreach (var o in objects)
            {
                o.CacheDefiningGeometry();
            }


        }
        public void InvalidateVisual()
        {
            foreach (var o in objects)
            {
                o.InvalidateVisual();
            }
        }

        public void SetStroke(Brush brush)
        {
            foreach(var o in objects)
            {
                o.SetStroke(brush.Clone());
            }
        }
        public Brush GetStroke()
        {
            var canGetStroke = true;


            if (objects.Count == 0)
            {
                return null;
            }

            var color = ((SolidColorBrush)objects.First().GetStroke()).Color;


            foreach (var o in objects)
            {
                if (!canGetStroke) break;
                canGetStroke &= ((SolidColorBrush)o.GetStroke()).Color.Equals(color);
            }

            return canGetStroke ? new SolidColorBrush(color) : null;
        }

        public void Move(double deltaX, double deltaY)
        {
            foreach (var o in objects)
            {
                o.Move(deltaX, deltaY);
            }
        }
        public void Move(double deltaX, double deltaY, double deltaZ)
        {
            foreach (var o in objects)
            {
                o.Move(deltaX, deltaY, deltaZ);
            }
        }
        public void MoveTo(double x1, double x2, double y1, double y2)
        {
            foreach (var o in objects)
            {
                o.MoveTo(x1, x2, y1, y2);
            }
        }
        public void MoveTo(double x1, double x2, double y1, double y2, double z1, double z2)
        {
            foreach (var o in objects)
            {
                o.MoveTo(x1, x2, y1, y2, z1, z2);
            }
        }
        public void MoveTo(Point pos1, Point pos2)
        {
            this.MoveTo(pos1.X, pos1.Y, pos2.X, pos2.Y);
        }
        public void MoveTo(Point3D pos1, Point3D pos2)
        {
            this.MoveTo(pos1.X, pos1.Y, pos2.X, pos2.Y, pos1.Z, pos2.Z);
        }

        public void SetParent(IMyObject parent)
        {
            var existedParent = this.GetParent();
            this.parentObj = parent;
            if (existedParent != null && existedParent is IMyGroup)
            {
                (existedParent as IMyGroup).Remove(this);
            }
        }

        public IMyObject GetParent()
        {
            return this.parentObj;
        }

        public bool IsSelected()
        {
            return isSelected;
        }
        public void Select()
        {
            foreach (var o in objects)
            {
                o.Select();
            }

            
            InvalidateVisual();
            isSelected = true;
        }
        public void UnSelect()
        {
            foreach (var o in objects)
            {
                o.UnSelect();
                o.SaveNewCords();
            }

            InvalidateVisual();
            isSelected = false;
        }

        public Tuple<Point3D, Point3D> GetSizeShape()
        {
            return GetSizeShapeByArray(objects);
        }
        public static Tuple<Point3D, Point3D> GetSizeShapeByArray(List<IMyObject> objects)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;
            var minZ = double.MaxValue;
            var maxZ = double.MinValue;

            var arrays = new List<Point3D>(objects.Count * 2);
            var _ = objects.Select(x =>
            {
                if (x == null) return false;

                Point3D point1;
                Point3D point2;
                x.GetSizeShape().Deconstruct(out point1, out point2);
                arrays.Add(point1);
                arrays.Add(point2);
                return true;
            }).ToList();

            if (arrays.Count == 0) return null;

            minX = arrays.Min(x => x.X);
            maxX = arrays.Max(x => x.X);
            minY = arrays.Min(x => x.Y);
            maxY = arrays.Max(x => x.Y);
            minZ = arrays.Min(x => x.Z);
            maxZ = arrays.Max(x => x.Z);

            return new Tuple<Point3D, Point3D>(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
        }

        public void ConvertByMatrix(Matrix3D matrix)
        {
            foreach(var o in objects)
            {
                o.ConvertByMatrix(matrix);
            }
        }
        public void SaveNewCords()
        {
            foreach(var o in objects)
            {
                o.SaveNewCords();
            }
        }
        public void ResetOriginalCords()
        {
            foreach (var o in objects)
            {
                o.ResetOriginalCords();
            }
        }
    }
}
