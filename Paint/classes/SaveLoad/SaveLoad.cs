using Paint.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint.classes
{
    public static class SaveLoad
    {
        public static string Save(string path, IEnumerable<CustomLine> objects, Point cordSystemCenter, bool isCordShown)
        {
            var savedGame = new SavedGame(objects, cordSystemCenter, isCordShown);
            return savedGame.Save(path);
        }

        public static Tuple<List<CustomLine>,Tuple<Point, bool>>  Load(string path)
        {
            var loadedObject = SavedGame.Load(path);

            var objects = loadedObject.Load();

            List<CustomLine> list = new List<CustomLine>();

            foreach(var o in objects)
            {
                if (o is CustomGroup)
                {
                    list.AddRange((o as CustomGroup).GetShapes().Cast<CustomLine>());
                }
                if (o is CustomLine)
                {
                    list.Add((o as CustomLine));
                }
            }

            return new Tuple<List<CustomLine>, Tuple<Point, bool>>(list, new Tuple<Point, bool>(loadedObject.CordSystemCenter, loadedObject.isCordShown));
        }
    }

    [Serializable]
    internal class SavedGame
    {
        public List<ISavedObject> Objects;

        public Point CordSystemCenter;

        public bool isCordShown;


        public SavedGame(IEnumerable<CustomLine> objects, Point cordSystemCenter, bool isCordShown)
        {
            Objects = saveObjects(objects);
            CordSystemCenter = cordSystemCenter;
            this.isCordShown = isCordShown;
        }


        private List<ISavedObject> saveObjects(IEnumerable<CustomLine> lines)
        {
            List<IMyObject> list = new List<IMyObject>();

            foreach (var l in lines)
            {
                IMyObject parent = l;
                while (parent.GetParent() != null)
                {
                    parent = parent.GetParent();
                }

                if (!list.Contains(parent))
                    list.Add(parent);
            }

            List<ISavedObject> savedObjects = new List<ISavedObject>(list.Count);

            foreach(var o in list)
            {
                if (o is CustomGroup)
                {
                    savedObjects.Add(new SavedGroup(o as CustomGroup));
                }
                if (o is CustomLine)
                {
                    savedObjects.Add(new SavedLine(o as CustomLine));
                }
            }

            return savedObjects;
        }

        public string Save(string path)
        {
            var formatter = new BinaryFormatter();
            var w = new StreamWriter(path);
            formatter.Serialize(w.BaseStream, this);
            w.Flush();
            w.Close();
            return path;
        }

        public List<IMyObject> Load()
        {
            List<IMyObject> list = new List<IMyObject>();

            foreach (var o in Objects)
            {
                if(o is SavedGroup)
                {
                    list.Add(((SavedGroup)o).Load());
                }
                if (o is SavedLine)
                {
                    list.Add(((SavedLine)o).Load());
                }
            }

            return list;
        }

        public static SavedGame Load(string path)
        {
            var formatter = new BinaryFormatter();
            var r = new StreamReader(path);
            var obj = formatter.Deserialize(r.BaseStream);
            r.Close();
            return obj as SavedGame;
        }
    }
}
