using System;
using System.Threading;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Core.ExtensionMethods
{
	public static class ServiceProviderExtensions
	{
		public static T UseBootstrapper<T>(this IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger logger = null) where T : Bootstrapper
		{
			T bootstrapper = (T)serviceProvider.GetService(typeof(T));
			bootstrapper.RunInitializers(serviceProvider, cancellationToken, logger);

            return bootstrapper;
		}
	}
}
