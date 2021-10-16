using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Core.Architecture;
using O10.Core.Translators;
using System.Threading;
using O10.Core.Logging;
using System.Threading.Tasks;
using O10.Node.DataLayer.DataServices.Keys;
using Newtonsoft.Json;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Exceptions;
using System;
using O10.Node.DataLayer.Specific.O10Id.Model;
using O10.Core.Serialization;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Transactions.Core.Ledgers;
using O10.Node.DataLayer.DataServices.Notifications;
using O10.Core.Models;
using O10.Core.Identity;

namespace O10.Node.DataLayer.Specific.O10Id
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Scoped)]
    public class DataService : ChainDataServiceBase<DataAccessService>
    {
        private readonly IHashCalculation _defaultHashCalculation;
        public override LedgerType LedgerType => LedgerType.O10State;

        public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            IHashCalculationsRepository hashCalculationsRepository,
            ITranslatorsRepository translatorsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ILoggerService loggerService)
            : base(dataAccessServiceRepository, translatorsRepository, identityKeyProvidersRegistry, loggerService)
        {
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override TaskCompletionWrapper<IPacketBase> Add(IPacketBase packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet is O10StatePacket statePacket)
            {
                var hash = _defaultHashCalculation.CalculateHash(packet.Transaction.ToString());
                var hashKey = IdentityKeyProvider.GetKey(hash);

                Logger?.LogIfDebug(() => $"Storing {packet.GetType().Name} with hash [{hashKey}]: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");

                var addCompletionWrapper = new TaskCompletionWrapper<IPacketBase>(packet);
                Service
                    .AddTransaction(statePacket.Payload.Transaction.Source, statePacket.Payload.Transaction.TransactionType, statePacket.Payload.Height, packet.ToJson(), hash)
                    .ContinueWith((t, o) => 
                    {
                        var w = ((Tuple<TaskCompletionWrapper<IPacketBase>, IKey>)o).Item1;
                        var h = ((Tuple<TaskCompletionWrapper<IPacketBase>, IKey>)o).Item2;
                        if (t.IsCompletedSuccessfully)
                        {
                            w.TaskCompletion.SetResult(new ItemAddedNotification(new HashAndIdKey(h, t.Result.O10TransactionId)));
                        }
                    }, new Tuple<TaskCompletionWrapper<IPacketBase>, IKey>(addCompletionWrapper, hashKey), TaskScheduler.Default);

                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} with hash [{hashKey}] completed");
                return addCompletionWrapper;
            }

            Logger?.Error($"Attempt to store an improper packet type {packet.GetType().FullName}");
            throw new Exception($"Attempt to store an improper packet type {packet.GetType().FullName}");
        }

        public override async Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken) 
            => key is null
                ? throw new ArgumentNullException(nameof(key))
                : key switch
                {
                    UniqueKey uniqueKey => Get(uniqueKey),
                    CombinedHashKey combinedHashKey => Get(combinedHashKey),
                    HashKey hashKey => Get(hashKey),
                    _ => throw new DataKeyNotSupportedException(key),
                };

        private IEnumerable<IPacketBase> Get(UniqueKey uniqueKey)
        {
            O10Transaction transactionalBlock = Service.GetLastTransactionalBlock(uniqueKey.IdentityKey);

            if (transactionalBlock != null)
            {
                var mapper = TranslatorsRepository.GetInstance<O10Transaction, O10StatePacket>();

                var block = mapper?.Translate(transactionalBlock);

                return new List<IPacketBase> { block };
            }

            return new List<IPacketBase>();
        }

        private IEnumerable<IPacketBase> Get(CombinedHashKey combinedHashKey)
        {
            O10Transaction transactionalBlock = Service.GetTransaction(combinedHashKey.Hash, combinedHashKey.CombinedBlockHeight);

            //TODO: this is very ugly implementation!!!
            if (transactionalBlock == null)
            {
                Task.Delay(200).Wait();
                transactionalBlock = Service.GetTransaction(combinedHashKey.Hash, combinedHashKey.CombinedBlockHeight);

                if (transactionalBlock == null)
                {
                    Task.Delay(200).Wait();
                    transactionalBlock = Service.GetTransaction(combinedHashKey.Hash, combinedHashKey.CombinedBlockHeight);
                }
            }

            if (transactionalBlock != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<O10Transaction, O10StatePacket>().Translate(transactionalBlock) };
            }

            return new List<IPacketBase>();
        }

        private IEnumerable<IPacketBase> Get(HashKey combinedHashKey)
        {
            O10Transaction transactionalBlock = Service.GetTransaction(combinedHashKey.Hash);

            //TODO: this is very ugly implementation!!!
            if (transactionalBlock == null)
            {
                Task.Delay(200).Wait();
                transactionalBlock = Service.GetTransaction(combinedHashKey.Hash);

                if (transactionalBlock == null)
                {
                    Task.Delay(200).Wait();
                    transactionalBlock = Service.GetTransaction(combinedHashKey.Hash);
                }
            }

            if (transactionalBlock != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<O10Transaction, O10StatePacket>().Translate(transactionalBlock) };
            }

            return new List<IPacketBase>();
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            await base.Initialize(cancellationToken);
        }

        public override void AddDataKey(IDataKey key, IDataKey newKey)
        {
            if(key is IdKey idKey && newKey is CombinedHashKey combined)
            {
                Service.UpdateRegistryInfo(idKey.Id, combined.CombinedBlockHeight);
            }
        }
    }
}
