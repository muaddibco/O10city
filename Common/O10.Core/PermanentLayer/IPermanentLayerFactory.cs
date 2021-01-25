using System;

namespace O10.Core.PermanentLayer
{
    public interface IPermanentLayerFactory
    {
        Type FactoryType { get; }
    }

    public interface IPermanentLayerFactory<T> : IFactory<T>, IPermanentLayerFactory
    {
    }
}
