﻿using System.Collections.Generic;
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

        public override async Task<DataResult<IPacketBase>> Add(IPacketBase packet)
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

                var transaction = await Service
                    .AddTransaction(statePacket.Payload.Transaction.Source, statePacket.Payload.Transaction.TransactionType, statePacket.Payload.Height, packet.ToJson(), hash, CancellationToken);

                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} with hash [{hashKey}] completed");
                return new DataResult<IPacketBase>(new HashAndIdKey(hashKey, transaction.O10TransactionId), packet);
            }

            Logger?.Error($"Attempt to store an improper packet type {packet.GetType().FullName}");
            throw new Exception($"Attempt to store an improper packet type {packet.GetType().FullName}");
        }

        public override async Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken) 
            => key is null
                ? throw new ArgumentNullException(nameof(key))
                : key switch
                {
                    UniqueKey uniqueKey => await Get(uniqueKey, cancellationToken),
                    CombinedHashKey combinedHashKey => await Get(combinedHashKey, cancellationToken),
                    HashKey hashKey => await Get(hashKey, cancellationToken),
                    _ => throw new DataKeyNotSupportedException(key),
                };

        private async Task<IEnumerable<IPacketBase>> Get(UniqueKey uniqueKey, CancellationToken cancellationToken)
        {
            O10Transaction transactionalBlock = await Service.GetLastTransactionalBlock(uniqueKey.IdentityKey, cancellationToken);

            if (transactionalBlock != null)
            {
                var mapper = TranslatorsRepository.GetInstance<O10Transaction, O10StatePacket>();

                var block = mapper?.Translate(transactionalBlock);

                return new List<IPacketBase> { block };
            }

            return new List<IPacketBase>();
        }

        private async Task<IEnumerable<IPacketBase>> Get(CombinedHashKey combinedHashKey, CancellationToken cancellationToken)
        {
            O10Transaction transactionalBlock = await Service.GetTransaction(combinedHashKey.Hash, combinedHashKey.CombinedBlockHeight, cancellationToken);

            //TODO: this is very ugly implementation!!!
            if (transactionalBlock == null)
            {
                Task.Delay(200).Wait();
                transactionalBlock = await Service.GetTransaction(combinedHashKey.Hash, combinedHashKey.CombinedBlockHeight, cancellationToken);

                if (transactionalBlock == null)
                {
                    Task.Delay(200).Wait();
                    transactionalBlock = await Service.GetTransaction(combinedHashKey.Hash, combinedHashKey.CombinedBlockHeight, cancellationToken);
                }
            }

            if (transactionalBlock != null)
            {
                return new List<IPacketBase> { TranslatorsRepository.GetInstance<O10Transaction, O10StatePacket>().Translate(transactionalBlock) };
            }

            return new List<IPacketBase>();
        }

        private async Task<IEnumerable<IPacketBase>> Get(HashKey combinedHashKey, CancellationToken cancellationToken)
        {
            O10Transaction transactionalBlock = await Service.GetTransaction(combinedHashKey.Hash, cancellationToken);

            //TODO: this is very ugly implementation!!!
            if (transactionalBlock == null)
            {
                Task.Delay(200).Wait();
                transactionalBlock = await Service.GetTransaction(combinedHashKey.Hash, cancellationToken);

                if (transactionalBlock == null)
                {
                    Task.Delay(200).Wait();
                    transactionalBlock = await Service.GetTransaction(combinedHashKey.Hash, cancellationToken);
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

        public override async Task AddDataKey(IDataKey key, IDataKey newKey, CancellationToken cancellationToken)
        {
            if(key is IdKey idKey && newKey is CombinedHashKey combined)
            {
                await Service.UpdateRegistryInfo(idKey.Id, combined.CombinedBlockHeight, cancellationToken);
            }
        }
    }
}
