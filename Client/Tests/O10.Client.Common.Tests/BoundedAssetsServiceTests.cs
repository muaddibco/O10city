﻿using NSubstitute;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.HashCalculations;
using Xunit;
using Xunit.Abstractions;
using O10.Core.Identity;
using O10.Client.Common.Crypto;
using O10.Client.Common.Services;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using O10.Core.Configuration;
using System;
using O10.Client.DataLayer.Model;
using O10.Client.Common.Configuration;
using O10.Client.Common.Dtos.UniversalProofs;
using System.Collections.Generic;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Dtos;

namespace O10.Client.Common.Tests
{
    public class BoundedAssetsServiceTests: TestBase
    {
        private readonly IAssetsService _assetsService;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IGatewayService _gatewayService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IConfigurationService _configurationService;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly string _issuer;

        public BoundedAssetsServiceTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper)
            : base(coreFixture, testOutputHelper)
        {
            _configurationService = Substitute.For<IConfigurationService>();
            _issuer = CryptoHelper.GetRandomSeed().ToHexString();
            _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            _identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            _gatewayService = Substitute.For<IGatewayService>();
            _restApiConfiguration = Substitute.For<IRestApiConfiguration>();
            _restApiConfiguration.RingSize.ReturnsForAnyArgs<ushort>(1);

            _configurationService.Get<IRestApiConfiguration>().ReturnsForAnyArgs(_restApiConfiguration);

            _eligibilityProofsProvider = new EligibilityProofsProvider(coreFixture.LoggerService, _gatewayService, _configurationService);

            IHashCalculationsRepository hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
            hashCalculationsRepository.Create(HashType.Tiger4).Returns(new Tiger4HashCalculation());
            
            ISchemeResolverService schemeResolverService = Substitute.For<ISchemeResolverService>();
            schemeResolverService
                .ResolveAttributeScheme(null, null)
                .ReturnsForAnyArgs(ci =>
                {
                    return (ci.ArgAt<string>(1)) switch
                    {
                        AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD => new AttributeDefinition { SchemeId = 3 },
                        _ => null,
                    };
                });

            _assetsService = new AssetsService(hashCalculationsRepository, schemeResolverService);

            _gatewayService.AreRootAttributesValid(null, null).ReturnsForAnyArgs(true);
            _gatewayService.GetIssuanceCommitments(Array.Empty<byte>(), 0).ReturnsForAnyArgs(ci =>
            {
                var ringSize = ci.ArgAt<int>(1);

                byte[][] issuanceCommitments = new byte[ringSize][];

                issuanceCommitments[0] = _issuer.HexStringToByteArray();

                for (int i = 1; i < ringSize; i++)
                {
                    issuanceCommitments[i] = CryptoHelper.GetRandomSeed();
                }

                return issuanceCommitments;
            });

            _clientCryptoService = new StealthClientCryptoService(_identityKeyProvidersRegistry, coreFixture.LoggerService);
            _clientCryptoService.Initialize(CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed());
        }

        [Fact]
        public async void CreateAndValidateValidRootAttributeProof()
        {
            var boundedAssetsService = new BoundedAssetsService(_assetsService, _clientCryptoService, _identityKeyProvidersRegistry, _eligibilityProofsProvider);
            var proofsValidationService = new ProofsValidationService(_gatewayService, _assetsService, CoreFixture.LoggerService, null);

            await boundedAssetsService.Initialize("pwd").ConfigureAwait(false);

            byte[] issuer = _issuer.HexStringToByteArray();

            string rootAttributeValue = "rootAttribute";
            byte[] rootAttributeAssetId = _assetsService.GenerateAssetId(1, rootAttributeValue);

            byte[] transactonSecretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionPublickey = CryptoHelper.GetPublicKey(transactonSecretKey);
            byte[] bf = _clientCryptoService.GetBlindingFactor(transactionPublickey);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(bf, rootAttributeAssetId);

            byte[] issuanceTransactionSecretKey = CryptoHelper.GetRandomSeed();
            byte[] issuanceTransactionPublicKey = CryptoHelper.GetPublicKey(issuanceTransactionSecretKey);
            byte[] issuanceBf = CryptoHelper.GetOTSK(issuanceTransactionSecretKey, _clientCryptoService.PublicKeys[1].ToByteArray());
            byte[] issuanceCommitment = CryptoHelper.GetAssetCommitment(issuanceBf, rootAttributeAssetId);

            UserRootAttribute userRootAttribute = new UserRootAttribute
            {
                AssetId = rootAttributeAssetId,
                Content = "rootAttribute",
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                IssuanceCommitment = issuanceCommitment,
                IssuanceTransactionKey = issuanceTransactionPublicKey,
                Source = issuer.ToHexString()
            };

            var attributeProofs = await boundedAssetsService.GetRootAttributeProofs(bf, userRootAttribute).ConfigureAwait(false);

            Assert.Equal(assetCommitment.ToHexString(), attributeProofs.Commitment.ToString());

            await proofsValidationService.CheckEligibilityProofs(attributeProofs.Commitment.Value, attributeProofs.BindingProof, null).ConfigureAwait(false);
        }

        [Fact]
        public async void CreateAndValidateRootAttributeProof_IssuanceBfIncorrect_ShouldFail()
        {
            var boundedAssetsService = new BoundedAssetsService(_assetsService, _clientCryptoService, _identityKeyProvidersRegistry, _eligibilityProofsProvider);
            var proofsValidationService = new ProofsValidationService(_gatewayService, _assetsService, CoreFixture.LoggerService, null);

            await boundedAssetsService.Initialize("pwd").ConfigureAwait(false);

            byte[] issuer = _issuer.HexStringToByteArray();

            string rootAttributeValue = "rootAttribute";
            byte[] rootAttributeAssetId = _assetsService.GenerateAssetId(1, rootAttributeValue);

            byte[] transactonSecretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionPublickey = CryptoHelper.GetPublicKey(transactonSecretKey);
            byte[] bf = _clientCryptoService.GetBlindingFactor(transactionPublickey);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(bf, rootAttributeAssetId);

            byte[] issuanceTransactionSecretKey = CryptoHelper.GetRandomSeed();
            byte[] issuanceTransactionPublicKey = CryptoHelper.GetPublicKey(issuanceTransactionSecretKey);
            byte[] issuanceBf = CryptoHelper.GetRandomSeed();
            byte[] issuanceCommitment = CryptoHelper.GetAssetCommitment(issuanceBf, rootAttributeAssetId);

            UserRootAttribute userRootAttribute = new UserRootAttribute
            {
                AssetId = rootAttributeAssetId,
                Content = "rootAttribute",
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                IssuanceCommitment = issuanceCommitment,
                IssuanceTransactionKey = issuanceTransactionPublicKey,
                Source = issuer.ToHexString()
            };

            var attributeProofs = await boundedAssetsService.GetRootAttributeProofs(bf, userRootAttribute).ConfigureAwait(false);

            Assert.Equal(assetCommitment.ToHexString(), attributeProofs.Commitment.ToString());

            await Assert.ThrowsAsync<CommitmentNotEligibleException>(async () => await proofsValidationService.CheckEligibilityProofs(attributeProofs.Commitment.Value, attributeProofs.BindingProof, null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async void CreateAndValidateValidAssociatedAttributeProof()
        {
            var boundedAssetsService = new BoundedAssetsService(_assetsService, _clientCryptoService, _identityKeyProvidersRegistry, _eligibilityProofsProvider);
            var proofsValidationService = new ProofsValidationService(_gatewayService, _assetsService, CoreFixture.LoggerService, null);

            await boundedAssetsService.Initialize("pwd").ConfigureAwait(false);

            IKey issuer = _identityKeyProvidersRegistry.GetInstance().GetKey(CryptoHelper.GetRandomSeed());


            string rootAttributeValue = "rootAttribute";

            var rootAssetInput = new BlindingAssetInput(_assetsService.GenerateAssetId(1, rootAttributeValue));

            byte[] issuanceBf = CryptoHelper.GetRandomSeed();
            byte[] issuanceCommitment = CryptoHelper.GetAssetCommitment(issuanceBf, rootAssetInput.AssetId);

            UserRootAttribute userRootAttribute = new UserRootAttribute
            {
                AssetId = rootAssetInput.AssetId,
                Content = "rootAttribute",
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                IssuanceCommitment = issuanceCommitment,
                OriginalBlindingFactor = issuanceBf,
                Source = issuer.ToString()
            };

            var attributeIdCardProofs = await boundedAssetsService.GetRootAttributeProofs(issuanceBf, userRootAttribute).ConfigureAwait(false);
            
            string associatedAttributeValue = "associatedAttribute";

            var associatedAssetInput = new BlindingAssetInput(_assetsService.GenerateAssetId(2, associatedAttributeValue));

            var attributeFirstNameProofs = await boundedAssetsService.GetAssociatedAttributeProofs(associatedAssetInput, rootAssetInput, AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME).ConfigureAwait(false);
            
            ValidationCriteriaDTO validationFirstName = new ValidationCriteriaDTO
            {
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME,
                ValidationType = DataLayer.Enums.ValidationType.MatchValue,
                NumericCriterion = null,
                GroupIdCriterion = null
            };

            AttributesByIssuer attributesByIssuer = new AttributesByIssuer
            {
                RootAttribute = attributeIdCardProofs,
                Issuer = issuer,
                Attributes = new List<AttributeProofs> { attributeFirstNameProofs }
            };

            var validationCriterias = new List<ValidationCriteriaDTO> { validationFirstName };

            RootIssuer rootIssuer = new RootIssuer
            {
                Issuer = issuer,
                IssuersAttributes = new List<AttributesByIssuer>
                {
                    attributesByIssuer
                }
            };

            await proofsValidationService.CheckAssociatedProofs(rootIssuer, validationCriterias).ConfigureAwait(false);
        }

        [Fact]
        public async void CreateAndValidateValidTwoLayerAttributeProofs()
        {
            var boundedAssetsService = new BoundedAssetsService(_assetsService, _clientCryptoService, _identityKeyProvidersRegistry, _eligibilityProofsProvider);
            var proofsValidationService = new ProofsValidationService(_gatewayService, _assetsService, CoreFixture.LoggerService, null);

            await boundedAssetsService.Initialize("pwd").ConfigureAwait(false);

            IKey issuer1 = _identityKeyProvidersRegistry.GetInstance().GetKey(CryptoHelper.GetRandomSeed());
            IKey issuer2 = _identityKeyProvidersRegistry.GetInstance().GetKey(CryptoHelper.GetRandomSeed());

            string rootAttributeParentValue = "rootAttributParent";

            var rootAssetInput = new BlindingAssetInput(_assetsService.GenerateAssetId(1, rootAttributeParentValue));

            byte[] issuanceBf = CryptoHelper.GetRandomSeed();
            byte[] issuanceCommitment = CryptoHelper.GetAssetCommitment(issuanceBf, rootAssetInput.AssetId);

            UserRootAttribute userRootAttribute = new UserRootAttribute
            {
                AssetId = rootAssetInput.AssetId,
                Content = "rootAttribute",
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                IssuanceCommitment = issuanceCommitment,
                OriginalBlindingFactor = issuanceBf,
                Source = issuer1.ToString()
            };

            var attributeIdCardProofs = await boundedAssetsService.GetRootAttributeProofs(issuanceBf, userRootAttribute).ConfigureAwait(false);

            string rootAttributeChildValue = "rootAttributeChild";

            var associatedRootAssetInput = new BlindingAssetInput(_assetsService.GenerateAssetId(2, rootAttributeChildValue));

            var attributeDrivingLicenseProofs = await boundedAssetsService.GetAssociatedAttributeProofs(associatedRootAssetInput, rootAssetInput, AttributesSchemes.ATTR_SCHEME_NAME_DRIVINGLICENSE).ConfigureAwait(false);

            string associatedAttributeValue = "associatedAttribute";
            var associatedChildAssetInput = new BlindingAssetInput(_assetsService.GenerateAssetId(2, associatedAttributeValue));

            var attributeFirstNameProofs = await boundedAssetsService.GetAssociatedAttributeProofs(associatedChildAssetInput, associatedRootAssetInput, AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME).ConfigureAwait(false);

            ValidationCriteriaDTO validationFirstName = new ValidationCriteriaDTO
            {
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME,
                ValidationType = DataLayer.Enums.ValidationType.MatchValue,
                NumericCriterion = null,
                GroupIdCriterion = null
            };

            ValidationCriteriaDTO validationDrivingLicense = new ValidationCriteriaDTO
            {
                SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_DRIVINGLICENSE,
                ValidationType = DataLayer.Enums.ValidationType.MatchValue,
                NumericCriterion = null,
                GroupIdCriterion = null
            };

            AttributesByIssuer attributesByIssuerParent = new AttributesByIssuer
            {
                RootAttribute = attributeIdCardProofs,
                Issuer = issuer1,
            };

            AttributesByIssuer attributesByIssuerChild = new AttributesByIssuer
            {
                RootAttribute = attributeDrivingLicenseProofs,
                Issuer = issuer2,
                Attributes = new List<AttributeProofs> { attributeFirstNameProofs }
            };

            RootIssuer rootIssuer = new RootIssuer
            {
                Issuer = issuer1,
                IssuersAttributes = new List<AttributesByIssuer>
                {
                    attributesByIssuerParent, attributesByIssuerChild
                }
            };

            var validationCriterias = new List<ValidationCriteriaDTO> { validationFirstName, validationDrivingLicense };


            await proofsValidationService.CheckAssociatedProofs(rootIssuer, validationCriterias).ConfigureAwait(false);
        }
    }
}
