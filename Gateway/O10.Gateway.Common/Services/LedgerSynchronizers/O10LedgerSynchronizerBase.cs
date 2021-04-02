using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Core.Serialization;
using O10.Gateway.Common.Configuration;
using O10.Gateway.Common.Exceptions;
using O10.Gateway.DataLayer.Model;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    public abstract class O10LedgerSynchronizerBase : ILedgerSynchronizer
    {
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly ILogger _logger;

        public O10LedgerSynchronizerBase(IConfigurationService configurationService, ILoggerService loggerService)
        {
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
            _logger = loggerService.GetLogger(GetType().Name);
        }

        public abstract LedgerType LedgerType { get; }

        public abstract IPacketBase GetByWitness(WitnessPacket witnessPacket);

        public virtual async Task SyncByWitness(WitnessPacket witnessPacket)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            Url url = _synchronizerConfiguration.NodeApiUri.AppendPathSegments("Ledger", witnessPacket.ReferencedLedgerType, "Transaction").SetQueryParam("combinedBlockHeight", witnessPacket.CombinedBlockHeight).SetQueryParam("hash", witnessPacket.ReferencedBodyHash.Hash);
            _logger.Info($"Querying packet by the URI {url}");

            try
            {
                var packet = await url.GetJsonAsync<IPacketBase>().ConfigureAwait(false);
                _logger.LogIfDebug(() => $"Packet obtained from URI {url}: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");

                if (packet != null)
                {
                    StorePacket(witnessPacket, packet);
                }
                else
                {
                    throw new NoPacketObtainedException(witnessPacket.ReferencedLedgerType, witnessPacket.CombinedBlockHeight, witnessPacket.ReferencedBodyHash.Hash);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure during obtaining and storing Transactional packet from URL {url}", ex);
                throw;
            }
        }

        protected abstract void StorePacket(WitnessPacket wp, IPacketBase packet);
    }
}
