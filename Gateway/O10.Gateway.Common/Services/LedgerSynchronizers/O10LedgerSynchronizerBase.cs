using Newtonsoft.Json;
using O10.Core.Logging;
using O10.Core.Serialization;
using O10.Core.Translators;
using O10.Crypto.Models;
using O10.Gateway.Common.Exceptions;
using O10.Gateway.DataLayer.Model;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using System;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    public abstract class O10LedgerSynchronizerBase : ILedgerSynchronizer
    {
        private readonly ILogger _logger;
        private readonly IAccessorProvider _accessorProvider;
        private readonly ITranslatorsRepository _translatorsRepository;

        public O10LedgerSynchronizerBase(IAccessorProvider accessorProvider,
                                         ITranslatorsRepository translatorsRepository,
                                         ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(GetType().Name);
            _accessorProvider = accessorProvider;
            _translatorsRepository = translatorsRepository;
        }

        public abstract LedgerType LedgerType { get; }

        public abstract TransactionBase GetByWitness(WitnessPacket witnessPacket);

        public virtual async Task SyncByWitness(WitnessPacket witnessPacket, RegisterTransaction registerTransaction)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            if (registerTransaction is null)
            {
                throw new ArgumentNullException(nameof(registerTransaction));
            }

            try
            {
                var accessor = _accessorProvider.GetInstance(registerTransaction.ReferencedLedgerType);
                var translator = _translatorsRepository.GetInstance<RegisterTransaction, EvidenceDescriptor>();
                var evidence = translator.Translate(registerTransaction);
                var transaction = await accessor.GetTransaction<TransactionBase>(evidence).ConfigureAwait(false);
                _logger.LogIfDebug(() => $"Transaction obtained: {JsonConvert.SerializeObject(transaction, new ByteArrayJsonConverter())}");

                if (transaction != null)
                {
                    StoreTransaction(witnessPacket, transaction);
                }
                else
                {
                    throw new NoPacketObtainedException(witnessPacket.ReferencedLedgerType, witnessPacket.CombinedBlockHeight, witnessPacket.ReferencedBodyHash.Hash);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure during obtaining and storing transaction", ex);
                throw;
            }
        }

        protected abstract void StoreTransaction(WitnessPacket wp, TransactionBase transaction);
    }
}
