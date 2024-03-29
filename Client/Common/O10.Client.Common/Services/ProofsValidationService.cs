﻿using O10.Client.Common.Dtos;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IProofsValidationService), Lifetime = LifetimeManagement.Singleton)]
    public class ProofsValidationService : IProofsValidationService
    {
        private readonly IGatewayService _gatewayService;
        private readonly IAssetsService _assetsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly ILogger _logger;

        public ProofsValidationService(
            IGatewayService gatewayService,
            IAssetsService assetsService,
            ILoggerService loggerService,
            IDataAccessService dataAccessService)
        {
            _gatewayService = gatewayService;
            _assetsService = assetsService;
            _dataAccessService = dataAccessService;
            _logger = loggerService.GetLogger(nameof(ProofsValidationService));
        }

        public async Task CheckEligibilityProofs(Memory<byte> assetCommitment, SurjectionProof eligibilityProofs, Memory<byte> issuer)
        {
            Contract.Requires(eligibilityProofs != null);

            bool isCommitmentCorrect = CryptoHelper.VerifySurjectionProof(eligibilityProofs, assetCommitment.Span);

            if (!isCommitmentCorrect)
            {
                throw new CommitmentNotEligibleException();
            }

            try
            {
                bool res = await _gatewayService.AreRootAttributesValid(issuer, eligibilityProofs.AssetCommitments.Select(a => new Memory<byte>(a))).ConfigureAwait(false);

                if (!res)
                {
                    throw new CommitmentNotEligibleException();
                }

            }
            catch (Exception ex)
            {
                throw new CommitmentNotEligibleException(ex);
            }
        }

        public (long registrationId, bool isNew) HandleRegistration(long accountId, Memory<byte> assetCommitment, SurjectionProof authenticationProof)
        {
            if (authenticationProof is null)
            {
                throw new ArgumentNullException(nameof(authenticationProof));
            }

            if (_dataAccessService.GetServiceProviderRegistrationId(accountId, authenticationProof.AssetCommitments[0], out long registrationId))
            {
                bool isAuthenticationProofValid = CryptoHelper.VerifySurjectionProof(authenticationProof, assetCommitment.Span);

                if (!isAuthenticationProofValid)
                {
                    throw new SpAuthenticationProofsFailedException();
                }

                return (registrationId, false);
            }
            else
            {
                long id = _dataAccessService.AddServiceProviderRegistration(accountId, authenticationProof.AssetCommitments[0]);
                return (id, true);
            }
        }

        public async Task<bool> CheckAssociatedProofs(RootIssuer rootIssuer, IEnumerable<ValidationCriteriaDTO> validationCriterias)
        {
            if (rootIssuer is null)
            {
                throw new ArgumentNullException(nameof(rootIssuer));
            }

            // 1. Need to validate that commitment provided in the Universal Proofs is really bounded to associated proofs
            if (validationCriterias?.Any(v => v.SchemeName != AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO) ?? false)
            {
                foreach (var validationCriteria in validationCriterias)
                {
                    await CheckAssociatedProof(rootIssuer, validationCriteria.SchemeName).ConfigureAwait(false);
                }
            }

            return true;
        }

        public async Task VerifyKnowledgeFactorProofs(List<RootIssuer> rootIssuers)
        {
            if (rootIssuers is null)
            {
                throw new ArgumentNullException(nameof(rootIssuers));
            }

            string schemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD;
            RootIssuer rootIssuer = rootIssuers.Find(
                    i => i.IssuersAttributes.Any(
                        a => a.Attributes.Any(
                            a1 => a1.SchemeName == schemeName)));

            if (rootIssuer is null)
            {
                throw new AssociatedAttrProofsAreMissingException(schemeName);
            }

            AttributeProofs attrProtection = rootIssuer.IssuersAttributes?.FirstOrDefault(a => a.Issuer.Equals(rootIssuer.Issuer))?.Attributes.FirstOrDefault(a => a.SchemeName == schemeName);

            if (attrProtection == null)
            {
                throw new AssociatedAttrProofsAreMissingException(schemeName);
            }

            if (attrProtection.CommitmentProof.SurjectionProof.AssetCommitments.Length != attrProtection.BindingProof.AssetCommitments.Length)
            {
                throw new AssociatedAttrProofsMalformedException(schemeName);
            }

            if (!CryptoHelper.VerifySurjectionProof(attrProtection.CommitmentProof.SurjectionProof, attrProtection.Commitment.ArraySegment.Array))
            {
                throw new AssociatedAttrProofToValueKnowledgeIncorrectException(schemeName);
            }

            IKey commitmentRoot = rootIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(rootIssuer.Issuer))?.RootAttribute.Commitment;
            byte[] commitment = CryptoHelper.SumCommitments(commitmentRoot.Value.Span, attrProtection.Commitment.Value.Span);
            if (!CryptoHelper.VerifySurjectionProof(attrProtection.BindingProof, commitment))
            {
                throw new AssociatedAttrProofToBindingIncorrectException(schemeName);
            }

            (Memory<byte> issuanceCommitment, Memory<byte> commitmentToRoot)[] attrs = new (Memory<byte>, Memory<byte>)[attrProtection.BindingProof.AssetCommitments.Length];
            for (int i = 0; i < attrProtection.BindingProof.AssetCommitments.Length; i++)
            {
                attrs[i].issuanceCommitment = attrProtection.CommitmentProof.SurjectionProof.AssetCommitments[i];
                attrs[i].commitmentToRoot = attrProtection.BindingProof.AssetCommitments[i];
            }

            bool res = await _gatewayService.AreAssociatedAttributesExist(rootIssuer.Issuer.Value, attrs).ConfigureAwait(false);
            if (!res)
            {
                throw new AssociatedProofsSourceNotFoundException(schemeName);
            }
        }

        #region Private Functions

        private async Task CheckAssociatedProof(RootIssuer rootIssuer, string schemeName)
        //Memory<byte> rootCommitment, IEnumerable<AttributesByIssuer> associatedAttributes, string schemeName)
        {
            var rootIssuerAttributes = rootIssuer.IssuersAttributes.FirstOrDefault(i => i.Issuer.Equals(rootIssuer.Issuer));

            AttributesByIssuer attributesForCheck = rootIssuer.IssuersAttributes.FirstOrDefault(c => c.RootAttribute.SchemeName == schemeName || c.Attributes.Any(a => a.SchemeName == schemeName));
            if (attributesForCheck == null)
            {
                throw new ValidationProofsWereNotCompleteException(schemeName);
            }

            if (attributesForCheck.RootAttribute == null)
            {
                throw new ValidationProofsWereNotCompleteException(schemeName);
            }

            if (attributesForCheck.Issuer.Equals(rootIssuer.Issuer))
            {
                // when the found attribute resides at the Root Identity need to prove 
                // binding of the associated attribute to the root attribute

                bool proofToRoot = CryptoHelper.VerifySurjectionProof(attributesForCheck.RootAttribute.BindingProof, attributesForCheck.RootAttribute.Commitment.Value.Span);
                if (!proofToRoot)
                {
                    _logger.Error("Proof of binding to Issuance failed");
                    throw new ValidationProofFailedException(schemeName);
                }

                try
                {
                    AttributeProofs attributeForCheck;
                    if (schemeName == attributesForCheck.RootAttribute.SchemeName)
                    {
                        attributeForCheck = attributesForCheck.RootAttribute;
                    }
                    else
                    {
                        attributeForCheck = attributesForCheck.Attributes.FirstOrDefault(a => a.SchemeName == schemeName);
                        VerifyBindingToParent(attributeForCheck, attributesForCheck.RootAttribute.Commitment);
                    }

                    await VerifyValueProof(attributeForCheck, attributesForCheck.Issuer).ConfigureAwait(false);
                }
                catch (NullReferenceException)
                {
                    throw new ValidationProofsWereNotCompleteException(schemeName);
                }
            }
            else
            {
                await VerifyBindingToParentAndValue(schemeName, rootIssuerAttributes, attributesForCheck).ConfigureAwait(false);
            }
        }

        private async Task VerifyBindingToParentAndValue(string schemeName, AttributesByIssuer rootIssuerAttributes, AttributesByIssuer attributesForCheck)
        {
            // when the found attribute resides at the Associated Identity there are two possible options:
            // 1. the found attribute is an associated one of the associated identity
            // 2. the found attribute is a root one of the associated identity 
            // In the case of former, it is required to prove binding of the found attribute to the root
            // one of the associated identity and binding of the root attribute of the
            // associated identity to the root one of the root identity.
            //
            // In the case of latter, it is required just to prove the binding of the found attribute to 
            // the root attribute of the root identity

            try
            {
                AttributeProofs attributeForCheck;
                IKey parentCommitment;
                if (schemeName == attributesForCheck.RootAttribute.SchemeName)
                {
                    attributeForCheck = attributesForCheck.RootAttribute;
                    parentCommitment = rootIssuerAttributes.RootAttribute.Commitment;
                }
                else
                {
                    VerifyBindingToParent(attributesForCheck.RootAttribute, rootIssuerAttributes.RootAttribute.Commitment);

                    attributeForCheck = attributesForCheck.Attributes.FirstOrDefault(a => a.SchemeName == schemeName);
                    parentCommitment = attributesForCheck.RootAttribute.Commitment;
                }

                VerifyBindingToParent(attributeForCheck, parentCommitment);

                await VerifyValueProof(attributeForCheck, attributesForCheck.Issuer).ConfigureAwait(false);

            }
            catch (NullReferenceException)
            {
                throw new ValidationProofsWereNotCompleteException(schemeName);
            }
        }

        private async Task VerifyValueProof(AttributeProofs attributeForCheck, IKey issuer)
        {
            if (attributeForCheck.CommitmentProof.DidUrls?.Any() ?? false)
            {
                long schemeId = await _assetsService.GetSchemeId(attributeForCheck.SchemeName, issuer.ToString()).ConfigureAwait(false);
                byte[][] assetIds = attributeForCheck.CommitmentProof.DidUrls.Select(v => _assetsService.GenerateAssetId(schemeId, v)).ToArray();
                bool proofOfValue = CryptoHelper.VerifyIssuanceSurjectionProof(attributeForCheck.CommitmentProof.SurjectionProof, attributeForCheck.Commitment.Value.Span, assetIds);
                if (!proofOfValue)
                {
                    _logger.Error("Proof of value failed");
                    throw new ValidationProofFailedException(attributeForCheck.SchemeName);
                }
            }
            else
            {
                bool proofOfValueKnowledge = CryptoHelper.VerifySurjectionProof(attributeForCheck.CommitmentProof.SurjectionProof, attributeForCheck.Commitment.Value.Span);
                if (!proofOfValueKnowledge)
                {
                    _logger.Error("Proof of value knowledge failed");
                    throw new ValidationProofFailedException(attributeForCheck.SchemeName);
                }
            }
        }

        private void VerifyBindingToParent(AttributeProofs attributeForCheck, IKey parentCommitment)
        {
            if (attributeForCheck is null)
            {
                throw new ArgumentNullException(nameof(attributeForCheck));
            }

            if (parentCommitment is null)
            {
                throw new ArgumentNullException(nameof(parentCommitment));
            }

            // Check binding to the Parent Attribute
            byte[] commitmentToParentAndBinding = CryptoHelper.SumCommitments(attributeForCheck.Commitment.Value.Span, parentCommitment.Value.Span);
            bool proofToParent = CryptoHelper.VerifySurjectionProof(attributeForCheck.BindingProof, commitmentToParentAndBinding);
            if (!proofToParent)
            {
                _logger.Error("Proof of binding to parent failed");
                throw new ValidationProofFailedException(attributeForCheck.SchemeName);
            }
        }

        #endregion Private Functions
    }
}
