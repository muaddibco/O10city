using System;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Core.ExtensionMethods
{
	public static class ServiceProviderExtensions
	{
		public static async Task<T> UseBootstrapper<T>(this IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger? logger = null) where T : Bootstrapper
		{
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            T bootstrapper = (T)serviceProvider.GetService(typeof(T));
			await bootstrapper.RunInitializers(serviceProvider, cancellationToken, logger).ConfigureAwait(false);

            return bootstrapper;
		}
	}
}
