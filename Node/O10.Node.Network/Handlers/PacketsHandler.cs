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
using O10.Core.ExtensionMethods;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    internal class PacketsHandler : IPacketsHandler, IDisposable
    {
        private readonly ILogger _log;
        private readonly BlockingCollection<IPacketBase> _packets;
        private readonly List<IServiceScope> _handlingFlowScopes;
        private readonly int _maxDegreeOfParallelism;
        private readonly ITrackingService _trackingService;
        private readonly INodeConfiguration _nodeConfiguration;
        private readonly List<Task> _tasks = new List<Task>();
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

            _maxDegreeOfParallelism = Environment.ProcessorCount;

            _handlingFlowScopes = new List<IServiceScope>(_maxDegreeOfParallelism);

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                _handlingFlowScopes.Add(serviceProvider.CreateScope());
            }
        }

        public void Initialize(CancellationToken ct)
        {
            if (!IsInitialized)
            {
                _cancellationToken = ct;

                _cancellationToken.Register(() => 
                { 
                    _handlingFlowScopes.ForEach(s => s.Dispose());
                    _handlingFlowScopes.Clear();
                });

                _handlingFlowScopes.AsyncParallelForEach(async s =>
                {
                    await InitModules(s, ct);
                });

                IsInitialized = true;
            }
        }

        private async Task InitModules(IServiceScope s, CancellationToken ct)
        {
            var modulesRepo = s.ServiceProvider.GetRequiredService<IModulesRepository>();
            ObtainConfiguredModules(modulesRepo);
            foreach (IModule module in modulesRepo.GetBulkInstances())
            {
                try
                {
                    await module.Initialize(ct);
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

            // TODO: need to add handling the case when already started
            _handlingFlowScopes.ForEach(s =>
            {
                _tasks.Add(Process(s, _cancellationToken));
            });

            _log.Debug(() => "PacketsHandler started");
        }

        private async Task Process(IServiceScope serviceScope, CancellationToken cancellationToken)
        {
            var handlingFlow = serviceScope.ServiceProvider.GetRequiredService<IHandlingFlowContext>();
            _log.Debug(() => $"Process function #{handlingFlow.Index} starting");

            try
            {
                _trackingService.TrackMetric("ParallelProcesses", 1);

                StartModules(serviceScope);

                await StartHandlingFlow(serviceScope, cancellationToken);
            }
            finally
            {
                _log.Debug(() => "Process function finished");
                _trackingService.TrackMetric("ParallelProcesses", -1);
            }
        }

        private async Task StartHandlingFlow(IServiceScope serviceScope, CancellationToken cancellationToken)
        {
            var handlingFlow = serviceScope.ServiceProvider.GetRequiredService<IPacketHandlingFlow>();

            foreach (var packet in _packets.GetConsumingEnumerable(cancellationToken))
            {
                _trackingService.TrackMetric("MessagesQueueSize", -1);
                await handlingFlow.PostPacket(packet);
            }
        }

        private static void StartModules(IServiceScope serviceScope)
        {
            var modulesRepo = serviceScope.ServiceProvider.GetRequiredService<IModulesRepository>();
            foreach (IModule module in modulesRepo.GetBulkInstances())
            {
                module.StartModule();
            }
        }

        public void Dispose()
        {
            _packets.Dispose();
            _handlingFlowScopes.ForEach(s => s.Dispose());
            _handlingFlowScopes.Clear();
        }
    }
}
