using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Core.Translators;
using O10.Node.DataLayer.DataAccess;
using RegistryFullBlockDb = O10.Node.DataLayer.Specific.Registry.Model.RegistryFullBlock;
using O10.Core.Tracking;
using O10.Core;
using O10.Transactions.Core.Ledgers;
using O10.Core.Identity;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Node.DataLayer.DataServices.Notifications;

namespace O10.Node.DataLayer.Specific.Registry
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class DataService : ChainDataServiceBase<DataAccessService>
    {
        private readonly IHashCalculation _defaultHashCalculation;
        private BufferBlock<TaskCompletionWrapper<KeyedEntity<IPacketBase>>> _packets;
        private readonly ConcurrentDictionary<long, ConcurrentBag<RegistryFullBlockDb>> _registryFullBlocks = new ConcurrentDictionary<long, ConcurrentBag<RegistryFullBlockDb>>();
        private readonly ITrackingService _trackingService;

        public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            ITranslatorsRepository translatorsRepository,
            IHashCalculationsRepository hashCalculationsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ILoggerService loggerService,
            ITrackingService trackingService)
            : base(dataAccessServiceRepository, translatorsRepository, identityKeyProvidersRegistry, loggerService)
        {
            _trackingService = trackingService;
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        override public LedgerType LedgerType => LedgerType.Registry;

        public override TaskCompletionWrapper<IPacketBase> Add(IPacketBase item)
        {
            if (item is RegistryPacket registryPacket && registryPacket.Payload.Transaction is FullRegistryTransaction)
            {
                var completionWithKey = new TaskCompletionWrapper<IPacketBase>(item);

                var completionWithPacket = new TaskCompletionWrapper<KeyedEntity<IPacketBase>>(new KeyedEntity<IPacketBase>(registryPacket));
                _packets.Post(completionWithPacket);

                completionWithPacket.TaskCompletion.Task.ContinueWith((t, o) => 
                { 
                    if(t.IsCompletedSuccessfully)
                    {
                        ((TaskCompletionWrapper<IPacketBase>)o).TaskCompletion.SetResult(t.Result);
                    }
                    else
                    {
                        ((TaskCompletionWrapper<IPacketBase>)o).TaskCompletion.SetException(t.Exception);
                    }
                }, completionWithKey, TaskScheduler.Default);

                return completionWithKey;
            }

            return null;
        }

        public override IEnumerable<IPacketBase> Get(IDataKey key)
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

        public override void Initialize(CancellationToken cancellationToken)
        {
            _packets = new BufferBlock<TaskCompletionWrapper<KeyedEntity<IPacketBase>>>(new DataflowBlockOptions { CancellationToken = cancellationToken });

            foreach (var registryFullBlock in Service.GetAllRegistryFullBlocks())
            {
                _registryFullBlocks.AddOrUpdate(registryFullBlock.SyncBlockHeight, new ConcurrentBag<RegistryFullBlockDb>(), (_, v) => v);
                _registryFullBlocks[registryFullBlock.SyncBlockHeight].Add(registryFullBlock);
            }

            ConsumeSynchronizationRegistryCombinedBlock(_packets, cancellationToken);
        }

        #region Private Functions

        private async Task ConsumeSynchronizationRegistryCombinedBlock(IReceivableSourceBlock<TaskCompletionWrapper<KeyedEntity<IPacketBase>>> source, CancellationToken cancellationToken)
        {
            while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
                if (source.TryReceiveAll(out IList<TaskCompletionWrapper<KeyedEntity<IPacketBase>>> blocks))
                {
                    RegistryFullBlockDb[] registryFullBlocks = blocks.Select(b =>
                    {
                        b.State.Key = IdentityKeyProvider.GetKey(_defaultHashCalculation.CalculateHash(b.State.Entity.ToString()));
                        
                        return new RegistryFullBlockDb 
                        { 
                            SyncBlockHeight = b.State.Entity.AsPacket<RegistryPacket>().Payload.SyncHeight, 
                            Round = b.State.Entity.AsPacket<RegistryPacket>().Payload.Height, 
                            TransactionsCount = b.State.Entity.AsPacket<RegistryPacket>().With<FullRegistryTransaction>().Witnesses.Length, 
                            Content = b.State.Entity.ToJson(), 
                            Hash = b.State.Key.ToString(), 
                            HashString = b.State.Key.ToString()
                        };
                    }).ToArray();

                    foreach (var registryFullBlock in registryFullBlocks)
                    {
                        _registryFullBlocks.AddOrUpdate(registryFullBlock.SyncBlockHeight, new ConcurrentBag<RegistryFullBlockDb>(), (_, v) => v);
                        _registryFullBlocks[registryFullBlock.SyncBlockHeight].Add(registryFullBlock);
                    }

                    Service.AddRegistryFullBlocks(registryFullBlocks);

                    foreach (var item in blocks)
                    {
                        item.TaskCompletion.SetResult(new ItemAddedNotification(new UniqueKey(item.State.Key)));
                    }
                }
            }
        }

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

        public override void AddDataKey(IDataKey key, IDataKey newKey)
        {
            throw new NotImplementedException();
        }

        #endregion Private Functions
    }
}
