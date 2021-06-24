using Newtonsoft.Json;
using O10.Core;
using O10.Core.HashCalculations;
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
        private readonly IAccessorProvider _accessorProvider;
        private readonly ITranslatorsRepository _translatorsRepository;

        public O10LedgerSynchronizerBase(IAccessorProvider accessorProvider,
                                         ITranslatorsRepository translatorsRepository,
                                         IHashCalculationsRepository hashCalculationsRepository,
                                         ILoggerService loggerService)
        {
            Logger = loggerService.GetLogger(GetType().Name);
            _accessorProvider = accessorProvider;
            _translatorsRepository = translatorsRepository;
			HashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public abstract LedgerType LedgerType { get; }

        public IHashCalculation HashCalculation { get; }

        protected ILogger Logger { get; }

        public abstract TransactionBase? GetByWitness(WitnessPacket witnessPacket);

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

            TransactionBase transaction = null;

            try
            {
                var translator = _translatorsRepository.GetInstance<RegisterTransaction, EvidenceDescriptor>();
                var evidence = translator.Translate(registerTransaction);
                var accessor = _accessorProvider.GetInstance(registerTransaction.ReferencedLedgerType);
                transaction = await accessor.GetTransaction<TransactionBase>(evidence).ConfigureAwait(false);
                Logger.LogIfDebug(() => $"Transaction obtained: {JsonConvert.SerializeObject(transaction, new ByteArrayJsonConverter())}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failure during obtaining transaction for storing by RegisterTransaction:\r\n{JsonConvert.SerializeObject(registerTransaction)}", ex);
                throw;
            }

            try
            {

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
                Logger.Error($"Failure during storing transaction {transaction.GetType().Name}:\r\n{JsonConvert.SerializeObject(transaction)}", ex);
                throw;
            }
        }

        protected abstract void StoreTransaction(WitnessPacket wp, TransactionBase transaction);
    }
}
