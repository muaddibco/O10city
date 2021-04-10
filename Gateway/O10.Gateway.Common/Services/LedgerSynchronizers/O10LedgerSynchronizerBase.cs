using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Core.Serialization;
using O10.Crypto.Models;
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

        public abstract TransactionBase GetByWitness(WitnessPacket witnessPacket);

        public virtual async Task SyncByWitness(WitnessPacket witnessPacket)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            Url url = _synchronizerConfiguration.NodeApiUri.AppendPathSegments("Ledger", witnessPacket.ReferencedLedgerType, "Transaction").SetQueryParam("combinedBlockHeight", witnessPacket.CombinedBlockHeight).SetQueryParam("hash", witnessPacket.ReferencedBodyHash.Hash);
            _logger.Info($"Querying transaction by the URI {url}");

            try
            {
                var transaction = await url.GetJsonAsync<TransactionBase>().ConfigureAwait(false);
                _logger.LogIfDebug(() => $"Transaction obtained from URI {url}: {JsonConvert.SerializeObject(transaction, new ByteArrayJsonConverter())}");

                if (transaction != null)
                {
                    StorePacket(witnessPacket, transaction);
                }
                else
                {
                    throw new NoPacketObtainedException(witnessPacket.ReferencedLedgerType, witnessPacket.CombinedBlockHeight, witnessPacket.ReferencedBodyHash.Hash);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure during obtaining and storing State Transaction from URL {url}", ex);
                throw;
            }
        }

        protected abstract void StorePacket(WitnessPacket wp, TransactionBase transaction);
    }
}
