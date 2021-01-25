using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using O10.Core.Architecture;

using O10.Core.Logging;

namespace O10.Core.ExtensionMethods
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBootstrapper<T>(this IServiceCollection container, ILogger logger = null, RunMode runMode = RunMode.Default) where T : Bootstrapper, new()
		{
			T bootstrapper = new T();
			bootstrapper.Run(container, logger, runMode);
			container.AddSingletonOnce(bootstrapper);
			return container;
		}

		public static IServiceCollection AddTransientOnce(this IServiceCollection services, Type serviceType, Type implementationType)
		{
			if(services.Any(d => d.ServiceType == serviceType && d.ImplementationType == implementationType))
			{
				return services;
			}

			return services.AddTransient(serviceType, implementationType);
		}

		public static IServiceCollection AddSingletonOnce<TService>(this IServiceCollection services, TService implementationInstance) where TService: class
		{
			ServiceDescriptor serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == implementationInstance.GetType());
			if (serviceDescriptor != null)
			{
				services.Remove(serviceDescriptor);
			}

			return services.AddSingleton(implementationInstance);
		}

		public static IServiceCollection AddSingletonOnce(this IServiceCollection services, Type serviceType, Type implementationType)
		{
			if (services.Any(d => d.ServiceType == serviceType && d.ImplementationType == implementationType))
			{
				return services;
			}

			return services.AddSingleton(serviceType, implementationType);
		}

		public static IServiceCollection AddSingletonOnce(this IServiceCollection services, Type serviceType, object implementationInstance)
		{
			if (services.Any(d => d.ServiceType == serviceType && d.ImplementationInstance == implementationInstance))
			{
				return services;
			}

			return services.AddSingleton(serviceType, implementationInstance);
		}

		public static IServiceCollection AddScopedOnce(this IServiceCollection services, Type serviceType, Type implementationType)
		{
			if (services.Any(d => d.ServiceType == serviceType && d.ImplementationType == implementationType))
			{
				return services;
			}

			return services.AddScoped(serviceType, implementationType);
		}
	}
}
