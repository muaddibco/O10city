using FizzWare.NBuilder;
using Flurl;
using Flurl.Http;
using O10.Core;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Crypto.Models;
using O10.Gateway.Common.Configuration;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Accessors
{
    public class O10StateAccessor : AccessorBase
    {
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly IHashCalculation _hashCalculation;
        
        public const string AggregatedTransactionsHeight = "AggregatedTransactionsHeight";
        public static readonly ReadOnlyCollection<string> AccessingKeys 
            = new ReadOnlyCollection<string>(new[] { AggregatedTransactionsHeight, EvidenceDescriptor.TRANSACTION_HASH });

        public O10StateAccessor(IHashCalculationsRepository hashCalculationsRepository, IConfigurationService configurationService)
        {
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
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
            
            var transaction = await url.GetJsonAsync<TransactionBase>().ConfigureAwait(false);

            return transaction;
        }

        public override EvidenceDescriptor GetEvidence(TransactionBase transaction)
            => Builder<EvidenceDescriptor>
                .CreateNew()
                    .With(s => s.ActionType = transaction.TransactionType)
                    .With(s => s.LedgerType = LedgerType)
                    .Do(s => s.Parameters.Add(EvidenceDescriptor.TRANSACTION_HASH, _hashCalculation.CalculateHash(transaction.ToString()).ToHexString()))
                .Build();
    }
}
