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
                assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
                blindingFactors[i] = ConfidentialAssetsHelper.GetRandomSeed();
                assetCommitments[i] = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);

                sideAssetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
                sideBlindingFactors[i] = ConfidentialAssetsHelper.GetRandomSeed();
                sideAssetCommitments[i] = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            for (int i = 0; i < totalAssets; i++)
            {
                issuanceProofs[i] = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitments[i], assetIds, i, blindingFactors[i]);
                bool issuanceValid = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(issuanceProofs[i], assetCommitments[i], assetIds);
                Assert.True(issuanceValid);

                sideIssuanceProofs[i] = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(sideAssetCommitments[i], sideAssetIds, i, sideBlindingFactors[i]);
                bool sideIssuanceValid = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(sideIssuanceProofs[i], sideAssetCommitments[i], assetIds);
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
			byte[] aux = ConfidentialAssetsHelper.FastHash256(Encoding.ASCII.GetBytes("some string for test"));

			for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
                blindingFactors[i] = ConfidentialAssetsHelper.GetRandomSeed();
                assetCommitments[i] = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            diffBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            newCommitment = ConfidentialAssetsHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);

            SurjectionProof surjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(newCommitment, assetCommitments, transferredAssetIndex, diffBlindingFactor, aux);

            bool res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, newCommitment, aux);

            Assert.True(res);

			byte[] newCommitmentCorrupted = ConfidentialAssetsHelper.BlindAssetCommitment(assetCommitments[(transferredAssetIndex + 1) % totalAssets], diffBlindingFactor);
			SurjectionProof surjectionProofCorrupted = ConfidentialAssetsHelper.CreateSurjectionProof(newCommitmentCorrupted, assetCommitments, transferredAssetIndex, diffBlindingFactor, aux);
			bool resCorrupted = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProofCorrupted, newCommitmentCorrupted, aux);
			Assert.False(resCorrupted);

			byte[] totalBlindingFactor = ConfidentialAssetsHelper.SumScalars(blindingFactors[transferredAssetIndex], diffBlindingFactor);
            byte[] assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(totalBlindingFactor, assetIds[transferredAssetIndex]);

            Assert.True(assetCommitment.Equals32(newCommitment));

            res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, ConfidentialAssetsHelper.GetRandomSeed(), aux);
            Assert.False(res);

            res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, assetCommitments[transferredAssetIndex], aux);
            Assert.False(res);

            byte[] randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();

            while(!ConfidentialAssetsHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();
            }

            surjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(randomCommitment, assetCommitments, 6, ConfidentialAssetsHelper.GetRandomSeed());
            res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);

            randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();

            while (ConfidentialAssetsHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();
            }

            surjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(randomCommitment, assetCommitments, 6, ConfidentialAssetsHelper.GetRandomSeed());
            res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

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
                assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
                blindingFactors[i] = ConfidentialAssetsHelper.GetRandomSeed();
                assetCommitments[i] = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
            }

            diffBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            diffBlindingFactor2 = ConfidentialAssetsHelper.GetRandomSeed();
            diffBlindingFactorSum = ConfidentialAssetsHelper.SumScalars(diffBlindingFactor, diffBlindingFactor2);
            
            newCommitment = ConfidentialAssetsHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);
            newCommitment2 = ConfidentialAssetsHelper.BlindAssetCommitment(newCommitment, diffBlindingFactor2);

            SurjectionProof surjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(newCommitment2, assetCommitments, transferredAssetIndex, diffBlindingFactorSum);

            bool res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, newCommitment2);

            Assert.True(res);

            byte[] totalBlindingFactor = ConfidentialAssetsHelper.SumScalars(blindingFactors[transferredAssetIndex], diffBlindingFactorSum);
            byte[] assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(totalBlindingFactor, assetIds[transferredAssetIndex]);

            Assert.True(assetCommitment.Equals32(newCommitment2));
        }

		[Fact]
		public void BoundedMultiSurjectionProofsVerificationTest()
		{
			int totalAssets = 9;
			int transferredAssetIndex = 4;

			byte[] senderSk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] senderPk = ConfidentialAssetsHelper.GetPublicKey(senderSk);
			byte[] receiverSk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] receiverPk = ConfidentialAssetsHelper.GetPublicKey(receiverSk);
			byte[] sharedSecret = ConfidentialAssetsHelper.GetReducedSharedSecret(senderSk, receiverPk);

			byte[][] assetIds = new byte[totalAssets][];
			byte[][] blindingFactors = new byte[totalAssets][];
			byte[][] assetCommitments = new byte[totalAssets][];
			byte[] transferCommitment, registrationCommitment, transferBlindingFactor, registrationBlindingFactor, registrationToPrevBlindingFactorSub,  registrationToTransferBlindingFactorSub;

			for (int i = 0; i < totalAssets; i++)
			{
				assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
				blindingFactors[i] = ConfidentialAssetsHelper.GetRandomSeed();
				assetCommitments[i] = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
			}

			transferBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
			registrationBlindingFactor = sharedSecret;
			registrationToPrevBlindingFactorSub = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(registrationBlindingFactor, blindingFactors[transferredAssetIndex]);
			registrationToTransferBlindingFactorSub = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(registrationBlindingFactor, transferBlindingFactor);

			transferCommitment = ConfidentialAssetsHelper.GetAssetCommitment(transferBlindingFactor, assetIds[transferredAssetIndex]);

			registrationCommitment = ConfidentialAssetsHelper.GetAssetCommitment(registrationBlindingFactor, assetIds[transferredAssetIndex]);

			SurjectionProof surjectionProofOwnership = ConfidentialAssetsHelper.CreateSurjectionProof(registrationCommitment, assetCommitments, transferredAssetIndex, registrationToPrevBlindingFactorSub);
			SurjectionProof surjectionProofTransfer = ConfidentialAssetsHelper.CreateSurjectionProof(registrationCommitment, new byte[][] { transferCommitment }, 0, registrationToTransferBlindingFactorSub);

			bool resOwnership = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProofOwnership, registrationCommitment);
			bool resTransfer = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProofTransfer, registrationCommitment);

			Assert.True(resOwnership);
			Assert.True(resTransfer);
		}

		[Fact]
        public void InversedSurjectionProofTest()
        {
            byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] nonBlindedAssetCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetId);
            byte[] blindingFactor1 = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] blindedAssetCommitment1 = ConfidentialAssetsHelper.BlindAssetCommitment(nonBlindedAssetCommitment, blindingFactor1);
            byte[] blindingFactor2 = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] blindedAssetCommitment2 = ConfidentialAssetsHelper.BlindAssetCommitment(blindedAssetCommitment1, blindingFactor2);

            SurjectionProof surjectionProof1 = ConfidentialAssetsHelper.CreateSurjectionProof(blindedAssetCommitment2, new byte[][] { blindedAssetCommitment1 }, 0, blindingFactor2);
            bool res1 = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof1, blindedAssetCommitment2);

            Assert.True(res1);

            byte[] blindingFactorNegated = ConfidentialAssetsHelper.NegateBlindingFactor(blindingFactor2);

            SurjectionProof surjectionProof2 = ConfidentialAssetsHelper.CreateSurjectionProof(blindedAssetCommitment1, new byte[][] { blindedAssetCommitment2 }, 0, blindingFactorNegated);
            bool res2 = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof2, blindedAssetCommitment1);

            Assert.True(res2);
        }

        [Fact]
        public void DynamicSurjectionProofTest()
        {
            byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] pk = ConfidentialAssetsHelper.GetPublicKey(blindingFactor);

            byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] nonBlindedCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetId);

            byte[] blindedCommitment = ConfidentialAssetsHelper.SumCommitments(pk, nonBlindedCommitment);

            SurjectionProof surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(blindedCommitment, new byte[][] { assetId }, 0, blindingFactor);
            bool res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, blindedCommitment, new byte[][] { assetId });

            Assert.True(res);

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(blindedCommitment, new byte[][] { assetId }, 0, ConfidentialAssetsHelper.GetRandomSeed());
            res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, blindedCommitment, new byte[][] { assetId });

            Assert.False(res);
        }

        [Fact]
        public void AssetsRingSignaturesTest()
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
                assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
                blindingFactors[i] = ConfidentialAssetsHelper.GetRandomSeed();
                nonBlindedAssetCommitments[i] = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetIds[i]);
                assetCommitments[i] = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactors[i], assetIds[i]);
                pks[i] = ConfidentialAssetsHelper.SubCommitments(assetCommitments[i], nonBlindedAssetCommitments[i]);
            }

            diffBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            newCommitment = ConfidentialAssetsHelper.BlindAssetCommitment(assetCommitments[transferredAssetIndex], diffBlindingFactor);

            SurjectionProof surjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(newCommitment, assetCommitments, transferredAssetIndex, diffBlindingFactor);

            bool res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, newCommitment);

            Assert.True(res);

            byte[] msg = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] keyImage = ConfidentialAssetsHelper.GenerateKeyImage(blindingFactors[transferredAssetIndex]);

            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            StealthSigningService cryptoService = new StealthSigningService(identityKeyProvidersRegistry, CoreFixture.LoggerService);

            RingSignature[] ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg, keyImage, pks, blindingFactors[transferredAssetIndex], transferredAssetIndex);
            res = ConfidentialAssetsHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.True(res);

            res = ConfidentialAssetsHelper.VerifyRingSignature(msg, ConfidentialAssetsHelper.GetRandomSeed(), pks, ringSignatures);
            Assert.False(res);
        }

        [Fact]
        public void SingleRingSignatureTest()
        {
            byte[] msg = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] otsk = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] keyImage = ConfidentialAssetsHelper.GenerateKeyImage(otsk);
            byte[][] pks = new byte[1][];
            pks[0] = ConfidentialAssetsHelper.GetPublicKey(otsk);

            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            StealthSigningService cryptoService = new StealthSigningService(identityKeyProvidersRegistry, CoreFixture.LoggerService);

            RingSignature[] ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg, keyImage, pks, otsk, 0);
            bool res = ConfidentialAssetsHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.True(res);

            byte[] keyImage1 = ConfidentialAssetsHelper.GenerateKeyImage(ConfidentialAssetsHelper.GetRandomSeed());
            ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg, keyImage1, pks, otsk, 0);
            res = ConfidentialAssetsHelper.VerifyRingSignature(msg, keyImage1, pks, ringSignatures);

            Assert.False(res);

            pks[0] = ConfidentialAssetsHelper.GetPublicKey(ConfidentialAssetsHelper.GetRandomSeed());
            ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg, keyImage, pks, otsk, 0);
            res = ConfidentialAssetsHelper.VerifyRingSignature(msg, keyImage, pks, ringSignatures);

            Assert.False(res);
        }

        [Fact]
        public void NewAssetIssuanceSurjectionProofsTest()
        {
            int totalAssets = 9;
            int transferredAssetIndex = 4;

            byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[][] assetIds = new byte[totalAssets][];
            byte[] assetCommitment;

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
            }

            assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, assetIds[transferredAssetIndex]);

            SurjectionProof surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            bool res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds);

            Assert.True(res);

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds.Skip(1).Union(new byte[][] { ConfidentialAssetsHelper.GetRandomSeed() }).ToArray());

            Assert.False(res);

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, (transferredAssetIndex + 1) % totalAssets, blindingFactor);

            res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds);

            Assert.False(res);

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds.Skip(transferredAssetIndex + 1).ToArray(), 0, blindingFactor);

            res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds.Skip(transferredAssetIndex + 1).ToArray());

            Assert.False(res);

            byte[] randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();

            while (!ConfidentialAssetsHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();
            }

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(randomCommitment, assetIds, transferredAssetIndex, ConfidentialAssetsHelper.GetRandomSeed());
            res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);

            randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();

            while (ConfidentialAssetsHelper.IsPointValid(randomCommitment))
            {
                randomCommitment = ConfidentialAssetsHelper.GetRandomSeed();
            }

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(randomCommitment, assetIds, transferredAssetIndex, ConfidentialAssetsHelper.GetRandomSeed());
            res = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProof, randomCommitment);

            Assert.False(res);
        }

        [Fact]
        public void NewAssetIssuance1on1SurjectionProofsTest()
        {
            int totalAssets = 1;
            int transferredAssetIndex = 0;

            byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[][] assetIds = new byte[totalAssets][];
            byte[] assetCommitment;

            for (int i = 0; i < totalAssets; i++)
            {
                assetIds[i] = ConfidentialAssetsHelper.GetRandomSeed();
            }

            assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, assetIds[transferredAssetIndex]);

            SurjectionProof surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            bool res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, assetIds);

            Assert.True(res);

            surjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(assetCommitment, assetIds, transferredAssetIndex, blindingFactor);

            res = ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, assetCommitment, new byte[][] { ConfidentialAssetsHelper.GetRandomSeed() });

            Assert.False(res);
        }

        [Fact]
        public void DestinationKeyTest()
        {
            byte[] secretTransactionKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] publicTransactionKey = ConfidentialAssetsHelper.GetPublicKey(secretTransactionKey);
            byte[] secretViewKey = ConfidentialAssetsHelper.GetRandomSeed(), secretSpendKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] publicViewKey = ConfidentialAssetsHelper.GetPublicKey(secretViewKey), publicSpendKey = ConfidentialAssetsHelper.GetPublicKey(secretSpendKey);

            byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretTransactionKey, publicSpendKey, publicViewKey);

            bool isDestinationKeyMine = ConfidentialAssetsHelper.IsDestinationKeyMine(destinationKey, publicTransactionKey, secretViewKey, publicSpendKey);

            Assert.True(isDestinationKeyMine);
        }

        //[Fact]
        public void AssetValueRangeProof()
        {
            byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] nonBlindedAssetCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetId);

            byte[] assetBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] assetCommitment = ConfidentialAssetsHelper.BlindAssetCommitment(nonBlindedAssetCommitment, assetBlindingFactor);

            byte[] valueBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] valueAssetCommitment = ConfidentialAssetsHelper.CreateBlindedValueCommitmentFromBlindingFactor(assetCommitment, 1, valueBlindingFactor);

            RangeProof rangeProof = ConfidentialAssetsHelper.CreateValueRangeProof(assetCommitment, valueAssetCommitment, 1, valueBlindingFactor);
            bool res = ConfidentialAssetsHelper.VerifyValueRangeProof(rangeProof, assetCommitment, valueAssetCommitment);

            Assert.True(res);

            RangeProof rangeProof1 = ConfidentialAssetsHelper.CreateValueRangeProof(assetCommitment, valueAssetCommitment, 11, valueBlindingFactor);
            bool res1 = ConfidentialAssetsHelper.VerifyValueRangeProof(rangeProof1, assetCommitment, valueAssetCommitment);

            Assert.False(res1);
        }

        //[Fact]
        public void BorromeanRSTest()
        {
            byte[] msg = ConfidentialAssetsHelper.GetRandomSeed();

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
                    sks[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
                    pks[i][j] = ConfidentialAssetsHelper.GetPublicKey(sks[i][j]);

                    if(indicies[i] == j)
                    {
                        sksMine[i] = sks[i][j];
                    }
                }
            }

            BorromeanRingSignatureEx brs = ConfidentialAssetsHelper.GenerateBorromeanRingSignature(msg, pks, sksMine, indicies);
            bool res = ConfidentialAssetsHelper.VerifyBorromeanRingSignature(brs, msg, pks);

            Assert.True(res);
        }

		[Fact]
		public void EncodeDecodeAssetTest()
		{
			byte[] bf = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] sk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] pk = ConfidentialAssetsHelper.GetPublicKey(sk);
			byte[] svk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] pvk = ConfidentialAssetsHelper.GetPublicKey(svk);

			EcdhTupleCA ecdhTuple = ConfidentialAssetsHelper.CreateEcdhTupleCA(bf, assetId, sk, pvk);
			ConfidentialAssetsHelper.DecodeEcdhTuple(ecdhTuple, pk, svk, out byte[] bfDecoded, out byte[] assetIdDecoded);

			Assert.Equal(assetId, assetIdDecoded);
			Assert.Equal(bf, bfDecoded);
		}

		[Fact]
		public void EncryptDecryptTest()
		{
			byte[] sk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] pk = ConfidentialAssetsHelper.GetPublicKey(sk);
			Sodium.SodiumCore.Init();

			KeyPair keyPair = PublicKeyBox.GenerateKeyPair(sk);

			byte[] pk1 = ConfidentialAssetsHelper.GetPublicKey(keyPair.PrivateKey);
			
			string text = "Text text!";

			byte[] textBytes = Encoding.ASCII.GetBytes(text);

			byte[] cipher = Sodium.SealedPublicKeyBox.Create(text, keyPair.PublicKey);

			byte[] textDecodedBytes = Sodium.SealedPublicKeyBox.Open(cipher, sk, keyPair.PublicKey);

			string decodedText = Encoding.ASCII.GetString(textDecodedBytes);

			Assert.Equal(text, decodedText);
		}

		[Fact]
		public void AssetsUniquenessCrossIssuersTest()
		{
			byte[] blindingFactor1 = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] blindingFactor2 = ConfidentialAssetsHelper.GetRandomSeed();

			byte[] assetIdSame = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetId1 = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetId2 = ConfidentialAssetsHelper.GetRandomSeed();

			byte[] assetSP = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] commitmentSP_1 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor1, assetSP);
			byte[] commitmentSP_2 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor2, assetSP);

			byte[] deltaSP = ConfidentialAssetsHelper.SubCommitments(commitmentSP_2, commitmentSP_1);

			byte[] commitmentSame_1 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor1, assetIdSame);
			byte[] commitmentSame_2 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor2, assetIdSame);
			byte[] deltaSame = ConfidentialAssetsHelper.SubCommitments(commitmentSame_2, commitmentSame_1);

			
			byte[] commitment1_1 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor1, assetId1);
			byte[] commitment1_2 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor2, assetId1);
			byte[] commitment1_2_ByDelta = ConfidentialAssetsHelper.SumCommitments(commitment1_1, deltaSP);

			byte[] commitment2_2 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor2, assetId2);

			Assert.True(deltaSP.Equals32(deltaSame));
			Assert.True(commitment1_2.Equals32(commitment1_2_ByDelta));
			Assert.False(commitment2_2.Equals32(commitment1_2_ByDelta));
		}

		[Fact]
		public void EcdhTupleProofsTest()
		{
			byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] issuer = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] payload = ConfidentialAssetsHelper.GetRandomSeed();

			byte[] senderSk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] senderPk = ConfidentialAssetsHelper.GetPublicKey(senderSk);

			byte[] receiverSk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] receiverPk = ConfidentialAssetsHelper.GetPublicKey(receiverSk);

			EcdhTupleProofs proofs = ConfidentialAssetsHelper.CreateEcdhTupleProofs(blindingFactor, assetId, issuer, payload, senderSk, receiverPk);

			ConfidentialAssetsHelper.DecodeEcdhTuple(proofs, senderPk, receiverSk, out byte[] actualBlindingFactor, out byte[] actualAssetId, out byte[] actuaIssuer, out byte[] actualPayload);

			Assert.Equal(blindingFactor, actualBlindingFactor);
			Assert.Equal(assetId, actualAssetId);
			Assert.Equal(issuer, actuaIssuer);
			Assert.Equal(payload, actualPayload);
		}

		[Fact]
		public void EncodedCommitmentTest()
		{
			byte[] commitment = ConfidentialAssetsHelper.GetRandomSeed();

			byte[] senderSk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] senderPk = ConfidentialAssetsHelper.GetPublicKey(senderSk);

			byte[] receiverSk = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] receiverPk = ConfidentialAssetsHelper.GetPublicKey(receiverSk);

			byte[] encodedCommitment = ConfidentialAssetsHelper.CreateEncodedCommitment(commitment, senderSk, receiverPk);
			byte[] decodedCommitment = ConfidentialAssetsHelper.DecodeCommitment(encodedCommitment, senderPk, receiverSk);

			Assert.Equal(commitment, decodedCommitment);
		}

		[Fact]
		public void SkPkTest()
		{

			byte[] seed = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] secretKey = Ed25519.SecretKeyFromSeed(seed);
			Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, seed);

			byte[] pk = ConfidentialAssetsHelper.GetPublicKey(secretKey);

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
			byte[] blindingFactorSeed = ConfidentialAssetsHelper.FastHash256(Encoding.ASCII.GetBytes(blindingFactorSeedString));
			byte[] blindingFactor = ConfidentialAssetsHelper.ReduceScalar32(blindingFactorSeed);
			byte[] blindingPoint = ConfidentialAssetsHelper.GetPublicKey(blindingFactor);
			byte[] nonBlindedAssociatedCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(associatedAssetId);
			byte[] nonBlindedRootCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAssetId);
			byte[] associatedCommitment = ConfidentialAssetsHelper.SumCommitments(blindingPoint, nonBlindedAssociatedCommitment);
			byte[] rootCommitment = ConfidentialAssetsHelper.SumCommitments(blindingPoint, nonBlindedRootCommitment);


			byte[] blindingFactorRoot = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] rootCommitment2 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactorRoot, rootAssetId);
			byte[] rootBfDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactorRoot, blindingFactor);

			SurjectionProof rootSurjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(rootCommitment2, new byte[][] { rootCommitment }, 0, rootBfDiff);

			bool rootRes = ConfidentialAssetsHelper.VerifySurjectionProof(rootSurjectionProof, rootCommitment2);

			Assert.True(rootRes);

			byte[] blindingFactorAssociated = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] associatedCommitment2 = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactorAssociated, associatedAssetId);
			byte[] associatedBfDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactorAssociated, blindingFactor);

			SurjectionProof associatedSurjectoinProof = ConfidentialAssetsHelper.CreateSurjectionProof(associatedCommitment2, new byte[][] { associatedCommitment }, 0, associatedBfDiff);

			bool associatedRes = ConfidentialAssetsHelper.VerifySurjectionProof(associatedSurjectoinProof, associatedCommitment2);

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
