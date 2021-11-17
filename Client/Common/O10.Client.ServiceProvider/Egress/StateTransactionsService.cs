using System;
using System.Collections.Generic;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using System.Threading.Tasks;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using O10.Crypto.Models;

namespace O10.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(IServiceProviderTransactionsService), Lifetime = LifetimeManagement.Scoped)]
    public class ServiceProviderTransactionsService : TransactionsServiceBase, IServiceProviderTransactionsService
    {
        private readonly IStateClientCryptoService _clientCryptoService;
        private long _lastHeight;

        public ServiceProviderTransactionsService(
            IHashCalculationsRepository hashCalculationsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            IStateClientCryptoService clientCryptoService,
            IGatewayService gatewayService,
            ILoggerService loggerService)
            : base(
                  hashCalculationsRepository,
                  identityKeyProvidersRegistry,
                  clientCryptoService,
                  gatewayService,
                  loggerService)
        {
            _clientCryptoService = clientCryptoService;
        }

        #region ============ PUBLIC FUNCTIONS =============  

        public async Task Initialize(long accountId)
        {
            _accountId = accountId;
            long lastBlockHeight = (await _gatewayService.GetLastPacketInfo(_clientCryptoService.GetPublicKey()).ConfigureAwait(false)).Height;
            _lastHeight = lastBlockHeight + 1;
        }

        public async Task<DocumentRecordTransaction?> IssueDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments)
        {
            var packet = CreateDocumentRecord(documentHash, allowedSignerCommitments);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<DocumentSignTransaction> IssueDocumentSignRecord(byte[] documentHash, IKey keyImage, IKey signerCommitment, SurjectionProof eligibilityProof, IKey issuer, SurjectionProof signerGroupRelationProof, IKey signerGroupCommitment, IKey groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof)
        {
            var packet = CreateDocumentSignRecord(documentHash, keyImage, signerCommitment, eligibilityProof, issuer, signerGroupRelationProof, signerGroupCommitment, groupIssuer, signerGroupProof, signerAllowedGroupsProof);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        /*        

                public async Task<CancelEmployeeRecord> IssueCancelEmployeeRecord(byte[] registrationCommitment)
                {
                    CancelEmployeeRecord packet = CreateCancelEmployeeRecord(registrationCommitment);

                    var completionResult = PropagateTransaction(packet);

                    return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
                }*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="registrationCommitment">commitment that already contains non-blinded commitment to group assetId</param>
        /// <returns></returns>
        public async Task<RelationTransaction?> IssueRelationRecordTransaction(IKey registrationCommitment)
        {
            var packet = CreateRelationRecord(registrationCommitment);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 

        private DocumentRecordTransaction CreateDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments)
        {
            var transaction = new DocumentRecordTransaction()
            {
                DocumentHash = documentHash,
                AllowedSignerGroupCommitments = allowedSignerCommitments ?? Array.Empty<byte[]>()
            };

            return transaction;
        }

        private DocumentSignTransaction CreateDocumentSignRecord(byte[] documentTransactionHash, IKey keyImage, IKey signerCommitment, SurjectionProof eligibilityProof, IKey issuer, SurjectionProof signerGroupRelationProof, IKey signerGroupCommitment, IKey groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof)
        {
            var transaction = new DocumentSignTransaction
            {
                DocumentTransactionHash = documentTransactionHash,
                KeyImage = keyImage,
                SignerCommitment = signerCommitment,
                EligibilityProof = eligibilityProof,
                Issuer = issuer,
                SignerGroupRelationProof = signerGroupRelationProof,
                SignerGroupCommitment = signerGroupCommitment,
                GroupIssuer = groupIssuer,
                SignerGroupProof = signerGroupProof,
                SignerAllowedGroupsProof = signerAllowedGroupsProof
            };

            return transaction;
        }

        /* 


         private CancelEmployeeRecord CreateCancelEmployeeRecord(byte[] registrationCommitment)
         {
             CancelEmployeeRecord issueEmployeeRecord = new CancelEmployeeRecord
             {
                 RegistrationCommitment = registrationCommitment
             };

             FillHeightInfo(issueEmployeeRecord);
             FillSyncData(issueEmployeeRecord);
             FillAndSign(issueEmployeeRecord);

             return issueEmployeeRecord;
         }*/

        private RelationTransaction CreateRelationRecord(IKey registrationCommitment)
        {
            var issueEmployeeRecord = new RelationTransaction
            {
                RegistrationCommitment = registrationCommitment
            };

            return issueEmployeeRecord;
        }

        #endregion

    }
}
