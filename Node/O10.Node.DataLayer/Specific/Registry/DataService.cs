using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Core.Translators;
using O10.Node.DataLayer.DataAccess;
using O10.Core;
using O10.Transactions.Core.Ledgers;
using O10.Core.Identity;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Registry.Transactions;

namespace O10.Node.DataLayer.Specific.Registry
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Scoped)]
    public class DataService : ChainDataServiceBase<DataAccessService>
    {
        private readonly IHashCalculation _defaultHashCalculation;

        public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            ITranslatorsRepository translatorsRepository,
            IHashCalculationsRepository hashCalculationsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ILoggerService loggerService)
            : base(dataAccessServiceRepository, translatorsRepository, identityKeyProvidersRegistry, loggerService)
        {
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        override public LedgerType LedgerType => LedgerType.Registry;

        public override async Task<DataResult<IPacketBase>> Add(IPacketBase item)
        {
            if (item is RegistryPacket registryPacket && registryPacket.Payload.Transaction is FullRegistryTransaction)
            {
                await Service.AddRegistryFullBlock(
                    registryPacket.Payload.SyncHeight,
                    registryPacket.Payload.Height,
                    registryPacket.Transaction<FullRegistryTransaction>().Witnesses.Length,
                    registryPacket.ToJson(),
                    _defaultHashCalculation.CalculateHash(registryPacket.ToString()),
                    CancellationToken);

                return new DataResult<IPacketBase>(null, registryPacket);
            }

            return null;
        }

        public override async Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //if (key is SyncHashKey syncTransactionKey)
            //{
            //    RegistryFullBlockDb transactionsRegistryBlock =
            //        FetchRegistryBlock(syncTransactionKey.SyncBlockHeight, syncTransactionKey.Hash)
            //        ?? FetchRegistryBlock(syncTransactionKey.SyncBlockHeight - 1, syncTransactionKey.Hash)
            //        ?? FetchRegistryBlock(syncTransactionKey.SyncBlockHeight - 2, syncTransactionKey.Hash);

            //    PacketBase blockBase = null;

            //    if (transactionsRegistryBlock != null)
            //    {
            //        blockBase = TranslatorsRepository.GetInstance<RegistryFullBlockDb, PacketBase>().Translate(transactionsRegistryBlock);
            //    }

            //    return new List<PacketBase> { blockBase };
            //}

            //throw new ArgumentException(nameof(key));
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            await base.Initialize(cancellationToken);
        }

        #region Private Functions


        //private RegistryFullBlockDb FetchRegistryBlock(long syncBlockHeight, IKey hash)
        //{
        //    string hashString = hash.ToString();
        //    DateTimeOffset start = DateTimeOffset.UtcNow;
        //    Stopwatch stopwatch = Stopwatch.StartNew();
        //    List<RegistryFullBlockDb> transactionsRegistryBlocks = _registryFullBlocks.GetOrAdd(syncBlockHeight, _ => new ConcurrentBag<RegistryFullBlockDb>()).ToList();
        //    RegistryFullBlockDb transactionsRegistryBlock = transactionsRegistryBlocks.Find(t => hashString == t.Hash);
        //    stopwatch.Stop();
        //    _trackingService.TrackDependency(nameof(DataService), nameof(FetchRegistryBlock), $"syncBlockHeight: {syncBlockHeight.ToString(CultureInfo.InvariantCulture)}", start, stopwatch.Elapsed);

        //    return transactionsRegistryBlock;
        //}

        public override Task AddDataKey(IDataKey key, IDataKey newKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion Private Functions
    }
}
