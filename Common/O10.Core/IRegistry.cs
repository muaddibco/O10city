using System.Collections.Generic;

namespace O10.Core
{
    public interface IRegistry<T>
    {
        void RegisterInstance(T obj);

        T GetInstance();
    }

    public interface IRegistry<T, TKey>
    {
        void RegisterInstance(T obj, TKey key);

        T GetInstance(TKey key);
    }

    public interface IBulkRegistry<T>
    {
        void RegisterInstance(T obj);

        IEnumerable<T> GetBulkInstances();
    }

    public interface IBulkRegistry<T, TKey>
    {
        void RegisterInstance(T obj);

        IEnumerable<T> GetBulkInstances(TKey key);
    }
}
