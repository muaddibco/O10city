using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Parsers;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Outputs;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Serialization;

namespace O10.Client.Common.Services
{
	[RegisterDefaultImplementation(typeof(IDocumentSignatureVerifier), Lifetime = LifetimeManagement.Singleton)]
	public class DocumentSignatureVerifier : IDocumentSignatureVerifier
	{
		private readonly IGatewayService _gatewayService;
		private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
		private readonly ILogger _logger;

		public DocumentSignatureVerifier(
			IGatewayService gatewayService, IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
			ILoggerService loggerService)
		{
			_gatewayService = gatewayService;
			_blockParsersRepositoriesRepository = blockParsersRepositoriesRepository;
			_logger = loggerService.GetLogger(nameof(DocumentSignatureVerifier));
		}

		public async Task<DocumentSignatureVerification> Verify(byte[] documentCreator, byte[] documentHash, ulong documentRecordHeight, ulong signatureRecordBlockHeight)
		{
			DocumentSignatureVerification res = new DocumentSignatureVerification();

			var packetInfoDocumentSignRecord = await _gatewayService.GetTransactionBySourceAndHeight(documentCreator.ToHexString(), signatureRecordBlockHeight).ConfigureAwait(false);
			var packetInfoDocumentRecord = await _gatewayService.GetTransactionBySourceAndHeight(documentCreator.ToHexString(), documentRecordHeight).ConfigureAwait(false);

			IBlockParsersRepository blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository(packetInfoDocumentSignRecord.LedgerType);
			IBlockParser blockParser = blockParsersRepository.GetInstance(packetInfoDocumentSignRecord.BlockType);

			DocumentSignRecord documentSignRecord = blockParser.Parse(packetInfoDocumentSignRecord.Content) as DocumentSignRecord;

			blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository(packetInfoDocumentRecord.LedgerType);
			blockParser = blockParsersRepository.GetInstance(packetInfoDocumentRecord.BlockType);

			res.SignatureTransactionFound = documentSignRecord != null;
			res.DocumentRecordTransactionFound = blockParser.Parse(packetInfoDocumentRecord.Content) is DocumentRecord;

			if(res.SignatureTransactionFound && res.DocumentRecordTransactionFound)
			{
				ulong combinedBlockHeight = await _gatewayService.GetCombinedBlockByAccountHeight(documentCreator, signatureRecordBlockHeight).ConfigureAwait(false);
                res.IsNotCompromised = !(await _gatewayService.IsKeyImageCompromised(documentSignRecord.KeyImage).ConfigureAwait(false));
				res.DocumentHashMatch = documentSignRecord.DocumentHash.Equals32(documentHash);
				res.SignerSignatureMatch = ConfidentialAssetsHelper.VerifySurjectionProof(documentSignRecord.SignerGroupRelationProof, documentSignRecord.SignerCommitment, documentHash, BitConverter.GetBytes(documentRecordHeight));
				res.EligibilityCorrect = await CheckEligibilityProofsWereValid(documentSignRecord.SignerCommitment, documentSignRecord.EligibilityProof, documentSignRecord.Issuer, combinedBlockHeight).ConfigureAwait(false);
				res.AllowedGroupRelation = ConfidentialAssetsHelper.VerifySurjectionProof(documentSignRecord.SignerGroupProof, documentSignRecord.SignerGroupCommitment);
				res.AllowedGroupMatching = ConfidentialAssetsHelper.VerifySurjectionProof(documentSignRecord.SignerAllowedGroupsProof, documentSignRecord.SignerGroupCommitment);

				//PacketInfoEx packetInfo = _gatewayService.GetTransactionBySourceAndHeight(documentSignRecord.GroupIssuer.ToHexString(), signatureRecordBlockHeight);

			}

			return res;
		}

		private async Task<bool> CheckEligibilityProofs(byte[] assetCommitment, SurjectionProof eligibilityProofs, byte[] issuer)
		{
			_logger.LogIfDebug(() => $"{nameof(CheckEligibilityProofs)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer.ToHexString()}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");
			bool isCommitmentCorrect = ConfidentialAssetsHelper.VerifySurjectionProof(eligibilityProofs, assetCommitment);

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

		private async Task<bool> CheckEligibilityProofsWereValid(byte[] assetCommitment, SurjectionProof eligibilityProofs, byte[] issuer, ulong combinedBlockHeight)
		{
			_logger.LogIfDebug(() => $"{nameof(CheckEligibilityProofsWereValid)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer.ToHexString()}, combinedBlockHeight={combinedBlockHeight}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");
			bool isCommitmentCorrect = ConfidentialAssetsHelper.VerifySurjectionProof(eligibilityProofs, assetCommitment);

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
