using O10.Transactions.Core.Ledgers.Stealth;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

using O10.Crypto.ConfidentialAssets;
using O10.Core.ExtensionMethods;
using O10.Client.Common.Interfaces.Inputs;
using System;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.AttributesScheme;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Cryptography;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Core.Configuration;
using O10.Client.Common.Configuration;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IRelationsProofsValidationService), Lifetime = LifetimeManagement.Singleton)]
    public class RelationsProofsValidationService : IRelationsProofsValidationService
    {
        private readonly IGatewayService _gatewayService;
        private readonly IAssetsService _assetsService;
		private readonly IRestApiConfiguration _restApiConfiguration;
		private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IDataAccessService _dataAccessService;
		private readonly ILogger _logger;

        public RelationsProofsValidationService(IGatewayService gatewayService, IAssetsService assetsService, 
            IIdentityAttributesService identityAttributesService, IConfigurationService configurationService,
            IDataAccessService dataAccessService, ILoggerService loggerService)
        {
            _gatewayService = gatewayService;
            _assetsService = assetsService;
			_identityAttributesService = identityAttributesService;
            _dataAccessService = dataAccessService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
			_logger = loggerService.GetLogger(nameof(RelationsProofsValidationService));
		}

		public async Task<RelationProofsValidationResults> VerifyRelationProofs(GroupsRelationsProofs relationsProofs, IStealthClientCryptoService clientCryptoService, RelationProofsSession proofSession)
        {
            RelationProofsValidationResults validationResults = new RelationProofsValidationResults();

            clientCryptoService.DecodeEcdhTuple(relationsProofs.EcdhTuple, relationsProofs.TransactionPublicKey, out byte[] mask, out byte[] imageHash, out byte[] issuer, out byte[] sessionKey);

			byte[] imageHashFromSession;

			if (!string.IsNullOrEmpty(proofSession.ProofsData.ImageContent))
			{
				byte[] image = Convert.FromBase64String(proofSession.ProofsData.ImageContent);
				validationResults.ImageContent = proofSession.ProofsData.ImageContent;
				imageHashFromSession = O10.Crypto.ConfidentialAssets.CryptoHelper.FastHash256(image);
			}
			else
			{
				imageHashFromSession = new byte[Globals.DEFAULT_HASH_SIZE];
			}

            validationResults.IsImageCorrect = imageHashFromSession.Equals32(imageHash);

			validationResults.IsEligibilityCorrect = await CheckEligibilityProofs(relationsProofs.AssetCommitment, relationsProofs.EligibilityProof, issuer).ConfigureAwait(false);

			if (proofSession.ProofsRequest.WithKnowledgeProof)
			{
				validationResults.IsKnowledgeFactorCorrect = await CheckKnowledgeFactor(relationsProofs.AssetCommitment, relationsProofs.AssociatedProofs, issuer.ToHexString()).ConfigureAwait(false);
			}
			else
			{
				validationResults.IsKnowledgeFactorCorrect = true;
			}

            foreach (var relationEntry in proofSession.ProofsData.RelationEntries)
            {
                bool isRelationContentMatching = false;

                foreach (var relationProof in relationsProofs.RelationProofs)
                {
                    byte[] registrationCommitment = relationProof.RelationProof.AssetCommitments[0];
                    byte[] groupNameCommitment = await _gatewayService.GetEmployeeRecordGroup(relationProof.GroupOwner, registrationCommitment).ConfigureAwait(false);
                    bool isRelationProofCorrect = groupNameCommitment != null ? O10.Crypto.ConfidentialAssets.CryptoHelper.VerifySurjectionProof(relationProof.RelationProof, relationsProofs.AssetCommitment) : false;

                    if (isRelationProofCorrect)
                    {
                        byte[] relationAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMPLOYEEGROUP, relationProof.GroupOwner.ToHexString() + relationEntry.RelatedAssetName, relationProof.GroupOwner.ToHexString()).ConfigureAwait(false);
                        if (O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyIssuanceSurjectionProof(relationProof.GroupNameProof, groupNameCommitment, new byte[][] { relationAssetId }))
                        {
                            isRelationContentMatching = true;
                            break;
                        }
                    }
                }

                validationResults.ValidationResults.Add(new RelationProofValidationResult { RelatedAttributeOwner = relationEntry.RelatedAssetOwnerName, RelatedAttributeContent = relationEntry.RelatedAssetName, IsRelationCorrect = isRelationContentMatching });
            }

            return validationResults;
        }

		private async Task<bool> CheckEligibilityProofs(byte[] assetCommitment, SurjectionProof eligibilityProofs, byte[] issuer)
		{
			_logger.LogIfDebug(() => $"{nameof(CheckEligibilityProofs)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer.ToHexString()}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");

			bool isCommitmentCorrect = O10.Crypto.ConfidentialAssets.CryptoHelper.VerifySurjectionProof(eligibilityProofs, assetCommitment);

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

		private async Task<bool> CheckKnowledgeFactor(byte[] commitment, AssociatedProofs[] associatedProofsList, string issuer)
		{
			byte[] groupId = await _identityAttributesService.GetGroupId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, issuer).ConfigureAwait(false);
			AssociatedProofs associatedProofs = Array.Find(associatedProofsList, p => p.SchemeName.Equals32(groupId));

			if(associatedProofs == null)
			{
				return false;
			}
			bool associatedProofValid;

			if (associatedProofs is AssociatedAssetProofs associatedAssetProofs)
			{
				associatedProofValid = O10.Crypto.ConfidentialAssets.CryptoHelper.VerifySurjectionProof(associatedAssetProofs.AssociationProofs, associatedAssetProofs.AssociatedAssetCommitment);
			}
			else
			{
                return false;
			}

			bool rootProofValid = O10.Crypto.ConfidentialAssets.CryptoHelper.VerifySurjectionProof(associatedProofs.RootProofs, commitment);

			if (!rootProofValid || !associatedProofValid)
			{
				return false;
			}

			//TODO: !!! adjust checking either against Gateway or against local database
			bool found = true; // associatedProofs.AssociationProofs.AssetCommitments.Any(a => associatedProofs.RootProofs.AssetCommitments.Any(r => _dataAccessService.CheckAssociatedAtributeExist(null, a, r)));

            found = _dataAccessService.CheckAttributeSchemeToCommitmentMatching(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, associatedAssetProofs.AssociationProofs.AssetCommitments[0]);

			if (!found)
			{
				return false;
			}

			return true;
		}
	}
}
