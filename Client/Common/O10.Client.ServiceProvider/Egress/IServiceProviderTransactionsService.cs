using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IServiceProviderTransactionsService : ITransactionsService
    {
        Task Initialize(long accountId);

        Task<DocumentRecordTransaction?> IssueDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments);
        Task<DocumentSignTransaction> IssueDocumentSignRecord(byte[] documentHash, IKey keyImage, IKey signerCommitment, SurjectionProof eligibilityProof, IKey issuer, SurjectionProof signerGroupRelationProof, IKey signerGroupCommitment, IKey groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registrationCommitment">commitment that already contains non-blinded commitment to group assetId</param>
        /// <returns></returns>
        Task<RelationTransaction?> IssueRelationRecordTransaction(IKey registrationCommitment);

        //Task<CancelEmployeeRecord> IssueCancelEmployeeRecord(byte[] registrationCommitment);
    }
}