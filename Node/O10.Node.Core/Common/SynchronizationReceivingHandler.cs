using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Transactions.Core.Serializers.RawPackets;
using O10.Network.Interfaces;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Core.HashCalculations;
using O10.Core.Models;
using O10.Core;
using O10.Node.DataLayer.DataServices;

namespace O10.Node.Core.Common
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationReceivingHandler : IBlocksHandler
    {
        public const string NAME = "SynchronizationReceiving";
        private readonly BlockingCollection<SynchronizationConfirmedBlock> _synchronizationBlocks;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly INeighborhoodState _neighborhoodState;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IChainDataService _chainDataService;
        private readonly IHashCalculation _hashCalculation;
        private IServerCommunicationService _communicationService;
        private uint _lastRetransmittedSyncBlockHeight;

        public SynchronizationReceivingHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IChainDataServicesManager chainDataServicesManager, IHashCalculationsRepository hashCalculationsRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _neighborhoodState = statesRepository.GetInstance<INeighborhoodState>();
            _synchronizationBlocks = new BlockingCollection<SynchronizationConfirmedBlock>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Synchronization;

        public void Initialize(CancellationToken ct)
        {
            //TODO: need to move definition of communication service name to configuration file
            _communicationService = _communicationServicesRegistry.GetInstance("GenericUdp");

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(PacketBase blockBase)
        {
			if (blockBase is SynchronizationConfirmedBlock synchronizationBlock)
			{
				_synchronizationBlocks.Add(synchronizationBlock);
			}
		}

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
			try
			{
				foreach (SynchronizationConfirmedBlock synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
				{
					if ((_synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0) >= synchronizationBlock.BlockHeight)
					{
						continue;
					}

					_synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationBlock.BlockHeight, _hashCalculation.CalculateHash(synchronizationBlock.RawData), synchronizationBlock.ReportedTime, DateTime.Now, synchronizationBlock.Round));

					_chainDataService.Add(synchronizationBlock);

					IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(synchronizationBlock);
					_communicationService.PostMessage(_neighborhoodState.GetAllNeighbors(), packetProvider);
				}

			}
			catch (OperationCanceledException)
			{
				
			}
		}

        #endregion Private Functions
    }
}
