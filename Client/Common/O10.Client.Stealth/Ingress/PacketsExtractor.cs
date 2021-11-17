using Newtonsoft.Json;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Architecture;
using O10.Core.Serialization;
using O10.Client.Common.Communication;

namespace O10.Client.Stealth.Ingress
{
    [RegisterExtension(typeof(IPacketsExtractor), Lifetime = LifetimeManagement.Scoped)]
    public class PacketsExtractor : PacketsExtractorBase
    {

        public PacketsExtractor(
            IGatewayService syncStateProvider,
            IStealthClientCryptoService clientCryptoService,
            IDataAccessService dataAccessService,
            ILoggerService loggerService)
            : base(syncStateProvider, clientCryptoService, dataAccessService, loggerService)
        {
        }

        public override string Name => "Stealth";

        protected override bool CheckPacketWitness(PacketWitness packetWitness)
        {
            _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(CheckPacketWitness)} {JsonConvert.SerializeObject(packetWitness, new ByteArrayJsonConverter())}");

            if (packetWitness.IsIdentityIssuing)
            {
                return true;
            }

            bool isToMe1 = packetWitness.DestinationKey?.Length == Globals.NODE_PUBLIC_KEY_SIZE
                  && packetWitness.TransactionKey?.Length == Globals.NODE_PUBLIC_KEY_SIZE
                  && _clientCryptoService.CheckTarget(packetWitness.DestinationKey, packetWitness.TransactionKey);

            bool isToMe2 = packetWitness.DestinationKey2?.Length == Globals.NODE_PUBLIC_KEY_SIZE
                  && packetWitness.TransactionKey?.Length == Globals.NODE_PUBLIC_KEY_SIZE
                  && _clientCryptoService.CheckTarget(packetWitness.DestinationKey2, packetWitness.TransactionKey);

            if (isToMe1)
            {
                _logger.Debug($"[{AccountId}]: It was detected packet sent by myself");
            }

            if (isToMe2)
            {
                _logger.Debug($"[{AccountId}]: It was detected packet sent to me by other");
            }

            return isToMe1 || isToMe2;
        }
    }
}
