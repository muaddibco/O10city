using FizzWare.NBuilder;
using Flurl;
using Flurl.Http;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Crypto.Models;
using O10.Gateway.Common.Configuration;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Accessors
{
    [RegisterExtension(typeof(IAccessor), Lifetime = LifetimeManagement.Singleton)]
    public class O10StateAccessor : AccessorGwBase
    {
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        
        public const string AggregatedTransactionsHeight = "AggregatedTransactionsHeight";
        public static readonly ReadOnlyCollection<string> AccessingKeys 
            = new ReadOnlyCollection<string>(new[] { AggregatedTransactionsHeight, EvidenceDescriptor.TRANSACTION_HASH });

        public O10StateAccessor(IHashCalculationsRepository hashCalculationsRepository, IConfigurationService configurationService, ILoggerService loggerService)
        {
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(O10StateAccessor));
        }

        public override LedgerType LedgerType => LedgerType.O10State;

        protected override IEnumerable<string> GetAccessingKeys()
        {
            throw new NotImplementedException();
        }

        protected override async Task<TransactionBase> GetTransactionInner(EvidenceDescriptor evidence)
        {
            if (evidence is null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            Url url = _synchronizerConfiguration.NodeApiUri
                .AppendPathSegments("Ledger", LedgerType, "Transaction")
                .SetQueryParam("hash", evidence[EvidenceDescriptor.TRANSACTION_HASH]);

            if(evidence.Parameters.ContainsKey(AggregatedTransactionsHeight))
            {
                url = url.SetQueryParam("combinedBlockHeight", evidence[AggregatedTransactionsHeight]);
            }

            _logger.Info($"Querying transaction by the URI {url}");
            var transaction = await url.GetJsonAsync<TransactionBase>().ConfigureAwait(false);

            return transaction;
        }

        public override EvidenceDescriptor GetEvidence(TransactionBase transaction) 
            => transaction is O10StateTransitionalTransactionBase o10StateTransitionalTransaction
                ? ProduceEvidenceForTransitional(o10StateTransitionalTransaction)
                : ProduceEvidenceForBasic(transaction);


        private EvidenceDescriptor ProduceEvidenceForTransitional(O10StateTransitionalTransactionBase transaction)
            => Builder<EvidenceDescriptor>
                .CreateNew()
                    .With(s => s.ActionType = transaction.TransactionType)
                    .With(s => s.LedgerType = LedgerType)
                    .Do(s => s.Parameters.Add(EvidenceDescriptor.TRANSACTION_HASH, _hashCalculation.CalculateHash(transaction.ToString()).ToHexString()))
                    .Do(s => s.Parameters.Add(EvidenceDescriptor.REFERENCED_TARGET, transaction.DestinationKey.ToString()))
                    .Do(s => s.Parameters.Add(EvidenceDescriptor.REFERENCED_TRANSACTION_KEY, transaction.TransactionPublicKey.ToString()))
                .Build();

        private EvidenceDescriptor ProduceEvidenceForBasic(TransactionBase transaction)
            => Builder<EvidenceDescriptor>
                .CreateNew()
                    .With(s => s.ActionType = transaction.TransactionType)
                    .With(s => s.LedgerType = LedgerType)
                    .Do(s => s.Parameters.Add(EvidenceDescriptor.TRANSACTION_HASH, _hashCalculation.CalculateHash(transaction.ToString()).ToHexString()))
                .Build();
    }
}
