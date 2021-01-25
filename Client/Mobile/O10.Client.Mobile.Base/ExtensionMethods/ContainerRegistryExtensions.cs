using Prism.Ioc;
using System;

namespace O10.Client.Mobile.Base.ExtensionMethods
{
    public static class ContainerRegistryExtensions
    {
        public static IContainerRegistry RegisterOnce<T>(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry.IsRegistered<T>())
            {
                return containerRegistry;
            }

            return containerRegistry.Register<T>();
        }

        public static IContainerRegistry RegisterOnce<T>(this IContainerRegistry containerRegistry, Type type)
        {
            if (containerRegistry.IsRegistered<T>())
            {
                return containerRegistry;
            }

            return containerRegistry.Register(typeof(T), type);
        }

        public static IContainerRegistry RegisterOnce(this IContainerRegistry containerRegistry, Type type)
        {
            if (containerRegistry.IsRegistered(type))
            {
                return containerRegistry;
            }

            return containerRegistry.Register(type);
        }
    }
}
