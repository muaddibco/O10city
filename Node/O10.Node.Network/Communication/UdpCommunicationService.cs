using System.Net.Sockets;
using O10.Network.Handlers;
using O10.Network.Interfaces;
using O10.Network.Topology;
using O10.Core.Architecture;
using O10.Core.Logging;
using System.Net;
using System;

namespace O10.Network.Communication
{
    [RegisterExtension(typeof(IServerCommunicationService), Lifetime = LifetimeManagement.Transient)]
    public class UdpCommunicationService : ServerCommunicationServiceBase
    {
        public UdpCommunicationService(IServiceProvider serviceProvider, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) 
            : base(serviceProvider, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => "GenericUdp";

        protected override void StartAccept()
        {
            InitializeCommunicationChannel(_listenSocket);
        }

        protected override Socket CreateSocket()
        {
            return new Socket(_settings.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

		protected override ICommunicationChannel FindChannel(IPAddress address) => _clientConnectedList.Count > 0 ? _clientConnectedList[0] : null;
	}
}
