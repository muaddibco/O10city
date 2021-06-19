using System;
using System.Collections.Generic;

namespace O10.Core
{
    public interface IFactory<T>
    {
        Type FactoryType => typeof(T);

        T Create();
    }

    public interface IFactory<T, Key>
    {
        Type FactoryType => typeof(T);

        T Create(Key key);

        void Utilize(T obj);
    }

    public interface IFactory<T, Key1, Key2>
    {
        Type FactoryType => typeof(T);

        T Create(Key1 key1, Key2 key2);

        void Utilize(T obj);
    }

    public interface IBulkFactory<T>
    {
        Type FactoryType => typeof(T);

        IEnumerable<T> Create();
    }
}
