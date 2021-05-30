using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Architecture;

namespace O10.Client.Common.Communication
{
    [RegisterExtension(typeof(IPacketsExtractor), Lifetime = LifetimeManagement.Scoped)]
    public class StatePacketsExtractor : PacketsExtractorBase
    {
        public StatePacketsExtractor(
            IGatewayService syncStateProvider,
            IStateClientCryptoService clientCryptoService,
            IDataAccessService dataAccessService,
            ILoggerService loggerService)
            : base(syncStateProvider, clientCryptoService, dataAccessService, loggerService)
        {
        }

        public override string Name => "State";

        protected override bool CheckPacketWitness(PacketWitness packetWitness)
        {
            if (packetWitness.IsIdentityIssuing)
            {
                _logger.Debug($"[{AccountId}]: obtained identity issuing packet");
                return true;
            }
            else if (packetWitness.KeyImage == null)
            {
                bool isToMe = _clientCryptoService.CheckTarget(packetWitness.DestinationKey);

                if (isToMe)
                {
                    _logger.Debug($"[{AccountId}]: It was detected data packet sent to me");
                }

                return isToMe;
            }
            else
            {
                _logger.LogIfDebug(() => $"[{AccountId}]: check destination of packet with {nameof(packetWitness.KeyImage)}={packetWitness.KeyImage}");
                bool isToMe1 = _clientCryptoService.CheckTarget(packetWitness.DestinationKey);
                bool isToMe2 = _clientCryptoService.CheckTarget(packetWitness.DestinationKey2);

                if (isToMe1)
                {
                    _logger.Debug($"[{AccountId}]: It was detected data packet sent by myself");
                }

                if (isToMe2)
                {
                    _logger.Debug($"[{AccountId}]: It was detected data packet sent to me by other");
                }

                return isToMe1 || isToMe2;
            }
        }
    }
}
