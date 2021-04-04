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
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using O10.Core.Models;
using O10.Core.Identity;

namespace O10.Node.DataLayer.Specific.O10Id
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
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

            Logger?.LogIfDebug(() => $"Storing {packet.GetType().Name}: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");

            if (packet is O10StatePacket statePacket)
            {
                var hash = _defaultHashCalculation.CalculateHash(packet.ToString());
                var hashKey = IdentityKeyProvider.GetKey(hash);
                var addCompletionWrapper = new TaskCompletionWrapper<IPacketBase>(packet);
                var addCompletion = Service.AddTransaction(statePacket.Body.Source, statePacket.Body.TransactionType, statePacket.Height, packet.ToString(), hash);
                addCompletion.Task.ContinueWith((t, o) => 
                {
                    var w = ((Tuple<TaskCompletionWrapper<IPacketBase>, IKey>)o).Item1;
                    var h = ((Tuple<TaskCompletionWrapper<IPacketBase>, IKey>)o).Item2;
                    if(t.IsCompletedSuccessfully)
                    {
                        w.TaskCompletion.SetResult(new ItemAddedNotification(new HashAndIdKey(h, t.Result.O10TransactionId)));
                    }
                }, new Tuple<TaskCompletionWrapper<IPacketBase>, IKey>(addCompletionWrapper, hashKey), TaskScheduler.Default);

                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} completed");
                return addCompletionWrapper;
            }

            Logger?.Error($"Attempt to store an improper packet type {packet.GetType().FullName}");
            throw new Exception($"Attempt to store an improper packet type {packet.GetType().FullName}");
        }

        public override IEnumerable<IPacketBase> Get(IDataKey key)
            => key switch
            {
                UniqueKey uniqueKey => GetByUniqueKey(uniqueKey),
                CombinedHashKey combinedHashKey => GetByCombinedHashKey(combinedHashKey),
                _ => throw new DataKeyNotSupportedException(key),
            };

        private IEnumerable<IPacketBase> GetByUniqueKey(UniqueKey uniqueKey)
        {
            O10Transaction transactionalBlock = Service.GetLastTransactionalBlock(uniqueKey.IdentityKey);

            if (transactionalBlock != null)
            {
                ITranslator<O10Transaction, IPacketBase> mapper = TranslatorsRepository.GetInstance<O10Transaction, IPacketBase>();

                var block = mapper?.Translate(transactionalBlock);

                return new List<IPacketBase> { block };
            }

            return new List<IPacketBase>();
        }

        private IEnumerable<IPacketBase> GetByCombinedHashKey(CombinedHashKey combinedHashKey)
        {
            O10Transaction transactionalBlock = Service.GetTransactionalBySyncAndHash(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);

            //TODO: this is very ugly implementation!!!
            if (transactionalBlock == null)
            {
                Task.Delay(200).Wait();
                transactionalBlock = Service.GetTransactionalBySyncAndHash(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);

                if (transactionalBlock == null)
                {
                    Task.Delay(200).Wait();
                    transactionalBlock = Service.GetTransactionalBySyncAndHash(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);
                }
            }

            if (transactionalBlock != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<O10Transaction, IPacketBase>().Translate(transactionalBlock) };
            }

            return new List<IPacketBase>();
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
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
