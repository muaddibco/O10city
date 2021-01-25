using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Transactions.Core.Interfaces;
using O10.Core.Logging;
using O10.Transactions.Core.Parsers;
using System.Collections.Generic;
using O10.Core.Tracking;
using O10.Core.Models;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    internal class PacketsHandler : IPacketsHandler
    {
        private readonly ILogger _log;
        private readonly BlockingCollection<byte[]> _messagePackets;
        private readonly BlockingCollection<PacketBase> _packets;
        private readonly PacketHandlingFlow[] _handlingFlows;
        private readonly int _maxDegreeOfParallelism;
        private readonly ITrackingService _trackingService;
        private CancellationToken _cancellationToken;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IPacketVerifiersRepository packetTypeHandlersFactory, IBlockParsersRepositoriesRepository blockParsersFactoriesRepository, IBlocksHandlersRegistry blocksProcessorFactory, ICoreVerifiersBulkFactory coreVerifiersBulkFactory, ITrackingService trackingService, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _messagePackets = new BlockingCollection<byte[]>();
            _packets = new BlockingCollection<PacketBase>();

            _maxDegreeOfParallelism = 4;

            _handlingFlows = new PacketHandlingFlow[_maxDegreeOfParallelism];

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                _handlingFlows[i] = new PacketHandlingFlow(i, coreVerifiersBulkFactory, packetTypeHandlersFactory, blockParsersFactoriesRepository, blocksProcessorFactory, trackingService, loggerService);
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

        /// <summary>
        /// Bytes being pushed to <see cref="IPacketsHandler"/> must form complete packet for following validation and processing
        /// </summary>
        /// <param name="messagePacket">Bytes of complete message for following processing</param>
        public void Push(byte[] messagePacket)
        {
            _trackingService.TrackMetric("PushedForHandlingTransactionsThroughput", 1);

            _log.Debug(() => $"Pushed packer for handling: {messagePacket.ToHexString()}");

            _messagePackets.Add(messagePacket);
            _trackingService.TrackMetric("MessagesQueueSize", 1);
        }

        public void Push(PacketBase packet)
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
                tasks.Add(Task.Factory.StartNew(o => Parse((int)o), i, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current));
            }

            _log.Debug(() => "PacketsHandler started");
        }

        private void Parse(int iteration)
        {
            _log.Debug(() => $"Parse function #{iteration} starting");

            try
            {
                _trackingService.TrackMetric("ParallelParsers", 1);

                foreach (byte[] messagePacket in _messagePackets.GetConsumingEnumerable(_cancellationToken))
                {
					_log.Debug(() => $"Picked for handling flow #{iteration} packet {messagePacket.ToHexString()}");

					_trackingService.TrackMetric("MessagesQueueSize", -1);
                    _handlingFlows[iteration].PostMessage(messagePacket);
                }
            }
            finally
            {
                _log.Debug(() => "Parse function finished");
                _trackingService.TrackMetric("ParallelParsers", -1);
            }
        }

        private void Process(int iteration)
        {
            _log.Debug(() => $"Process function #{iteration} starting");

            try
            {
                _trackingService.TrackMetric("ParallelProcesses", 1);

                foreach (PacketBase packet in _packets.GetConsumingEnumerable(_cancellationToken))
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
