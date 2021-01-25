using NSubstitute;
using System;
using O10.Client.Common.Crypto;
using O10.Client.Common.Entities;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.Cryptography;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.HashCalculations;
using Xunit;

namespace O10.Client.Common.Tests
{
    public class StateClientCryptoServiceTests
    {
        private readonly IHashCalculationsRepository _hashCalculationsRepository;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IAssetsService _assetsService;

        public StateClientCryptoServiceTests()
        {
            _hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
            _hashCalculationsRepository.Create(HashType.Keccak256).Returns(new Keccak256HashCalculation());
            _hashCalculationsRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());
            _hashCalculationsRepository.Create(HashType.Tiger4).Returns(new Tiger4HashCalculation());
            _hashCalculationsRepository.Create(HashType.Sha224).Returns(new Sha224HashCalculation());
            _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            _identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());

            ISchemeResolverService schemeResolverService = Substitute.For<ISchemeResolverService>();
            schemeResolverService
                .ResolveAttributeScheme(null, null)
                .Returns(ci =>
                {
                    return (ci.ArgAt<string>(1)) switch
                    {
                        AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD => new AttributeDefinition { SchemeId = 100 },
                        _ => null,
                    };
                });


            _assetsService = new AssetsService(_hashCalculationsRepository, schemeResolverService);
        }

        [Fact]
        public void DecodeCommitmentSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);
            var decoded = clientCryptoService.DecodeCommitment(Array.Empty<byte>(), Array.Empty<byte>());

            Assert.NotNull(decoded);
        }

        [Fact]
        public void DecodeEcdhTupleSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);
            clientCryptoService.Initialize(ConfidentialAssetsHelper.GetRandomSeed());

            byte[] arr1 = Array.Empty<byte>();
            byte[] arr2 = Array.Empty<byte>();
            clientCryptoService.DecodeEcdhTuple(new EcdhTupleCA(), null, out arr1, out arr2);

            Assert.NotNull(arr1);
            Assert.NotNull(arr2);
        }

        [Fact]
        public void DecodeEcdhTuplePayloadSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);

            byte[] arr1 = Array.Empty<byte>();
            byte[] arr2 = Array.Empty<byte>();
            byte[] arr3 = Array.Empty<byte>();
            clientCryptoService.DecodeEcdhTuple(new EcdhTupleIP(), arr1, out arr2, out arr3);

            Assert.NotNull(arr3);
            Assert.NotNull(arr2);
        }

        [Fact]
        public void DecodeEcdhTupleProofsSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);

            byte[] arr1 = Array.Empty<byte>();
            byte[] arr2 = Array.Empty<byte>();
            byte[] arr3 = Array.Empty<byte>();
            byte[] arr4 = Array.Empty<byte>();
            byte[] arr5 = Array.Empty<byte>();
            clientCryptoService.DecodeEcdhTuple(new EcdhTupleProofs(), arr1, out arr2, out arr3, out arr4, out arr5);

            Assert.NotNull(arr3);
            Assert.NotNull(arr2);
        }

        [Fact]
        public void EncodeEcdhTupleSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);

            byte[] arr1 = Array.Empty<byte>();
            byte[] arr2 = Array.Empty<byte>();
            var retval = clientCryptoService.EncodeEcdhTuple(arr1, arr2);

            Assert.NotNull(retval);
        }

        [Fact]
        public void GetBoundedCommitmentSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);

            byte[] arr1 = Array.Empty<byte>();
            byte[] arr2 = Array.Empty<byte>();
            byte[] arr3 = Array.Empty<byte>();
            RingSignature ringSignature = new RingSignature();
            clientCryptoService.GetBoundedCommitment(arr1, out arr2, out arr3, out ringSignature);

            Assert.True(true);
        }

        [Fact]
        public void InitializeSucceeded()
        {
            StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);

            clientCryptoService.Initialize(Array.Empty<byte[]>());

            Assert.True(true);
        }

        [Fact]
		public void EncodeDecodeAssetTest()
		{
			byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
			StateClientCryptoService clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);
			clientCryptoService.Initialize(secretKey);

			byte[] assetId = _assetsService.GenerateAssetId(1, "123456789");
			byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] nonBlindedAssetId = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetId);
			EcdhTupleCA ecdhTupleCA = clientCryptoService.EncodeEcdhTuple(blindingFactor, assetId);
			clientCryptoService.DecodeEcdhTuple(ecdhTupleCA, null, out byte[] blindingFactorDecoded, out byte[] assetIdDecoded);

			Assert.Equal(assetId, assetIdDecoded);
			Assert.Equal(blindingFactor, blindingFactorDecoded);
		}
    }
}
