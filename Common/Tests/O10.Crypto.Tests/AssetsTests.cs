using System;
using O10.Core.Cryptography;
using O10.Crypto.ConfidentialAssets;
using O10.Core.ExtensionMethods;
using Xunit;
using System.Linq;
using O10.Core.Identity;
using NSubstitute;
using System.Text;
using Sodium;
using Chaos.NaCl;
using HashLib;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using Xunit.Abstractions;
using O10.Crypto.Services;
using Chaos.NaCl.Internal.Ed25519Ref10;

namespace O10.Crypto.Tests
{
    public class AssetsTests : TestBase
    {
        public AssetsTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper) : base(coreFixture, testOutputHelper)
        {
        }

        [Fact]
        public void SurjectionProofsIssuanceVerificationTest()
        {
            int totalAssets = 9;

            byte[][] assetIds = new byte[totalAssets][];
            byte[][] blindingFactors = new byte[totalAssets][];
            byte[][] assetCommitments = new byte[totalAssets][];
            SurjectionProof[] issuanceProofs = new SurjectionProof[totalAssets];

            byte[][] sideAssetIds = new byte[totalAssets][];
            byte[][] sideBlindingFactors = new byte[totalAssets][];
            byte[][] sideAssetCommitments = new byte[totalAssets][];
            SurjectionProof[] sideIssuanceProofs = new SurjectionProof[totalAssets];

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
                blindingFactors[i] = CryptoHelper.GetRandomSeed();
                assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);

                sideAssetIds[i] = CryptoHelper.GetRandomSeed();
                sideBlindingFactors[i] = CryptoHelper.GetRandomSeed();
                sideAssetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            for (int i = 0; i < totalAssets; i++)
            {
                issuanceProofs[i] = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitments[i], assetIds, i, blindingFactors[i]);
                bool issuanceValid = CryptoHelper.VerifyIssuanceSurjectionProof(issuanceProofs[i], assetCommitments[i], assetIds);
                Assert.True(issuanceValid);

                sideIssuanceProofs[i] = CryptoHelper.CreateNewIssuanceSurjectionProof(sideAssetCommitments[i], sideAssetIds, i, sideBlindingFactors[i]);
                bool sideIssuanceValid = CryptoHelper.VerifyIssuanceSurjectionProof(sideIssuanceProofs[i], sideAssetCommitments[i], assetIds);
                Assert.False(sideIssuanceValid);
            }
        }

        [Fact]
        public void SurjectionProofsVerificationTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 3;

            byte[][] assetIds = new byte[totalAssets][];
            byte[][] blindingFactors = new byte[totalAssets][];
            byte[][] assetCommitments = new byte[totalAssets][];
            byte[] newCommitment, diffBlindingFactor;
			byte[] aux = CryptoHelper.FastHash256(Encoding.ASCII.GetBytes("some string for test"));

			for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
                blindingFactors[i] = CryptoHelper.GetRandomSeed();
                assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            diffBlindingFactor = CryptoHelper.GetRandomSeed();
            newCommitment = CryptoHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(newCommitment, assetCommitments, transferredAssetIndex, diffBlindingFactor, aux);

            bool res = CryptoHelper.VerifySurjectionProof(surjectionProof, newCommitment, aux);

            Assert.True(res);

			byte[] newCommitmentCorrupted = CryptoHelper.BlindAssetCommitment(assetCommitments[(transferredAssetIndex + 1) % totalAssets], diffBlindingFactor);
            SurjectionProof surjectionProofCorrupted = CryptoHelper.CreateSurjectionProof(newCommitmentCorrupted, assetCommitments, transferredAssetIndex, diffBlindingFactor, aux);
			bool resCorrupted = CryptoHelper.VerifySurjectionProof(surjectionProofCorrupted, newCommitmentCorrupted, aux);
			Assert.False(resCorrupted);

			byte[] totalBlindingFactor = CryptoHelper.SumScalars(blindingFactors[transferredAssetIndex], diffBlindingFactor);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(totalBlindingFactor, assetIds[transferredAssetIndex]);

            Assert.True(assetCommitment.Equals32(newCommitment));

            res = CryptoHelper.VerifySurjectionProof(surjectionProof, CryptoHelper.GetRandomSeed(), aux);
            Assert.False(res);

            res = CryptoHelper.VerifySurjectionProof(surjectionProof, assetCommitments[transferredAssetIndex], aux);
            Assert.False(res);

            byte[] randomCommitment = CryptoHelper.GetRandomSeed();

            while(!CryptoHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = CryptoHelper.GetRandomSeed();
            }

            surjectionProof = CryptoHelper.CreateSurjectionProof(randomCommitment, assetCommitments, 6, CryptoHelper.GetRandomSeed());
            res = CryptoHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);

            randomCommitment = CryptoHelper.GetRandomSeed();

            while (CryptoHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = CryptoHelper.GetRandomSeed();
            }

            surjectionProof = CryptoHelper.CreateSurjectionProof(randomCommitment, assetCommitments, 6, CryptoHelper.GetRandomSeed());
            res = CryptoHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);
        }

        [Fact]
        public void SurjectionProofsGenerationTwoVerificationTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 4;

            byte[][] assetIds = new byte[totalAssets][];
            byte[][] blindingFactors = new byte[totalAssets][];
            byte[][] assetCommitments = new byte[totalAssets][];
            byte[] newCommitment, newCommitment2, diffBlindingFactor, diffBlindingFactor2, diffBlindingFactorSum;

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
                blindingFactors[i] = CryptoHelper.GetRandomSeed();
                assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            diffBlindingFactor = CryptoHelper.GetRandomSeed();
            diffBlindingFactor2 = CryptoHelper.GetRandomSeed();
            diffBlindingFactorSum = CryptoHelper.SumScalars(diffBlindingFactor, diffBlindingFactor2);
            
            newCommitment = CryptoHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);
            newCommitment2 = CryptoHelper.BlindAssetCommitment(newCommitment, diffBlindingFactor2);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(newCommitment2, assetCommitments, transferredAssetIndex, diffBlindingFactorSum);

            bool res = CryptoHelper.VerifySurjectionProof(surjectionProof, newCommitment2);

            Assert.True(res);

            byte[] totalBlindingFactor = CryptoHelper.SumScalars(blindingFactors[transferredAssetIndex], diffBlindingFactorSum);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(totalBlindingFactor, assetIds[transferredAssetIndex]);

            Assert.True(assetCommitment.Equals32(newCommitment2));
        }

		[Fact]
		public void BoundedMultiSurjectionProofsVerificationTest()
		{
			int totalAssets = 9;
			int transferredAssetIndex = 4;

			byte[] senderSk = CryptoHelper.GetRandomSeed();
			byte[] senderPk = CryptoHelper.GetPublicKey(senderSk);
			byte[] receiverSk = CryptoHelper.GetRandomSeed();
			byte[] receiverPk = CryptoHelper.GetPublicKey(receiverSk);
			byte[] sharedSecret = CryptoHelper.GetReducedSharedSecret(senderSk, receiverPk);

			byte[][] assetIds = new byte[totalAssets][];
			byte[][] blindingFactors = new byte[totalAssets][];
			byte[][] assetCommitments = new byte[totalAssets][];
			byte[] transferCommitment, registrationCommitment, transferBlindingFactor, registrationBlindingFactor, registrationToPrevBlindingFactorSub,  registrationToTransferBlindingFactorSub;

			for (int i = 0; i < totalAssets; i++)
			{
				assetIds[i] = CryptoHelper.GetRandomSeed();
				blindingFactors[i] = CryptoHelper.GetRandomSeed();
				assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
			}

			transferBlindingFactor = CryptoHelper.GetRandomSeed();
			registrationBlindingFactor = sharedSecret;
			registrationToPrevBlindingFactorSub = CryptoHelper.GetDifferentialBlindingFactor(registrationBlindingFactor, blindingFactors[transferredAssetIndex]);
			registrationToTransferBlindingFactorSub = CryptoHelper.GetDifferentialBlindingFactor(registrationBlindingFactor, transferBlindingFactor);

			transferCommitment = CryptoHelper.GetAssetCommitment(transferBlindingFactor, assetIds[transferredAssetIndex]);

			registrationCommitment = CryptoHelper.GetAssetCommitment(registrationBlindingFactor, assetIds[transferredAssetIndex]);

            SurjectionProof surjectionProofOwnership = CryptoHelper.CreateSurjectionProof(registrationCommitment, assetCommitments, transferredAssetIndex, registrationToPrevBlindingFactorSub);
            SurjectionProof surjectionProofTransfer = CryptoHelper.CreateSurjectionProof(registrationCommitment, new byte[][] { transferCommitment }, 0, registrationToTransferBlindingFactorSub);

			bool resOwnership = CryptoHelper.VerifySurjectionProof(surjectionProofOwnership, registrationCommitment);
			bool resTransfer = CryptoHelper.VerifySurjectionProof(surjectionProofTransfer, registrationCommitment);

			Assert.True(resOwnership);
			Assert.True(resTransfer);
		}

		[Fact]
        public void InversedSurjectionProofTest()
        {
            byte[] assetId = CryptoHelper.GetRandomSeed();
            byte[] nonBlindedAssetCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
            byte[] blindingFactor1 = CryptoHelper.GetRandomSeed();
            byte[] blindedAssetCommitment1 = CryptoHelper.BlindAssetCommitment(nonBlindedAssetCommitment, blindingFactor1);
            byte[] blindingFactor2 = CryptoHelper.GetRandomSeed();
            byte[] blindedAssetCommitment2 = CryptoHelper.BlindAssetCommitment(blindedAssetCommitment1, blindingFactor2);

            SurjectionProof surjectionProof1 = CryptoHelper.CreateSurjectionProof(blindedAssetCommitment2, new byte[][] { blindedAssetCommitment1 }, 0, blindingFactor2);
            bool res1 = CryptoHelper.VerifySurjectionProof(surjectionProof1, blindedAssetCommitment2);

            Assert.True(res1);

            byte[] blindingFactorNegated = CryptoHelper.NegateBlindingFactor(blindingFactor2);

            SurjectionProof surjectionProof2 = CryptoHelper.CreateSurjectionProof(blindedAssetCommitment1, new byte[][] { blindedAssetCommitment2 }, 0, blindingFactorNegated);
            bool res2 = CryptoHelper.VerifySurjectionProof(surjectionProof2, blindedAssetCommitment1);

            Assert.True(res2);
        }

        [Fact]
        public void DynamicSurjectionProofTest()
        {
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] pk = CryptoHelper.GetPublicKey(blindingFactor);

            byte[] assetId = CryptoHelper.GetRandomSeed();
            byte[] nonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);

            byte[] blindedCommitment = CryptoHelper.SumCommitments(pk, nonBlindedCommitment);

            SurjectionProof surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(blindedCommitment, new byte[][] { assetId }, 0, blindingFactor);
            bool res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, blindedCommitment, new byte[][] { assetId });

            Assert.True(res);

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(blindedCommitment, new byte[][] { assetId }, 0, CryptoHelper.GetRandomSeed());
            res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, blindedCommitment, new byte[][] { assetId });

            Assert.False(res);
        }

        [Fact]
        public void AssetsLinkableRingSignaturesTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 4;

            byte[][] assetIds = new byte[totalAssets][];
            byte[][] blindingFactors = new byte[totalAssets][];
            byte[][] nonBlindedAssetCommitments = new byte[totalAssets][];
            byte[][] assetCommitments = new byte[totalAssets][];
            byte[] newCommitment, diffBlindingFactor;
            byte[][] pks = new byte[totalAssets][];

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
                blindingFactors[i] = CryptoHelper.GetRandomSeed();
                nonBlindedAssetCommitments[i] = CryptoHelper.GetNonblindedAssetCommitment(assetIds[i]);
                assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
                pks[i] = CryptoHelper.SubCommitments(assetCommitments[i], nonBlindedAssetCommitments[i]);
            }

            diffBlindingFactor = CryptoHelper.GetRandomSeed();
            newCommitment = CryptoHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(newCommitment, assetCommitments, transferredAssetIndex, diffBlindingFactor);

            bool res = CryptoHelper.VerifySurjectionProof(surjectionProof, newCommitment);

            Assert.True(res);

            byte[] msg = CryptoHelper.GetRandomSeed();
            byte[] keyImage = CryptoHelper.GenerateKeyImage(blindingFactors[transferredAssetIndex]);

            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            StealthSigningService cryptoService = new StealthSigningService(identityKeyProvidersRegistry, CoreFixture.LoggerService);

            RingSignature[] ringSignatures = CryptoHelper.GenerateRingSignature(msg, keyImage, pks.Select(s => s.AsMemory()).ToArray(), blindingFactors[transferredAssetIndex], transferredAssetIndex);
            res = CryptoHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.True(res);

            res = CryptoHelper.VerifyRingSignature(msg, CryptoHelper.GenerateKeyImage(CryptoHelper.GetRandomSeed()), pks, ringSignatures);
            Assert.False(res);
        }

        [Fact]
        public void AssetsLinkedByAssetRingSignaturesTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 4;

            byte[][] assetIds = new byte[totalAssets][];
            byte[][] blindingFactors = new byte[totalAssets][];
            byte[][] nonBlindedAssetCommitments = new byte[totalAssets][];
            byte[][] assetCommitments = new byte[totalAssets][];
            byte[] newCommitment, diffBlindingFactor;
            byte[][] pks = new byte[totalAssets][];

            diffBlindingFactor = CryptoHelper.GetRandomSeed();

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
                blindingFactors[i] = CryptoHelper.GetRandomSeed();
                nonBlindedAssetCommitments[i] = CryptoHelper.GetNonblindedAssetCommitment(assetIds[i]);
                assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            newCommitment = CryptoHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);

            for (int i = 0; i < totalAssets; i++)
            {
                pks[i] = CryptoHelper.SubCommitments(newCommitment, assetCommitments[i]);
            }

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(newCommitment, assetCommitments, transferredAssetIndex, diffBlindingFactor);

            bool res = CryptoHelper.VerifySurjectionProof(surjectionProof, newCommitment);

            Assert.True(res);

            byte[] msg = CryptoHelper.GetRandomSeed();
            byte[] keyImage = CryptoHelper.GenerateKeyImage(diffBlindingFactor);

            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            StealthSigningService cryptoService = new StealthSigningService(identityKeyProvidersRegistry, CoreFixture.LoggerService);

            RingSignature[] ringSignatures = CryptoHelper.GenerateRingSignature(msg, keyImage, pks.Select(s => s.AsMemory()).ToArray(), diffBlindingFactor, transferredAssetIndex);
            res = CryptoHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.True(res);

            res = CryptoHelper.VerifyRingSignature(msg, CryptoHelper.GetRandomSeed(), pks, ringSignatures);
            Assert.False(res);
        }

        [Fact]
        public void DestinationKeysAndAssetCommitmentLinkageTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 4;

            byte[][] assetIds = new byte[totalAssets][];
            byte[][] privateSpendKeys = new byte[totalAssets][];
            byte[][] privateViewKeys = new byte[totalAssets][];
            byte[][] publicSpendKeys = new byte[totalAssets][];
            byte[][] publicViewKeys = new byte[totalAssets][];
            byte[][] transactionSecretKeys = new byte[totalAssets][];
            byte[][] transactionPublicKeys = new byte[totalAssets][];
            byte[][] destinationKeys = new byte[totalAssets][];

            byte[][] blindingFactors = new byte[totalAssets][];
            byte[][] nonBlindedAssetCommitments = new byte[totalAssets][];
            byte[][] assetCommitments = new byte[totalAssets][];
            byte[][] newCommitments = new byte[totalAssets][];
            byte[][] diffBlindingFactors = new byte[totalAssets][];

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
                privateSpendKeys[i] = CryptoHelper.GetRandomSeed();
                privateViewKeys[i] = CryptoHelper.GetRandomSeed();
                transactionSecretKeys[i] = CryptoHelper.GetRandomSeed();
                publicSpendKeys[i] = CryptoHelper.GetPublicKey(privateSpendKeys[i]);
                publicViewKeys[i] = CryptoHelper.GetPublicKey(privateViewKeys[i]);
                transactionPublicKeys[i] = CryptoHelper.GetPublicKey(transactionSecretKeys[i]);
                destinationKeys[i] = CryptoHelper.GetDestinationKey(transactionSecretKeys[i], publicSpendKeys[i], publicViewKeys[i]);

                blindingFactors[i] = CryptoHelper.GetRandomSeed();
                diffBlindingFactors[i] = CryptoHelper.GetRandomSeed();
                nonBlindedAssetCommitments[i] = CryptoHelper.GetNonblindedAssetCommitment(assetIds[i]);
                assetCommitments[i] = CryptoHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
                newCommitments[i] = CryptoHelper.BlindAssetCommitment(assetCommitments[i], diffBlindingFactors[i]);

                byte[] destinationKeyTmp1 = CryptoHelper.SumCommitments(destinationKeys[i], newCommitments[i]);
                byte[] destinationKeyTmp2 = CryptoHelper.SubCommitments(destinationKeyTmp1, newCommitments[i]);
                Assert.True(destinationKeys[i].Equals32(destinationKeyTmp2), $"Sum of commitments {destinationKeys[i].ToHexString()} + {newCommitments[i].ToHexString()} = {destinationKeyTmp1.ToHexString()} does not results right because sub = {destinationKeyTmp2.ToHexString()} and not {destinationKeys[i].ToHexString()}");
                destinationKeys[i] = destinationKeyTmp1;
            }

            byte[] newTransactionSecretKey = CryptoHelper.GetRandomSeed();
            byte[] newDestinationKey = CryptoHelper.GetDestinationKey(newTransactionSecretKey, publicSpendKeys[transferredAssetIndex], publicViewKeys[transferredAssetIndex]);
            byte[] otsk = CryptoHelper.GetOTSK(transactionPublicKeys[transferredAssetIndex], privateViewKeys[transferredAssetIndex], privateSpendKeys[transferredAssetIndex]);
            byte[] otpk = CryptoHelper.GetPublicKey(otsk);
            byte[] keyImage = CryptoHelper.GenerateKeyImage(otsk);
            byte[] msg = CryptoHelper.GetRandomSeed();

            destinationKeys[transferredAssetIndex] = CryptoHelper.SubCommitments(destinationKeys[transferredAssetIndex], newCommitments[transferredAssetIndex]);
            Assert.True(otpk.Equals32(destinationKeys[transferredAssetIndex]), $"Calculated OTPK {otpk.ToHexString()} does not equal Destination Key {destinationKeys[transferredAssetIndex].ToHexString()}");

            RingSignature[] ringSignatures = CryptoHelper.GenerateRingSignature(msg, keyImage, destinationKeys.Select(s => s.AsMemory()).ToArray(), otsk, transferredAssetIndex);

            var res = CryptoHelper.VerifyRingSignature(msg, keyImage, destinationKeys, ringSignatures);

            Assert.True(res);
        }

        [Fact]
        public void SingleRingSignatureTest()
        {
            byte[] msg = CryptoHelper.GetRandomSeed();
            byte[] otsk = CryptoHelper.GetRandomSeed();
            byte[] keyImage = CryptoHelper.GenerateKeyImage(otsk);
            byte[][] pks = new byte[1][];
            pks[0] = CryptoHelper.GetPublicKey(otsk);

            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            StealthSigningService cryptoService = new StealthSigningService(identityKeyProvidersRegistry, CoreFixture.LoggerService);

            RingSignature[] ringSignatures = CryptoHelper.GenerateRingSignature(msg, keyImage, pks.Select(s => s.AsMemory()).ToArray(), otsk, 0);
            bool res = CryptoHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.True(res);

            byte[] keyImage1 = CryptoHelper.GenerateKeyImage(CryptoHelper.GetRandomSeed());
            ringSignatures = CryptoHelper.GenerateRingSignature(msg, keyImage1, pks.Select(s => s.AsMemory()).ToArray(), otsk, 0);
            res = CryptoHelper.VerifyRingSignature(msg, keyImage1, pks, ringSignatures);

            Assert.False(res);

            pks[0] = CryptoHelper.GetPublicKey(CryptoHelper.GetRandomSeed());
            ringSignatures = CryptoHelper.GenerateRingSignature(msg, keyImage, pks.Select(s => s.AsMemory()).ToArray(), otsk, 0);
            res = CryptoHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.False(res);
        }

        [Fact]
        public void NewAssetIssuanceSurjectionProofsTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 4;

            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[][] assetIds = new byte[totalAssets][];
            byte[] assetCommitment;

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
            }

            assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, assetIds[transferredAssetIndex]);

            SurjectionProof surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            bool res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds);

            Assert.True(res);

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds.Skip(1).Union(new byte[][] { CryptoHelper.GetRandomSeed() }).ToArray());

            Assert.False(res);

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, (transferredAssetIndex + 1) % totalAssets, blindingFactor);

            res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds);

            Assert.False(res);

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds.Skip(transferredAssetIndex + 1).ToArray(), 0, blindingFactor);

            res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds.Skip(transferredAssetIndex + 1).ToArray());

            Assert.False(res);

            byte[] randomCommitment = CryptoHelper.GetRandomSeed();

            while (!CryptoHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = CryptoHelper.GetRandomSeed();
            }

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(randomCommitment, assetIds, transferredAssetIndex, CryptoHelper.GetRandomSeed());
            res = CryptoHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);

            randomCommitment = CryptoHelper.GetRandomSeed();

            while (CryptoHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = CryptoHelper.GetRandomSeed();
            }

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(randomCommitment, assetIds, transferredAssetIndex, CryptoHelper.GetRandomSeed());
            res = CryptoHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);
        }

        [Fact]
        public void NewAssetIssuance1on1SurjectionProofsTest()
        {
            int totalAssets = 1;
            int transferredAssetIndex = 0;

            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[][] assetIds = new byte[totalAssets][];
            byte[] assetCommitment;

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = CryptoHelper.GetRandomSeed();
            }

            assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, assetIds[transferredAssetIndex]);

            SurjectionProof surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            bool res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds);

            Assert.True(res);

            surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            res = CryptoHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, new byte[][] { CryptoHelper.GetRandomSeed() });

            Assert.False(res);
        }

        [Fact]
        public void DestinationKeyTest()
        {
            byte[] secretTransactionKey = CryptoHelper.GetRandomSeed();
            byte[] publicTransactionKey = CryptoHelper.GetPublicKey(secretTransactionKey);
            byte[] secretViewKey = CryptoHelper.GetRandomSeed(), secretSpendKey = CryptoHelper.GetRandomSeed();
            byte[] publicViewKey = CryptoHelper.GetPublicKey(secretViewKey), publicSpendKey = CryptoHelper.GetPublicKey(secretSpendKey);

            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretTransactionKey, publicSpendKey, publicViewKey);

            bool isDestinationKeyMine = CryptoHelper.IsDestinationKeyMine(destinationKey, publicTransactionKey, secretViewKey, publicSpendKey);

            Assert.True(isDestinationKeyMine);
        }

        //[Fact]
        public void AssetValueRangeProof()
        {
            byte[] assetId = CryptoHelper.GetRandomSeed();
            byte[] nonBlindedAssetCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);

            byte[] assetBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = CryptoHelper.BlindAssetCommitment(nonBlindedAssetCommitment, assetBlindingFactor);

            byte[] valueBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] valueAssetCommitment = CryptoHelper.CreateBlindedValueCommitmentFromBlindingFactor(assetCommitment, 1, valueBlindingFactor);

            RangeProof rangeProof = CryptoHelper.CreateValueRangeProof(assetCommitment, valueAssetCommitment, 1, valueBlindingFactor);
            bool res = CryptoHelper.VerifyValueRangeProof(rangeProof, assetCommitment, valueAssetCommitment);

            Assert.True(res);

            RangeProof rangeProof1 = CryptoHelper.CreateValueRangeProof(assetCommitment, valueAssetCommitment, 11, valueBlindingFactor);
            bool res1 = CryptoHelper.VerifyValueRangeProof(rangeProof1, assetCommitment, valueAssetCommitment);

            Assert.False(res1);
        }

        //[Fact]
        public void BorromeanRSTest()
        {
            byte[] msg = CryptoHelper.GetRandomSeed();

            int totalGroups = 64;
            int itemsInGroup = 2;
            byte[][] sksMine = new byte[totalGroups][];
            byte[][][] sks = new byte[totalGroups][][];
            byte[][][] pks = new byte[totalGroups][][];
            int[] indicies = new int[totalGroups];

            Random r = new Random();
            
            for (int i = 0; i < totalGroups; i++)
            {
                sks[i] = new byte[itemsInGroup][];
                pks[i] = new byte[itemsInGroup][];

                indicies[i] = r.Next(0, itemsInGroup);

                for (int j = 0; j < itemsInGroup; j++)
                {
                    sks[i][j] = CryptoHelper.GetRandomSeed();
                    pks[i][j] = CryptoHelper.GetPublicKey(sks[i][j]);

                    if(indicies[i] == j)
                    {
                        sksMine[i] = sks[i][j];
                    }
                }
            }

            BorromeanRingSignatureEx brs = CryptoHelper.GenerateBorromeanRingSignature(msg, pks, sksMine, indicies);
            bool res = CryptoHelper.VerifyBorromeanRingSignature(brs, msg, pks);

            Assert.True(res);
        }

		[Fact]
		public void EncodeDecodeAssetTest()
		{
			byte[] bf = CryptoHelper.GetRandomSeed();
			byte[] assetId = CryptoHelper.GetRandomSeed();
			byte[] sk = CryptoHelper.GetRandomSeed();
			byte[] pk = CryptoHelper.GetPublicKey(sk);
			byte[] svk = CryptoHelper.GetRandomSeed();
			byte[] pvk = CryptoHelper.GetPublicKey(svk);

            EcdhTupleCA ecdhTuple = CryptoHelper.CreateEcdhTupleCA(bf, assetId, sk, pvk);
            CryptoHelper.DecodeEcdhTuple(ecdhTuple, pk, svk, out byte[] bfDecoded, out byte[] assetIdDecoded);

			Assert.Equal(assetId, assetIdDecoded);
			Assert.Equal(bf, bfDecoded);
		}

		[Fact]
		public void EncryptDecryptTest()
		{
			byte[] sk = CryptoHelper.GetRandomSeed();
			byte[] pk = CryptoHelper.GetPublicKey(sk);
            SodiumCore.Init();

			KeyPair keyPair = PublicKeyBox.GenerateKeyPair(sk);

			byte[] pk1 = CryptoHelper.GetPublicKey(keyPair.PrivateKey);
			
			string text = "Text text!";

			byte[] textBytes = Encoding.ASCII.GetBytes(text);

			byte[] cipher = SealedPublicKeyBox.Create(text, keyPair.PublicKey);

			byte[] textDecodedBytes = SealedPublicKeyBox.Open(cipher, sk, keyPair.PublicKey);

			string decodedText = Encoding.ASCII.GetString(textDecodedBytes);

			Assert.Equal(text, decodedText);
		}

		[Fact]
		public void AssetsUniquenessCrossIssuersTest()
		{
			byte[] blindingFactor1 = CryptoHelper.GetRandomSeed();
			byte[] blindingFactor2 = CryptoHelper.GetRandomSeed();

			byte[] assetIdSame = CryptoHelper.GetRandomSeed();
			byte[] assetId1 = CryptoHelper.GetRandomSeed();
			byte[] assetId2 = CryptoHelper.GetRandomSeed();

			byte[] assetSP = CryptoHelper.GetRandomSeed();
			byte[] commitmentSP_1 = CryptoHelper.GetAssetCommitment(blindingFactor1, assetSP);
			byte[] commitmentSP_2 = CryptoHelper.GetAssetCommitment(blindingFactor2, assetSP);

			byte[] deltaSP = CryptoHelper.SubCommitments(commitmentSP_2, commitmentSP_1);

			byte[] commitmentSame_1 = CryptoHelper.GetAssetCommitment(blindingFactor1, assetIdSame);
			byte[] commitmentSame_2 = CryptoHelper.GetAssetCommitment(blindingFactor2, assetIdSame);
			byte[] deltaSame = CryptoHelper.SubCommitments(commitmentSame_2, commitmentSame_1);

			
			byte[] commitment1_1 = CryptoHelper.GetAssetCommitment(blindingFactor1, assetId1);
			byte[] commitment1_2 = CryptoHelper.GetAssetCommitment(blindingFactor2, assetId1);
			byte[] commitment1_2_ByDelta = CryptoHelper.SumCommitments(commitment1_1, deltaSP);

			byte[] commitment2_2 = CryptoHelper.GetAssetCommitment(blindingFactor2, assetId2);

			Assert.True(deltaSP.Equals32(deltaSame));
			Assert.True(commitment1_2.Equals32(commitment1_2_ByDelta));
			Assert.False(commitment2_2.Equals32(commitment1_2_ByDelta));
		}

		[Fact]
		public void EcdhTupleProofsTest()
		{
			byte[] blindingFactor = CryptoHelper.GetRandomSeed();
			byte[] assetId = CryptoHelper.GetRandomSeed();
			byte[] issuer = CryptoHelper.GetRandomSeed();
			byte[] payload = CryptoHelper.GetRandomSeed();

			byte[] senderSk = CryptoHelper.GetRandomSeed();
			byte[] senderPk = CryptoHelper.GetPublicKey(senderSk);

			byte[] receiverSk = CryptoHelper.GetRandomSeed();
			byte[] receiverPk = CryptoHelper.GetPublicKey(receiverSk);

            EcdhTupleProofs proofs = CryptoHelper.CreateEcdhTupleProofs(blindingFactor, assetId, issuer, payload, senderSk, receiverPk);

            CryptoHelper.DecodeEcdhTuple(proofs, senderPk, receiverSk, out byte[] actualBlindingFactor, out byte[] actualAssetId, out byte[] actuaIssuer, out byte[] actualPayload);

			Assert.Equal(blindingFactor, actualBlindingFactor);
			Assert.Equal(assetId, actualAssetId);
			Assert.Equal(issuer, actuaIssuer);
			Assert.Equal(payload, actualPayload);
		}

		[Fact]
		public void EncodedCommitmentTest()
		{
			byte[] commitment = CryptoHelper.GetRandomSeed();

			byte[] senderSk = CryptoHelper.GetRandomSeed();
			byte[] senderPk = CryptoHelper.GetPublicKey(senderSk);

			byte[] receiverSk = CryptoHelper.GetRandomSeed();
			byte[] receiverPk = CryptoHelper.GetPublicKey(receiverSk);

			byte[] encodedCommitment = CryptoHelper.CreateEncodedCommitment(commitment, senderSk, receiverPk);
			byte[] decodedCommitment = CryptoHelper.DecodeCommitment(encodedCommitment, senderPk, receiverSk);

			Assert.Equal(commitment, decodedCommitment);
		}

		[Fact]
		public void SkPkTest()
		{

			byte[] seed = CryptoHelper.GetRandomSeed();
			byte[] secretKey = Ed25519.SecretKeyFromSeed(seed);
			Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, seed);

			byte[] pk = CryptoHelper.GetPublicKey(secretKey);

			Assert.Equal(publicKey, pk);
		}

		[Fact]
		public void BlindingPointTest()
		{
			string rootContent = "root";
			string associatedContent = "associated";
			string pwd = "qqq";

			byte[] rootAssetId = GetAssetId(rootContent);
			byte[] associatedAssetId = GetAssetId(associatedContent);

			string blindingFactorSeedString = $"{associatedContent}{pwd}";
			byte[] blindingFactorSeed = CryptoHelper.FastHash256(Encoding.ASCII.GetBytes(blindingFactorSeedString));
			byte[] blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);
			byte[] blindingPoint = CryptoHelper.GetPublicKey(blindingFactor);
			byte[] nonBlindedAssociatedCommitment = CryptoHelper.GetNonblindedAssetCommitment(associatedAssetId);
			byte[] nonBlindedRootCommitment = CryptoHelper.GetNonblindedAssetCommitment(rootAssetId);
			byte[] associatedCommitment = CryptoHelper.SumCommitments(blindingPoint, nonBlindedAssociatedCommitment);
			byte[] rootCommitment = CryptoHelper.SumCommitments(blindingPoint, nonBlindedRootCommitment);


			byte[] blindingFactorRoot = CryptoHelper.GetRandomSeed();
			byte[] rootCommitment2 = CryptoHelper.GetAssetCommitment(blindingFactorRoot, rootAssetId);
			byte[] rootBfDiff = CryptoHelper.GetDifferentialBlindingFactor(blindingFactorRoot, blindingFactor);

            SurjectionProof rootSurjectionProof = CryptoHelper.CreateSurjectionProof(rootCommitment2, new byte[][] { rootCommitment }, 0, rootBfDiff);

			bool rootRes = CryptoHelper.VerifySurjectionProof(rootSurjectionProof, rootCommitment2);

			Assert.True(rootRes);

			byte[] blindingFactorAssociated = CryptoHelper.GetRandomSeed();
			byte[] associatedCommitment2 = CryptoHelper.GetAssetCommitment(blindingFactorAssociated, associatedAssetId);
			byte[] associatedBfDiff = CryptoHelper.GetDifferentialBlindingFactor(blindingFactorAssociated, blindingFactor);

            SurjectionProof associatedSurjectoinProof = CryptoHelper.CreateSurjectionProof(associatedCommitment2, new byte[][] { associatedCommitment }, 0, associatedBfDiff);

			bool associatedRes = CryptoHelper.VerifySurjectionProof(associatedSurjectoinProof, associatedCommitment2);

			Assert.True(associatedRes);
		}

		private static byte[] GetAssetId(string content)
		{
			byte[] assetId = new byte[32];

			byte[] hash = HashFactory.Crypto.CreateSHA224().ComputeBytes(Encoding.ASCII.GetBytes(content)).GetBytes();
			Array.Copy(hash, 0, assetId, 0, hash.Length);
			Array.Copy(BitConverter.GetBytes((uint)1), 0, assetId, hash.Length, sizeof(uint));
			return assetId;
		}
	}
}
