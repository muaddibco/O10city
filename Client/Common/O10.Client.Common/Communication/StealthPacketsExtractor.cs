using Newtonsoft.Json;
using O10.Transactions.Core.Parsers;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Architecture;

namespace O10.Client.Common.Communication
{
	[RegisterExtension(typeof(IPacketsExtractor), Lifetime = LifetimeManagement.Scoped)]
	public class StealthPacketsExtractor : PacketsExtractorBase
	{

		public StealthPacketsExtractor(
            IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
            IGatewayService syncStateProvider,
			IStealthClientCryptoService clientCryptoService,
			IDataAccessService dataAccessService,
            ILoggerService loggerService) 
            : base(blockParsersRepositoriesRepository, syncStateProvider, clientCryptoService, dataAccessService, loggerService)
		{
		}

        public override string Name => "Stealth";

        protected override bool CheckPacketWitness(PacketWitness packetWitness)
		{
			_logger.LogIfDebug(() => $"[{_accountId}]: {nameof(CheckPacketWitness)} {JsonConvert.SerializeObject(packetWitness, new ByteArrayJsonConverter())}");

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

			if(isToMe1)
			{
				_logger.Debug($"[{_accountId}]: It was detected packet sent by myself");
			}

			if (isToMe2)
			{
				_logger.Debug($"[{_accountId}]: It was detected packet sent to me by other");
			}

			return isToMe1 || isToMe2;
		}
	}
}
