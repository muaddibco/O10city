using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Logging;
using System.Collections.Generic;
using O10.Core.Tracking;
using O10.Transactions.Core.Ledgers;
using System;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Modularity;
using O10.Core.Configuration;
using O10.Node.Network.Configuration;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    internal class PacketsHandler : IPacketsHandler
    {
        private readonly ILogger _log;
        private readonly BlockingCollection<IPacketBase> _packets;
        private readonly List<IServiceScope> _handlingFlowScopes;
        private readonly int _maxDegreeOfParallelism;
        private readonly ITrackingService _trackingService;
        private readonly INodeConfiguration _nodeConfiguration;
        private CancellationToken _cancellationToken;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IServiceProvider serviceProvider,
                              ITrackingService trackingService,
                              IConfigurationService configurationService,
                              ILoggerService loggerService)
        {
            _trackingService = trackingService;
            _log = loggerService.GetLogger(GetType().Name);
            _packets = new BlockingCollection<IPacketBase>();
            _nodeConfiguration = configurationService.Get<INodeConfiguration>();

            _maxDegreeOfParallelism = 4;

            _handlingFlowScopes = new List<IServiceScope>(_maxDegreeOfParallelism);

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                _handlingFlowScopes[i] = serviceProvider.CreateScope();
            }
        }

        public void Initialize(CancellationToken ct)
        {
            if (!IsInitialized)
            {
                _cancellationToken = ct;

                _handlingFlowScopes.ForEach(s =>
                {
                    InitModules(s, ct);
                });

                IsInitialized = true;
            }
        }

        private void InitModules(IServiceScope s, CancellationToken ct)
        {
            var modulesRepo = s.ServiceProvider.GetRequiredService<IModulesRepository>();
            ObtainConfiguredModules(modulesRepo);
            foreach (IModule module in modulesRepo.GetBulkInstances())
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

        private void ObtainConfiguredModules(IModulesRepository modulesRepository)
        {
            string[] moduleNames = _nodeConfiguration.Modules;
            if (moduleNames != null)
            {
                foreach (string moduleName in moduleNames)
                {
                    try
                    {
                        IModule module = modulesRepository.GetInstance(moduleName);
                        modulesRepository.RegisterInstance(module);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Failed to register Module with name '{moduleName}'.", ex);
                    }
                }
            }
        }

        public void Push(IPacketBase packet)
        {
            _trackingService.TrackMetric("PushedForHandlingPacketsThroughput", 1);

            _log.Debug(() => $"Pushed packer for handling: {packet.GetType().Name}");

            _packets.Add(packet);
            _trackingService.TrackMetric("PacketsQueueSize", 1);
        }

        public void Start()
        {
            _log.Debug(() => "PacketsHandler starting");

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                tasks.Add(Task.Factory.StartNew(o => Process((int)o), i, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current));
            }

            _log.Debug(() => "PacketsHandler started");
        }

        private void Process(int iteration)
        {
            _log.Debug(() => $"Process function #{iteration} starting");

            try
            {
                _trackingService.TrackMetric("ParallelProcesses", 1);

                StartModules(iteration);

                StartHandlingFlow(iteration);
            }
            finally
            {
                _log.Debug(() => "Process function finished");
                _trackingService.TrackMetric("ParallelProcesses", -1);
            }
        }

        private void StartHandlingFlow(int iteration)
        {
            var handlingFlow = ActivatorUtilities.CreateInstance<PacketHandlingFlow>(_handlingFlowScopes[iteration].ServiceProvider, iteration);

            foreach (var packet in _packets.GetConsumingEnumerable(_cancellationToken))
            {
                _log.Debug(() => $"Picked for handling flow #{iteration} packet {packet.GetType().Name}");

                _trackingService.TrackMetric("MessagesQueueSize", -1);
                handlingFlow.PostPacket(packet);
            }
        }

        private void StartModules(int iteration)
        {
            var modulesRepo = _handlingFlowScopes[iteration].ServiceProvider.GetRequiredService<IModulesRepository>();
            foreach (IModule module in modulesRepo.GetBulkInstances())
            {
                module.StartModule();
            }
        }
    }
}
