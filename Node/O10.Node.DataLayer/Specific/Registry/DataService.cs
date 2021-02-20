using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Translators;
using O10.Node.DataLayer.DataAccess;
using RegistryFullBlockPacket = O10.Transactions.Core.Ledgers.Registry.RegistryFullBlock;
using RegistryFullBlockDb = O10.Node.DataLayer.Specific.Registry.Model.RegistryFullBlock;
using O10.Core.Tracking;
using System.Globalization;
using O10.Core;

namespace O10.Node.DataLayer.Specific.Registry
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class DataService : ChainDataServiceBase<DataAccessService>
    {
        private readonly IHashCalculation _defaultHashCalculation;
        private BufferBlock<RegistryFullBlockPacket> _packets;
        private readonly ConcurrentDictionary<ulong, ConcurrentBag<RegistryFullBlockDb>> _registryFullBlocks = new ConcurrentDictionary<ulong, ConcurrentBag<RegistryFullBlockDb>>();
        private readonly ITrackingService _trackingService;

        public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            ITranslatorsRepository translatorsRepository,
            IHashCalculationsRepository hashCalculationsRepository,
            ILoggerService loggerService,
            ITrackingService trackingService)
            : base(dataAccessServiceRepository, translatorsRepository, loggerService)
        {
            _trackingService = trackingService;
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        override public LedgerType LedgerType => LedgerType.Registry;
        public override void Add(PacketBase item)
        {
            if (item is RegistryFullBlockPacket registryFullBlock)
            {
                _packets.Post(registryFullBlock);
            }
        }

        public override IEnumerable<PacketBase> Get(IDataKey key)
        {
            if (key is SyncHashKey syncTransactionKey)
            {
                RegistryFullBlockDb transactionsRegistryBlock =
                    FetchRegistryBlock(syncTransactionKey.SyncBlockHeight, syncTransactionKey.Hash)
                    ?? FetchRegistryBlock(syncTransactionKey.SyncBlockHeight - 1, syncTransactionKey.Hash)
                    ?? FetchRegistryBlock(syncTransactionKey.SyncBlockHeight - 2, syncTransactionKey.Hash);

                PacketBase blockBase = null;

                if (transactionsRegistryBlock != null)
                {
                    blockBase = TranslatorsRepository.GetInstance<RegistryFullBlockDb, PacketBase>().Translate(transactionsRegistryBlock);
                }

                return new List<PacketBase> { blockBase };
            }

            throw new ArgumentException(nameof(key));
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
            _packets = new BufferBlock<RegistryFullBlockPacket>(new DataflowBlockOptions { CancellationToken = cancellationToken });

            foreach (var registryFullBlock in Service.GetAllRegistryFullBlocks())
            {
                _registryFullBlocks.AddOrUpdate(registryFullBlock.SyncBlockHeight, new ConcurrentBag<RegistryFullBlockDb>(), (_, v) => v);
                _registryFullBlocks[registryFullBlock.SyncBlockHeight].Add(registryFullBlock);
            }

            ConsumeSynchronizationRegistryCombinedBlock(_packets, cancellationToken);
        }

        #region Private Functions

        private async Task ConsumeSynchronizationRegistryCombinedBlock(IReceivableSourceBlock<RegistryFullBlockPacket> source, CancellationToken cancellationToken)
        {
            while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
                if (source.TryReceiveAll(out IList<RegistryFullBlockPacket> blocks))
                {
                    RegistryFullBlockDb[] registryFullBlocks = blocks.Select(b =>
                    {
                        string hash = _defaultHashCalculation.CalculateHash(b.ToString()).ToHexString();
                        return new RegistryFullBlockDb { SyncBlockHeight = b.SyncHeight, Round = b.Height, TransactionsCount = b.StateWitnesses.Length + b.StealthWitnesses.Length, Content = b.ToString(), Hash = hash, HashString = hash };
                    }).ToArray();

                    foreach (var registryFullBlock in registryFullBlocks)
                    {
                        _registryFullBlocks.AddOrUpdate(registryFullBlock.SyncBlockHeight, new ConcurrentBag<RegistryFullBlockDb>(), (_, v) => v);
                        _registryFullBlocks[registryFullBlock.SyncBlockHeight].Add(registryFullBlock);
                    }

                    Service.AddRegistryFullBlocks(registryFullBlocks);
                }
            }
        }

        private RegistryFullBlockDb FetchRegistryBlock(ulong syncBlockHeight, byte[] hash)
        {
            string hashString = hash.ToHexString();
            DateTimeOffset start = DateTimeOffset.UtcNow;
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<RegistryFullBlockDb> transactionsRegistryBlocks = _registryFullBlocks.GetOrAdd(syncBlockHeight, _ => new ConcurrentBag<RegistryFullBlockDb>()).ToList();
            RegistryFullBlockDb transactionsRegistryBlock = transactionsRegistryBlocks.Find(t => hashString == t.Hash);
            stopwatch.Stop();
            _trackingService.TrackDependency(nameof(DataService), nameof(FetchRegistryBlock), $"syncBlockHeight: {syncBlockHeight.ToString(CultureInfo.InvariantCulture)}", start, stopwatch.Elapsed);

            return transactionsRegistryBlock;
        }

        #endregion Private Functions
    }
}
