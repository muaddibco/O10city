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
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks;

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

        public override async Task RunInitializers(IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger logger)
        {
            await base.RunInitializers(serviceProvider, cancellationToken, logger).ConfigureAwait(false);

            SetPipes(serviceProvider, cancellationToken);
        }

        private void SetPipes(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            INetworkSynchronizer networkSynchronizer = serviceProvider.GetService<INetworkSynchronizer>();

            INotificationsHubService notificationsHubService = serviceProvider.GetService<INotificationsHubService>();
            notificationsHubService.Initialize(cancellationToken);

            ITransactionsHandler transactionsHandler = serviceProvider.GetService<ITransactionsHandler>();
            IEvidencesHandler evidencesHandler = serviceProvider.GetService<IEvidencesHandler>();
            
            transactionsHandler.GetSourcePipe<DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>>().LinkTo(evidencesHandler.GetTargetPipe<DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>>());

            transactionsHandler.GetSourcePipe<TaskCompletionWrapper<IPacketBase>>().LinkTo(new ActionBlock<TaskCompletionWrapper<IPacketBase>>(w => networkSynchronizer.PipeIn.SendAsync(w)));
            evidencesHandler.GetSourcePipe<TaskCompletionWrapper<IPacketBase>>().LinkTo(new ActionBlock<TaskCompletionWrapper<IPacketBase>>(w => networkSynchronizer.PipeIn.SendAsync(w)));

            networkSynchronizer.PipeOut = new TransformBlock<WitnessPackage, WitnessPackage>(w => w);
            networkSynchronizer.PipeOut.LinkTo(notificationsHubService.PipeIn);

            networkSynchronizer.Initialize(cancellationToken);
            networkSynchronizer.Start();
        }
    }
}
