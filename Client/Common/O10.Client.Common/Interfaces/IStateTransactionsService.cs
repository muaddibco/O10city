using O10.Client.Common.Entities;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IStateTransactionsService : ITransactionsService
    {
        Task Initialize(long accountId);

        Task<RelationTransaction> IssueRelationRecordTransaction(IKey registrationCommitment);
        Task<IssueBlindedAssetTransaction> IssueBlindedAsset(byte[] assetId);
        Task<IssueBlindedAssetTransaction> IssueBlindedAsset2(byte[] assetId, byte[] blindingFactor);

        /*        Task<CancelEmployeeRecord> IssueCancelEmployeeRecord(byte[] registrationCommitment);

                Task<DocumentSignRecord> IssueDocumentSignRecord(byte[] documentHash, ulong recordHeight, byte[] keyImage, byte[] signerCommitment, SurjectionProof eligibilityProof, byte[] issuer, SurjectionProof signerGroupRelationProof, byte[] signerGroupCommitment, byte[] groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof);

                Task<DocumentRecord> IssueDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments);


        */
        /// <summary>
        /// originatingCommitment = blindingPointValue + assetId * G
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="groupId"></param>
        /// <param name="blindingPointValue"></param>
        /// <param name="blindingPointRoot"></param>
        /// <param name="originatingCommitment"></param>
        /// <returns></returns>
        Task<IssueAssociatedBlindedAssetTransaction> IssueAssociatedAsset(byte[] assetId, byte[] blindingPointValue, byte[] blindingPointRoot);

        Task<TransferAssetToStealthTransaction> TransferAssetToStealth(byte[] assetId, ConfidentialAccount receiver);

        Task<TransferAssetToStealthTransaction> TransferAssetToStealth2(byte[] assetId, byte[] issuanceCommitment, ConfidentialAccount receiver);

    }
}