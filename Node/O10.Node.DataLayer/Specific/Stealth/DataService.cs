using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Node.DataLayer.Specific.Stealth.Model;
using O10.Core.Architecture;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Exceptions;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;
using O10.Core.Identity;
using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Core.Translators;
using O10.Core.Logging;
using O10.Node.DataLayer.DataServices.Notifications;
using O10.Core.ExtensionMethods;

namespace O10.Node.DataLayer.Specific.Stealth
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Scoped)]
    public class DataService : ChainDataServiceBase<DataAccessService>, IStealthDataService
    {
        private readonly IHashCalculation _defaultHashCalculation;

        public DataService(INodeDataAccessServiceRepository dataAccessServiceRepository,
                           IHashCalculationsRepository hashCalculationsRepository,
                           ITranslatorsRepository translatorsRepository,
                           IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                           ILoggerService loggerService)
            : base(dataAccessServiceRepository, translatorsRepository, identityKeyProvidersRegistry, loggerService)
        {
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public override TaskCompletionWrapper<IPacketBase> Add(IPacketBase packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet is StealthPacket stealth)
            {
                var hash = _defaultHashCalculation.CalculateHash(stealth.Payload.Transaction.ToString());
                var hashKey = IdentityKeyProvider.GetKey(hash);

                Logger?.LogIfDebug(() => $"Storing {packet.GetType().Name} with hash [{hashKey}]: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");

                var addCompletionWrapper = new TaskCompletionWrapper<IPacketBase>(packet);
                Service
                    .AddStealthBlock(stealth.Payload.Transaction.KeyImage, stealth.Payload.Transaction.TransactionType, stealth.Payload.Transaction.DestinationKey, stealth.ToJson(), hash.ToHexString())
                    .ContinueWith((t, o) =>
                    {
                        var w = ((Tuple<TaskCompletionWrapper<IPacketBase>, IKey>)o).Item1;
                        var h = ((Tuple<TaskCompletionWrapper<IPacketBase>, IKey>)o).Item2;
                        if (t.IsCompletedSuccessfully)
                        {
                            w.TaskCompletion.SetResult(new ItemAddedNotification(new HashAndIdKey(h, t.Result.StealthTransactionId)));
                        }
                        else
                        {
                            w.TaskCompletion.SetException(t.Exception.InnerException);
                        }
                    }, new Tuple<TaskCompletionWrapper<IPacketBase>, IKey>(addCompletionWrapper, hashKey), TaskScheduler.Default);

                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} with hash [{hashKey}] completed");
                return addCompletionWrapper;
            }
        
            Logger?.Error($"Attempt to store an improper packet type {packet.GetType().FullName}");
            throw new Exception($"Attempt to store an improper packet type {packet.GetType().FullName}");
        }

        public override async Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken) 
            => key == null
                ? throw new ArgumentNullException(nameof(key))
                : key switch 
                { 
                    CombinedHashKey combinedHashKey => await Get(combinedHashKey),
                    HashKey hashKey => Get(hashKey),
                    _ => throw new DataKeyNotSupportedException(key) 
                };

        private async Task<IEnumerable<IPacketBase>> Get(CombinedHashKey combinedHashKey)
        {
            var stealth = Service.GetTransaction(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);

            if (stealth == null)
            {
                Task.Delay(200).Wait();
                stealth = Service.GetTransaction(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);

                if (stealth == null)
                {
                    Task.Delay(200).Wait();
                    stealth = Service.GetTransaction(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);
                }
            }

            if (stealth != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<StealthTransaction, StealthPacket>().Translate(stealth) };
            }

            return new List<IPacketBase>();
        }

        private IEnumerable<IPacketBase> Get(HashKey hashKey)
        {
            var stealth = Service.GetTransaction(hashKey.Hash);

            if (stealth == null)
            {
                Task.Delay(200).Wait();
                stealth = Service.GetTransaction(hashKey.Hash);

                if (stealth == null)
                {
                    Task.Delay(200).Wait();
                    stealth = Service.GetTransaction(hashKey.Hash);
                }
            }

            if (stealth != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<StealthTransaction, StealthPacket>().Translate(stealth) };
            }

            return new List<IPacketBase>();
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            await base.Initialize(cancellationToken);
        }

        public string GetPacketHash(IDataKey dataKey)
        {
            if (dataKey == null)
            {
                throw new ArgumentNullException(nameof(dataKey));
            }

            if (dataKey is KeyImageKey keyImageKey)
            {
                return Service.GetHashByKeyImage(keyImageKey.KeyImage);
            }

            throw new DataKeyNotSupportedException(dataKey);
        }

        public override void AddDataKey(IDataKey key, IDataKey newKey)
        {
            if (key is IdKey idKey && newKey is CombinedHashKey combined)
            {
                Service.UpdateRegistryInfo(idKey.Id, combined.CombinedBlockHeight);
            }
        }
    }
}
