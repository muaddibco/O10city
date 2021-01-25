using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using O10.Core.Architecture;

using O10.Core.Exceptions;

namespace O10.Core.PermanentLayer
{
    [RegisterDefaultImplementation(typeof(IPermanentLayerServiceFactory), Lifetime = LifetimeManagement.Singleton)]
    public class PermanentLayerServiceFactory : IPermanentLayerServiceFactory
    {
        private ReadOnlyDictionary<Type, IPermanentLayerFactory> _permanentLayerFactories;

        public PermanentLayerServiceFactory(IEnumerable<IPermanentLayerFactory> permanentLayerFactories)
        {
            _permanentLayerFactories = new ReadOnlyDictionary<Type, IPermanentLayerFactory>(permanentLayerFactories.ToDictionary(s =>s.FactoryType, s => s));
        }

        public T GetService<T>()
        {
            if(_permanentLayerFactories.ContainsKey(typeof(T)))
            {
                if(_permanentLayerFactories[typeof(T)] is IPermanentLayerFactory<T> permanentLayerFactory)
                {
                    return permanentLayerFactory.Create();
                }

                throw new InvalidCastException($"Failed to cast permanent layer factory to the provided type {typeof(T).FullName}");
            }

            throw new FactoryTypeResolutionFailureException(typeof(T));
        }
    }
}
