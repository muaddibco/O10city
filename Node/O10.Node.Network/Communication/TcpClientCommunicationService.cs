using System.Net.Sockets;
using O10.Network.Handlers;
using O10.Network.Interfaces;
using O10.Network.Topology;
using O10.Core.Architecture;
using O10.Core.Logging;
using System;

namespace O10.Network.Communication
{

    [RegisterExtension(typeof(ICommunicationService), Lifetime = LifetimeManagement.Singleton)]
    public class TcpClientCommunicationService : CommunicationServiceBase
    {

        public TcpClientCommunicationService(IServiceProvider serviceProvider, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService)
            : base(serviceProvider, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {

        }

        public override string Name => nameof(TcpClientCommunicationService);

        protected override Socket CreateSocket()
        {
            return new Socket(_settings.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
