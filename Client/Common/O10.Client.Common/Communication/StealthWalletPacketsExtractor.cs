using System.Linq;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Parsers;
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
        private byte[] _nextKeyImage;
        private readonly IPropagatorBlock<NotificationBase, NotificationBase> _propagatorBlockNotifications;
        private readonly ITargetBlock<byte[]> _pipeInKeyImages;

        public StealthWalletPacketsExtractor(
            IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
            IDataAccessService dataAccessService,
            IStealthClientCryptoService clientCryptoService,
            IGatewayService syncStateProvider,
            ILoggerService loggerService)
            : base(blockParsersRepositoriesRepository, syncStateProvider, clientCryptoService, dataAccessService, loggerService)
        {
            _dataAccessService = dataAccessService;
            _propagatorBlockNotifications = new TransformBlock<NotificationBase, NotificationBase>(p => p);

            _pipeInKeyImages = new ActionBlock<byte[]>(k =>
            {
                _nextKeyImage = k;
            });
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

        public override ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return (ITargetBlock<T>)_pipeInKeyImages;
            }

            return base.GetTargetPipe<T>();
        }

        protected override bool CheckPacketWitness(PacketWitness packetWitness)
        {
            foreach (var item in _dataAccessService.GetUserAttributes(_accountId).Where(u => !u.IsOverriden && !u.LastCommitment.Equals32(new byte[32])))
            {
                byte[] nextKeyImage = item.NextKeyImage.HexStringToByteArray();// ((IUtxoClientCryptoService)_clientCryptoService).GetKeyImage(item.LastTransactionKey);

                _logger.LogIfDebug(() => $"[{_accountId}]: Checking KeyImage {nextKeyImage?.ToHexString() ?? "NULL"} compromised");

                if (!nextKeyImage.Equals32(_nextKeyImage) && (packetWitness.KeyImage?.Equals32(nextKeyImage) ?? false))
                {
                    _logger.LogIfDebug(() => $"[{_accountId}]: KeyImage {nextKeyImage?.ToHexString() ?? "NULL"} is compromised");
                    _propagatorBlockNotifications.SendAsync(new CompromisedKeyImage { KeyImage = packetWitness.KeyImage, TransactionKey = packetWitness.TransactionKey, DestinationKey = packetWitness.DestinationKey, Target = packetWitness.DestinationKey2 });
                    break;
                }
            }

            return base.CheckPacketWitness(packetWitness);
        }
    }
}
