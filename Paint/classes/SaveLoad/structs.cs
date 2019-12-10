using Paint.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Paint.classes
{
    public  interface ISavedObject
    {
        //ISavedObject Save(IMyObject obj);
    }

    [Serializable]
    public struct SavedColor : ILoaded<Color>
    {
        byte A;

        byte R;
        byte G;
        byte B;

        public SavedColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;

            A = a;
        }

        public Color Load()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }

    [Serializable]
    public  struct SavedLine: ISavedObject, ILoaded<CustomLine>
    {
        double X1;
        double Y1;
        double Z1;

        double X2;
        double Y2;
        double Z2;

        double StrokeThickness;

        bool isSelected;

        SavedColor brushColor;

        public SavedLine(CustomLine line)
        {
            X1 = line.X1;
            Y1 = line.Y1;
            Z1 = line.Z1;
            X2 = line.X2;
            Y2 = line.Y2;
            Z2 = line.Z2;

            this.StrokeThickness = line.StrokeThickness;

            this.isSelected = line.IsSelected();

            var brushColor = (line.OriginalBrush as SolidColorBrush).Color;
            this.brushColor = new SavedColor(brushColor.R, brushColor.G, brushColor.B, brushColor.A);
        }

        public CustomLine Load()
        {
            var line  = new CustomLine(new SolidColorBrush(brushColor.Load()))
            {
                X1 = X1,
                Y1 = Y1,
                Z1 = Z1,
                X2 = X2,
                Y2 = Y2,
                Z2 = Z2,
                StrokeThickness = StrokeThickness,
                originalPoint1 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue),
                originalPoint2 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue)
            };

            if (isSelected)
            {
                line.Select();
            }

            return line;
        }
    }

    [Serializable]
    public struct SavedGroup : ISavedObject, ILoaded<CustomGroup>
    {
        List<ISavedObject> objects;
        
        bool isSelected;

        public SavedGroup(CustomGroup group)
        {
            List<ISavedObject> list = new List<ISavedObject>();

            foreach (var o in group.GetObjects())
            {
                if (o is CustomGroup)
                {
                    var savedGroup = new SavedGroup(o as CustomGroup);

                    list.Add(savedGroup);
                }

                if (o is CustomLine)
                {
                    var savedLine = new SavedLine(o as CustomLine);

                    list.Add(savedLine);
                }
            }

            objects = list;

            this.isSelected = group.IsSelected();
        }

        public CustomGroup Load()
        {
            List<IMyObject> list = new List<IMyObject>();
            foreach(var o in objects)
            {
                if (o is SavedGroup)
                {
                    list.Add((o as ILoaded<CustomGroup>).Load());
                }
                if (o is SavedLine)
                {
                    list.Add((o as ILoaded<CustomLine>).Load());
                }
            }

            var group = new CustomGroup(list);
            if (this.isSelected)
            {
                group.Select();
            }
            return group;
        }
    }
}
