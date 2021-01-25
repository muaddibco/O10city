using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using O10.Network.Handlers;
using O10.Network.Interfaces;
using O10.Network.Topology;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Network.Communication
{
    [RegisterExtension(typeof(IServerCommunicationService), Lifetime = LifetimeManagement.Transient)]
    public class TcpCommunicationService : ServerCommunicationServiceBase
    {
        protected Semaphore _maxConnectedClients;
        protected readonly GenericPool<SocketAsyncEventArgs> _acceptEventArgsPool;

        public override string Name => "GenericTcp";

        public TcpCommunicationService(IServiceProvider serviceProvider, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) 
            : base(serviceProvider, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {
            _acceptEventArgsPool = new GenericPool<SocketAsyncEventArgs>(10);

            for (Int32 i = 0; i < 10; i++)
            {
                _acceptEventArgsPool.Push(CreateNewSocketAsyncEventArgs(_acceptEventArgsPool));
            }
        }

        #region Public Functions

        public override void Init(SocketSettings settings)
        {
            _maxConnectedClients = new Semaphore(settings.MaxConnections, settings.MaxConnections);

            base.Init(settings);
        }

        #endregion Public Functions

        #region Private Functions
        protected override Socket CreateSocket()
        {
            return new Socket(_settings.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        protected override void StartAccept()
        {
            _log.Debug($"Started accepting socket");

            _listenSocket.Listen(10);

            SocketAsyncEventArgs acceptEventArg;

            if (_acceptEventArgsPool.Count > 1)
            {
                try
                {
                    acceptEventArg = _acceptEventArgsPool.Pop();
                }
                catch
                {
                    acceptEventArg = CreateNewSocketAsyncEventArgs(_acceptEventArgsPool);
                }
            }
            else
            {
                acceptEventArg = CreateNewSocketAsyncEventArgs(_acceptEventArgsPool);
            }


            _maxConnectedClients.WaitOne();

            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (_communicationProvisioning != null)
            {
                if (!_communicationProvisioning.AllowedEndpoints.Any(ep => ep.Address.Equals(((IPEndPoint)acceptEventArgs.RemoteEndPoint).Address)))
                {
                    HandleBadAccept(acceptEventArgs);
                    return;
                }
            }

            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept();

                HandleBadAccept(acceptEventArgs);
            }
            else
            {
                StartAccept();

                try
                {
                    InitializeCommunicationChannel(acceptEventArgs.AcceptSocket);
                }
                finally
                {
                    acceptEventArgs.AcceptSocket = null;
                    _acceptEventArgsPool.Push(acceptEventArgs);
                }
            }
        }

        protected override void ReleaseClientHandler(ICommunicationChannel communicationChannel)
        {
            base.ReleaseClientHandler(communicationChannel);

            _maxConnectedClients.Release();
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            _log.Debug($"Accept Socket with completed");

            ProcessAccept(e);
        }

        private SocketAsyncEventArgs CreateNewSocketAsyncEventArgs(GenericPool<SocketAsyncEventArgs> pool)
        {
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);

            return acceptEventArg;
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            _log.Debug($"Closing socket due to some error");

            acceptEventArgs.AcceptSocket.Close();

            _acceptEventArgsPool.Push(acceptEventArgs);
        }

        #endregion Private Functions
    }
}
