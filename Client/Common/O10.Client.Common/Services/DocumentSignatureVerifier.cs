using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Outputs;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Serialization;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using O10.Core.Identity;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IDocumentSignatureVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class DocumentSignatureVerifier : IDocumentSignatureVerifier
    {
        private readonly IGatewayService _gatewayService;
        private readonly ILogger _logger;

        public DocumentSignatureVerifier(IGatewayService gatewayService, ILoggerService loggerService)
        {
            _gatewayService = gatewayService;
            _logger = loggerService.GetLogger(nameof(DocumentSignatureVerifier));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentCreator"></param>
        /// <param name="documentHash"></param>
        /// <param name="documentRecordHeight"></param>
        /// <param name="signatureTransactionHash">hash of the transaction with signature that the document owner stored into a ledger</param>
        /// <returns></returns>
        public async Task<DocumentSignatureVerification> Verify(byte[] documentCreator, byte[] documentHash, byte[] documentRecordTransactionHash, byte[] signatureTransactionHash)
        {
            DocumentSignatureVerification res = new DocumentSignatureVerification();

            var packetInfoDocumentSignRecord = await _gatewayService.GetTransaction(documentCreator.ToHexString(), signatureTransactionHash).ConfigureAwait(false);
            var packetInfoDocumentRecord = await _gatewayService.GetTransaction(documentCreator.ToHexString(), documentRecordTransactionHash).ConfigureAwait(false);


            DocumentSignTransaction documentSignRecord = packetInfoDocumentSignRecord as DocumentSignTransaction;


            res.SignatureTransactionFound = documentSignRecord != null;
            res.DocumentRecordTransactionFound = packetInfoDocumentRecord is DocumentRecordTransaction;

            if (res.SignatureTransactionFound && res.DocumentRecordTransactionFound)
            {
                ulong combinedBlockHeight = await _gatewayService.GetCombinedBlockByTransactionHash(documentCreator, signatureTransactionHash).ConfigureAwait(false);
                res.IsNotCompromised = !(await _gatewayService.IsKeyImageCompromised(documentSignRecord.KeyImage).ConfigureAwait(false));
                res.DocumentHashMatch = documentSignRecord.DocumentTransactionHash.Equals(documentHash);
                res.SignerSignatureMatch = CryptoHelper.VerifySurjectionProof(documentSignRecord.SignerGroupRelationProof, documentSignRecord.SignerCommitment.Value.Span, documentHash, documentRecordTransactionHash);
                res.EligibilityCorrect = await CheckEligibilityProofsWereValid(documentSignRecord.SignerCommitment.Value, documentSignRecord.EligibilityProof, documentSignRecord.Issuer, combinedBlockHeight).ConfigureAwait(false);
                res.AllowedGroupRelation = CryptoHelper.VerifySurjectionProof(documentSignRecord.SignerGroupProof, documentSignRecord.SignerGroupCommitment.Value.Span);
                res.AllowedGroupMatching = CryptoHelper.VerifySurjectionProof(documentSignRecord.SignerAllowedGroupsProof, documentSignRecord.SignerGroupCommitment.Value.Span);

                //PacketInfoEx packetInfo = _gatewayService.GetTransactionBySourceAndHeight(documentSignRecord.GroupIssuer.ToHexString(), signatureRecordBlockHeight);

            }

            return res;
        }

        private async Task<bool> CheckEligibilityProofs(byte[] assetCommitment, SurjectionProof eligibilityProofs, byte[] issuer)
        {
            _logger.LogIfDebug(() => $"{nameof(CheckEligibilityProofs)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer.ToHexString()}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");
            bool isCommitmentCorrect = CryptoHelper.VerifySurjectionProof(eligibilityProofs, assetCommitment);

            if (!isCommitmentCorrect)
            {
                return false;
            }

            foreach (byte[] commitment in eligibilityProofs.AssetCommitments)
            {
                //TODO: make bulk check!
                if (!await _gatewayService.IsRootAttributeValid(issuer, commitment).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CheckEligibilityProofsWereValid(Memory<byte> assetCommitment, SurjectionProof eligibilityProofs, IKey issuer, ulong combinedBlockHeight)
        {
            _logger.LogIfDebug(() => $"{nameof(CheckEligibilityProofsWereValid)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer}, combinedBlockHeight={combinedBlockHeight}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");
            bool isCommitmentCorrect = CryptoHelper.VerifySurjectionProof(eligibilityProofs, assetCommitment.Span);

            if (!isCommitmentCorrect)
            {
                return false;
            }

            foreach (byte[] commitment in eligibilityProofs.AssetCommitments)
            {
                //TODO: make bulk check!
                if (!await _gatewayService.WasRootAttributeValid(issuer, commitment, (long)combinedBlockHeight).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
