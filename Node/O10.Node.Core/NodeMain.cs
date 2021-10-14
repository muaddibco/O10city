using O10.Network.Interfaces;
using O10.Network.Communication;
using System.Net;
using O10.Core.Configuration;
using System.Threading;
using O10.Core.Logging;
using O10.Network.Configuration;
using O10.Network.Handlers;
using O10.Node.Network.Configuration;

namespace O10.Node.Core
{
    /// <summary>
    /// Main class with business logic of Node.
    /// 
    /// Process of start-up:
    ///  1. Initialize - it creates, initializes and launches listeners of other nodes and wallet accounts
    ///  2. EnterGroup - before Node can start to function it must to connect to some consensus group of Nodes for consensus decisions accepting
    ///  3. Start - after Node entered to any consensus group it starts to work
    /// </summary>
    public class NodeMain
    {
        private readonly ILogger _log;
        private readonly IServerCommunicationServicesRepository _communicationServicesFactory;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IConfigurationService _configurationService;
        private readonly IPacketsHandler _packetsHandler;

        public NodeMain(IServerCommunicationServicesRepository communicationServicesFactory,
                        IServerCommunicationServicesRegistry communicationServicesRegistry,
                        IConfigurationService configurationService,
                        IPacketsHandler packetsHandler,
                        ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _communicationServicesFactory = communicationServicesFactory;
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
            _packetsHandler = packetsHandler;
        }

        public void Initialize(CancellationToken ct)
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();

            foreach (string communicationServiceName in nodeConfiguration.CommunicationServices)
            {
                CommunicationConfigurationBase communicationConfiguration = (CommunicationConfigurationBase)_configurationService[communicationServiceName];
                IServerCommunicationService serverCommunicationService = _communicationServicesFactory.GetInstance(communicationConfiguration.CommunicationServiceName);
                serverCommunicationService.Init(new SocketListenerSettings(communicationConfiguration.MaxConnections, communicationConfiguration.ReceiveBufferSize, new IPEndPoint(IPAddress.Any, communicationConfiguration.ListeningPort)));
                _communicationServicesRegistry.RegisterInstance(serverCommunicationService, communicationConfiguration.CommunicationServiceName);
            }

            _packetsHandler.Initialize(ct);
        }

        internal void Start()
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();
            foreach (string communicationServiceName in nodeConfiguration.CommunicationServices)
            {
                CommunicationConfigurationBase communicationConfiguration = (CommunicationConfigurationBase)_configurationService[communicationServiceName];
                _communicationServicesRegistry.GetInstance(communicationConfiguration.CommunicationServiceName).Start();
            }

            _packetsHandler.Start();
        }
    }
}
