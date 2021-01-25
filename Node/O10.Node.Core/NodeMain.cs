using O10.Network.Interfaces;
using O10.Network.Communication;
using System;
using System.Net;
using O10.Core.Configuration;
using System.Threading;
using O10.Node.Core.Configuration;
using O10.Core.Logging;
using O10.Core.Modularity;
using O10.Network.Configuration;
using O10.Network.Handlers;

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
        private readonly IModulesRepository _modulesRepository;
        private readonly IPacketsHandler _packetsHandler;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public NodeMain(IServerCommunicationServicesRepository communicationServicesFactory,
                        IServerCommunicationServicesRegistry communicationServicesRegistry,
                        IConfigurationService configurationService,
                        IModulesRepository modulesRepository,
                        IPacketsHandler packetsHandler,
                        ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _communicationServicesFactory = communicationServicesFactory;
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
            _modulesRepository = modulesRepository;
            _packetsHandler = packetsHandler;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize(CancellationToken ct)
        {
            InitializeCommunicationLayer();

            ObtainConfiguredModules();

            InitializeModules(ct);
        }

        private void InitializeCommunicationLayer()
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();

            foreach (string communicationServiceName in nodeConfiguration.CommunicationServices)
            {
                CommunicationConfigurationBase communicationConfiguration = (CommunicationConfigurationBase)_configurationService[communicationServiceName];
                IServerCommunicationService serverCommunicationService = _communicationServicesFactory.GetInstance(communicationConfiguration.CommunicationServiceName);
                serverCommunicationService.Init(new SocketListenerSettings(communicationConfiguration.MaxConnections, communicationConfiguration.ReceiveBufferSize, new IPEndPoint(IPAddress.Any, communicationConfiguration.ListeningPort)));
                _communicationServicesRegistry.RegisterInstance(serverCommunicationService, communicationConfiguration.CommunicationServiceName);
            }

            _packetsHandler.Initialize(_cancellationTokenSource.Token);
        }

        private void InitializeModules(CancellationToken ct)
        {
            foreach (IModule module in _modulesRepository.GetBulkInstances())
            {
                try
                {
                    module.Initialize(ct);
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize Module '{module.Name}'", ex);
                }
            }
        }

        private void ObtainConfiguredModules()
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();

            string[] moduleNames = nodeConfiguration.Modules;
            if (moduleNames != null)
            {
                foreach (string moduleName in moduleNames)
                {
                    try
                    {
                        IModule module = _modulesRepository.GetInstance(moduleName);
                        _modulesRepository.RegisterInstance(module);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Failed to register Module with name '{moduleName}'.", ex);
                    }
                }
            }
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

            foreach (IModule module in _modulesRepository.GetBulkInstances())
            {
                module.StartModule();
            }
        }
    }
}
