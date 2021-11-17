using O10.Client.Common.Dtos;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IIdentityProviderTransactionsService : ITransactionsService
    {
        Task Initialize(long accountId);

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

        Task<TransferAssetToStealthTransaction?> TransferAssetToStealth(byte[] assetId, ConfidentialAccountDTO receiver);

        Task<TransferAssetToStealthTransaction?> TransferAssetToStealth2(byte[] assetId, byte[] issuanceCommitment, ConfidentialAccountDTO receiver);
        Task<IssueBlindedAssetTransaction?> IssueBlindedAsset(byte[] assetId);
        Task<IssueBlindedAssetTransaction?> IssueBlindedAsset2(byte[] assetId, byte[] blindingFactor);
    }
}