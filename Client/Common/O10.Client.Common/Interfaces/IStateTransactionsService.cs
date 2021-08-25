using O10.Client.Common.Entities;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IStateTransactionsService : ITransactionsService
    {
        Task Initialize(long accountId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registrationCommitment">commitment that already contains non-blinded commitment to group assetId</param>
        /// <returns></returns>
        Task<RelationTransaction?> IssueRelationRecordTransaction(IKey registrationCommitment);
        Task<IssueBlindedAssetTransaction?> IssueBlindedAsset(byte[] assetId);
        Task<IssueBlindedAssetTransaction?> IssueBlindedAsset2(byte[] assetId, byte[] blindingFactor);
        Task<DocumentRecordTransaction?> IssueDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments);
        Task<DocumentSignTransaction> IssueDocumentSignRecord(byte[] documentHash, IKey keyImage, IKey signerCommitment, SurjectionProof eligibilityProof, IKey issuer, SurjectionProof signerGroupRelationProof, IKey signerGroupCommitment, IKey groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof);

        /*        Task<CancelEmployeeRecord> IssueCancelEmployeeRecord(byte[] registrationCommitment);




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
        Task<IssueAssociatedBlindedAssetTransaction?> IssueAssociatedAsset(byte[] assetId, byte[] blindingPointValue, byte[] blindingPointRoot);

        Task<TransferAssetToStealthTransaction?> TransferAssetToStealth(byte[] assetId, ConfidentialAccount receiver);

        Task<TransferAssetToStealthTransaction?> TransferAssetToStealth2(byte[] assetId, byte[] issuanceCommitment, ConfidentialAccount receiver);

    }
}