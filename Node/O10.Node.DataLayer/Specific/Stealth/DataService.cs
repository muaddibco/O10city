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

namespace O10.Node.DataLayer.Specific.Stealth
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
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

        public override TaskCompletionWrapper<IKey> Add(IPacketBase packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            Logger?.LogIfDebug(() => $"Storing {packet.GetType().Name}: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");

            if (packet is StealthPacket stealth)
            {
                var hash = _defaultHashCalculation.CalculateHash(packet.ToString());
                var addCompletionWrapper = new TaskCompletionWrapper<IKey>(IdentityKeyProvider.GetKey(hash));
                var addCompletion = Service.AddStealthBlock(stealth.Body.KeyImage, stealth.Body.TransactionType, stealth.Body.DestinationKey, stealth.ToString(), hash.ToString());
                addCompletion.Task.ContinueWith((t, o) =>
                {
                    var w = (TaskCompletionWrapper<IPacketBase>)o;
                    if (t.IsCompletedSuccessfully)
                    {
                        w.TaskCompletion.SetResult(new ItemAddedNotification(new LedgerAndIdKey(w.State.LedgerType, t.Result.StealthTransactionId)));
                    }
                }, addCompletionWrapper, TaskScheduler.Default);

                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} completed");
                return addCompletionWrapper;
            }
        
            Logger?.Error($"Attempt to store an improper packet type {packet.GetType().FullName}");
            throw new Exception($"Attempt to store an improper packet type {packet.GetType().FullName}");
        }

        public override IEnumerable<IPacketBase> Get(IDataKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key is CombinedHashKey combinedHashKey)
            {
                StealthTransaction stealth = Service.GetStealthBySyncAndHash(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);

                if (stealth == null)
                {
                    Task.Delay(200).Wait();
                    stealth = Service.GetStealthBySyncAndHash(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);

                    if (stealth == null)
                    {
                        Task.Delay(200).Wait();
                        stealth = Service.GetStealthBySyncAndHash(combinedHashKey.CombinedBlockHeight, combinedHashKey.Hash);
                    }
                }

                if (stealth != null)
                {
                    return new List<IPacketBase> { TranslatorsRepository.GetInstance<Model.StealthTransaction, IPacketBase>().Translate(stealth) };
                }
            }

            throw new DataKeyNotSupportedException(key);
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
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
