using System.Collections.Generic;
using System.Windows.Shapes;

namespace Paint.interfaces
{
    public interface IMyGroup: ISelected
    {
        void Add(IMyObject obj);
        void Remove(IMyObject obj);

        int GetCount();

        List<IMyObject> GetObjects();
        List<Shape> GetShapes();

        void UnGroup();
    }
}
