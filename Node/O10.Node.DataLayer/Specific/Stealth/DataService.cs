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
using O10.Core.Identity;
using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Core.Translators;
using O10.Core.Logging;

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

        public override async Task<DataResult<IPacketBase>> Add(IPacketBase packet)
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

                var transaction = await Service
                    .AddTransaction(stealth.Payload.Transaction.KeyImage, stealth.Payload.Transaction.TransactionType, stealth.Payload.Transaction.DestinationKey, stealth.ToJson(), hash);

                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} with hash [{hashKey}] completed");
                return new DataResult<IPacketBase>(new HashAndIdKey(hashKey, transaction.StealthTransactionId), packet);
            }
        
            Logger?.Error($"Attempt to store an improper packet type {packet.GetType().FullName}");
            throw new Exception($"Attempt to store an improper packet type {packet.GetType().FullName}");
        }

        public override async Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken) 
            => key == null
                ? throw new ArgumentNullException(nameof(key))
                : key switch 
                { 
                    CombinedHashKey combinedHashKey => await Get(combinedHashKey, cancellationToken),
                    HashKey hashKey => await Get(hashKey, cancellationToken),
                    _ => throw new DataKeyNotSupportedException(key) 
                };

        private async Task<IEnumerable<IPacketBase>> Get(CombinedHashKey combinedHashKey, CancellationToken cancellationToken)
        {
            var stealth = await Service.GetTransaction(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash, cancellationToken);

            if (stealth == null)
            {
                Task.Delay(200).Wait();
                stealth = await Service.GetTransaction(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash, cancellationToken);

                if (stealth == null)
                {
                    Task.Delay(200).Wait();
                    stealth = await Service.GetTransaction(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash, cancellationToken);
                }
            }

            if (stealth != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<StealthTransaction, StealthPacket>().Translate(stealth) };
            }

            return new List<IPacketBase>();
        }

        private async Task<IEnumerable<IPacketBase>> Get(HashKey hashKey, CancellationToken cancellationToken)
        {
            var stealth = await Service.GetTransaction(hashKey.Hash, cancellationToken);

            if (stealth == null)
            {
                Task.Delay(200).Wait();
                stealth = await Service.GetTransaction(hashKey.Hash, cancellationToken);

                if (stealth == null)
                {
                    Task.Delay(200).Wait();
                    stealth = await Service.GetTransaction(hashKey.Hash, cancellationToken);
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

        public async Task<byte[]> GetPacketHash(IDataKey dataKey, CancellationToken cancellationToken)
        {
            if (dataKey == null)
            {
                throw new ArgumentNullException(nameof(dataKey));
            }

            if (dataKey is KeyImageKey keyImageKey)
            {
                return await Service.GetHashByKeyImage(keyImageKey.KeyImage, cancellationToken);
            }

            throw new DataKeyNotSupportedException(dataKey);
        }

        public override async Task AddDataKey(IDataKey key, IDataKey newKey, CancellationToken cancellationToken)
        {
            if (key is IdKey idKey && newKey is CombinedHashKey combined)
            {
                await Service.UpdateRegistryInfo(idKey.Id, combined.CombinedBlockHeight, cancellationToken);
            }
        }
    }
}
