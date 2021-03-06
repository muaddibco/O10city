using System.Net;
using O10.Network.Handlers;
using O10.Network.Interfaces;
using O10.Network.Topology;
using O10.Core.Architecture;
using O10.Core.Logging;
using System;

namespace O10.Network.Communication
{
    /// <summary>
    /// Seems such type of Communication Service will be used at Storage Level only
    /// </summary>
    [RegisterExtension(typeof(IServerCommunicationService), Lifetime = LifetimeManagement.Transient)]
    public class TcpIntermittentCommunicationService : TcpCommunicationService
    {

        public TcpIntermittentCommunicationService(IServiceProvider serviceProvider, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) 
            : base(serviceProvider, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => nameof(TcpIntermittentCommunicationService);

        public override void Init(SocketSettings settings)
        {
            RegisterOnReceivedExtendedValidation(OnCommunicationChannelReceived);

            base.Init(settings);
        }

        private bool OnCommunicationChannelReceived(ICommunicationChannel communicationChannel, IPEndPoint remoteEndPoint, int receivedBytes)
        {
            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receivedBytes == 0)
            {
                _log.Info($"ProcessReceive NO DATA from IP {communicationChannel.RemoteIPAddress}");

                communicationChannel.Close();

                return false;
            }

            return true;
        }
    }
}