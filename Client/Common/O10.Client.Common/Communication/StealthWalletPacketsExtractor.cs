﻿using System.Linq;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Architecture;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Notifications;

namespace O10.Client.Common.Communication
{
    [RegisterExtension(typeof(IPacketsExtractor), Lifetime = LifetimeManagement.Scoped)]
    public class StealthWalletPacketsExtractor : StealthPacketsExtractor
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IStealthTransactionsService _stealthTransactionsService;
        private readonly IPropagatorBlock<NotificationBase, NotificationBase> _propagatorBlockNotifications;

        public StealthWalletPacketsExtractor(
            IDataAccessService dataAccessService,
            IStealthClientCryptoService clientCryptoService,
            IStealthTransactionsService stealthTransactionsService,
            IGatewayService syncStateProvider,
            ILoggerService loggerService)
            : base(syncStateProvider, clientCryptoService, dataAccessService, loggerService)
        {
            _dataAccessService = dataAccessService;
            _stealthTransactionsService = stealthTransactionsService;
            _propagatorBlockNotifications = new TransformBlock<NotificationBase, NotificationBase>(p => p);
        }

        public override string Name => "StealthWallet";

        public override ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if (typeof(T) == typeof(NotificationBase))
            {
                return (ISourceBlock<T>)_propagatorBlockNotifications;
            }

            return base.GetSourcePipe<T>();
        }

        protected override bool CheckPacketWitness(PacketWitness packetWitness)
        {
            foreach (var item in _dataAccessService.GetUserAttributes(AccountId).Where(u => !u.IsOverriden && !u.LastCommitment.Equals32(new byte[32])))
            {
                byte[] nextKeyImage = item.NextKeyImage.HexStringToByteArray();// ((IUtxoClientCryptoService)_clientCryptoService).GetKeyImage(item.LastTransactionKey);

                _logger.LogIfDebug(() => $"[{AccountId}]: Checking KeyImage {nextKeyImage?.ToHexString() ?? "NULL"} compromised");

                if (!(_stealthTransactionsService.NextKeyImage?.Equals(nextKeyImage) ?? false) && (packetWitness.KeyImage?.Equals(nextKeyImage) ?? false))
                {
                    _logger.LogIfDebug(() => $"[{AccountId}]: KeyImage {nextKeyImage?.ToHexString() ?? "NULL"} is compromised");
                    _propagatorBlockNotifications.SendAsync(new CompromisedKeyImage { KeyImage = packetWitness.KeyImage, TransactionKey = packetWitness.TransactionKey, DestinationKey = packetWitness.DestinationKey, Target = packetWitness.DestinationKey2 });
                    break;
                }
            }

            return base.CheckPacketWitness(packetWitness);
        }
    }
}
