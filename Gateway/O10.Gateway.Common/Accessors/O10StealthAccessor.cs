using Flurl;
using Flurl.Http;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Gateway.Common.Configuration;
using O10.Gateway.DataLayer.Services;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using O10.Crypto.Models;
using FizzWare.NBuilder;
using O10.Core.HashCalculations;
using O10.Core.ExtensionMethods;

namespace O10.Gateway.Common.Accessors
{
    /// <summary>
    /// Obtains Stealth packets using their Hash
    /// </summary>
    [RegisterExtension(typeof(IAccessor), Lifetime = LifetimeManagement.Singleton)]
    public class O10StealthAccessor : AccessorBase
    {
        public const string AggregatedTransactionsHeight = "AggregatedTransactionsHeight";

        private readonly IHashCalculation _hashCalculation;

        public static readonly ReadOnlyCollection<string> AccessingKeys 
            = new ReadOnlyCollection<string>(new[] { AggregatedTransactionsHeight, EvidenceDescriptor.TRANSACTION_HASH });

        private readonly IDataAccessService _dataAccessService;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly ILogger _logger;

        public O10StealthAccessor(IDataAccessService dataAccessService,
                                  IConfigurationService configurationService,
                                  IHashCalculationsRepository hashCalculationsRepository,
                                  ILoggerService loggerService)
        {
            _dataAccessService = dataAccessService;
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
            _logger = loggerService.GetLogger(nameof(O10StealthAccessor));
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        protected override IEnumerable<string> GetAccessingKeys() => AccessingKeys;

        protected override async Task<TransactionBase> GetTransactionInner(EvidenceDescriptor evidence)
        {
            if (evidence is null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            Url url = _synchronizerConfiguration.NodeApiUri
                .AppendPathSegments("Ledger", LedgerType, "Transaction")
                .SetQueryParam("hash", evidence[EvidenceDescriptor.TRANSACTION_HASH]);

            if (evidence.Parameters.ContainsKey(AggregatedTransactionsHeight))
            {
                url = url.SetQueryParam("combinedBlockHeight", evidence[AggregatedTransactionsHeight]);
            }

            _logger.Info($"Querying transaction by the URI {url}");
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
