using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Logging;
using System.Collections.Generic;
using O10.Core.Tracking;
using O10.Transactions.Core.Ledgers;
using O10.Network.Interfaces;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    internal class PacketsHandler : IPacketsHandler
    {
        private readonly ILogger _log;
        private readonly BlockingCollection<IPacketBase> _packets;
        private readonly PacketHandlingFlow[] _handlingFlows;
        private readonly int _maxDegreeOfParallelism;
        private readonly ITrackingService _trackingService;
        private CancellationToken _cancellationToken;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IPacketVerifiersRepository packetTypeHandlersFactory,
                              IPacketsHandlersRegistry blocksProcessorFactory,
                              ICoreVerifiersBulkFactory coreVerifiersBulkFactory,
                              ITrackingService trackingService,
                              ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _packets = new BlockingCollection<IPacketBase>();

            _maxDegreeOfParallelism = 4;

            _handlingFlows = new PacketHandlingFlow[_maxDegreeOfParallelism];

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                _handlingFlows[i] = new PacketHandlingFlow(i,
                                                           coreVerifiersBulkFactory,
                                                           packetTypeHandlersFactory,
                                                           blocksProcessorFactory,
                                                           trackingService,
                                                           loggerService);
            }

            _trackingService = trackingService;
        }

        public void Initialize(CancellationToken ct)
        {
            if (!IsInitialized)
            {
                _cancellationToken = ct;
                IsInitialized = true;
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

                foreach (var packet in _packets.GetConsumingEnumerable(_cancellationToken))
                {
                    _log.Debug(() => $"Picked for handling flow #{iteration} packet {packet.GetType().Name}");

                    _trackingService.TrackMetric("MessagesQueueSize", -1);
                    _handlingFlows[iteration].PostPacket(packet);
                }
            }
            finally
            {
                _log.Debug(() => "Process function finished");
                _trackingService.TrackMetric("ParallelProcesses", -1);
            }
        }
    }
}
