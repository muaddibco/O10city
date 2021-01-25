using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Core.Models;
using O10.Core.Logging;
using O10.Gateway.Common;
using O10.Gateway.Common.Services;
using O10.Gateway.WebApp.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using O10.Transactions.Core.Accessors;

namespace O10.Server.Gateway
{
    public class GatewayCommonBootstrapper : GatewayBootstrapperBase
	{
		public GatewayCommonBootstrapper() : base()
		{
		}

		protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
		{
			return base.EnumerateCatalogItems(rootFolder).Union(new string[] { "O10.Gateway.WebApp.Common.dll" });
		}

        public override void RunInitializers(IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger logger)
        {
            base.RunInitializers(serviceProvider, cancellationToken, logger);

            SetPipes(serviceProvider, cancellationToken);
        }

        private void SetPipes(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            INetworkSynchronizer networkSynchronizer = serviceProvider.GetService<INetworkSynchronizer>();

            INotificationsHubService notificationsHubService = serviceProvider.GetService<INotificationsHubService>();
            notificationsHubService.Initialize(cancellationToken);

            ITransactionsHandler transactionsHandler = serviceProvider.GetService<ITransactionsHandler>();
            IEvidencesHandler evidencesHandler = serviceProvider.GetService<IEvidencesHandler>();
            
            transactionsHandler.GetSourcePipe<EvidenceDescriptor>().LinkTo(evidencesHandler.GetTargetPipe<EvidenceDescriptor>());

            transactionsHandler.GetSourcePipe<PacketBase>().LinkTo(networkSynchronizer.PipeIn);
            evidencesHandler.GetSourcePipe<PacketBase>().LinkTo(networkSynchronizer.PipeIn);

            networkSynchronizer.PipeOut = new TransformBlock<WitnessPackage, WitnessPackage>(w => w);
            networkSynchronizer.PipeOut.LinkTo(notificationsHubService.PipeIn);

            networkSynchronizer.Initialize(cancellationToken);
            networkSynchronizer.Start();
        }
    }
}
