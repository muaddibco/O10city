using System.Collections.Generic;
using System.Threading;
using O10.Client.Common.Interfaces.Inputs;
using O10.Core.Architecture;
using O10.Client.Web.DataContracts.ServiceProvider;
using O10.Client.Common.Interfaces;

namespace O10.Client.Web.Portal.Services
{
    [ServiceContract]
    public interface IConsentManagementService : IUpdater
    {
        string PublicSpendKey { get; }
        string PublicViewKey { get; }

        void Initialize(IExecutionContextManager executionContextManager, CancellationToken cancellationToken);

        bool PushRelationProofsData(string sessionKey, RelationProofsData relationProofSession);

        RelationProofsSession PopRelationProofSession(string sessionKey);

        string InitiateRelationProofsSession(ProofsRequest proofsRequest);

        IEnumerable<SpUserTransactionDto> GetUserTransactions(long spAccountId);

        void RegisterTransactionForConsent(TransactionConsentRequest consentRequest);

        IEnumerable<TransactionConsentRequest> GetTransactionConsentRequests(string registrationCommitment);

        bool TryGetTransactionRequestByKey(string key, string keyImage, out string transactionId, out bool confirmed);

        string GetTransactionRequestByKeyImage(string keyImage);
    }
}
