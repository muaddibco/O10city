using O10.Client.Common.Identities;
using O10.Crypto.ConfidentialAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace O10.Client.Common.Tests.UniversalProofsTests
{
    public class HierarchicalIdentitiesTests
    {
        [Fact]
        public void AssociatedAttributeTest()
        {
            string pwd = "qqq";

            byte[] rootAttributeAssetId = CryptoHelper.GetRandomSeed();
            byte[] attributeAssetId = CryptoHelper.GetRandomSeed();
            byte[] bf = CryptoHelper.GetRandomSeed();

            byte[] commitmentRoot = CryptoHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] commitmentRootToRoot = CryptoHelper.BlindAssetCommitment(commitmentRoot, bf);

            byte[] commitmentAssociated = CryptoHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] commitmentToRoot = CryptoHelper.BlindAssetCommitment(commitmentAssociated, bf);

            byte[] bindingKey = CryptoHelper.PasswordHash(pwd);
            byte[] bfRoot = GetBlindingFactor(bindingKey, rootAttributeAssetId);
            byte[] blindingPointRoot = GetBlindingPoint(bindingKey, rootAttributeAssetId);
            byte[] bfValue = GetBlindingFactor(bindingKey, rootAttributeAssetId, attributeAssetId);
            byte[] blindingPointValue = GetBlindingPoint(bindingKey, rootAttributeAssetId, attributeAssetId);
            byte[] nonBlindedRootCommitment = CryptoHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] nonBlindedAttributeCommitment = CryptoHelper.GetNonblindedAssetCommitment(attributeAssetId);
            byte[] commitmentToValue = CryptoHelper.SumCommitments(blindingPointValue, nonBlindedAttributeCommitment);
            byte[] commitmentToRootBinding = CryptoHelper.SumCommitments(blindingPointRoot, nonBlindedRootCommitment);
            commitmentToRootBinding = CryptoHelper.SumCommitments(commitmentToRootBinding, nonBlindedAttributeCommitment);

            byte[] bfAttribute = CryptoHelper.GetRandomSeed();
            byte[] blindedAttributeCommitment = CryptoHelper.BlindAssetCommitment(nonBlindedAttributeCommitment, bfAttribute);
            byte[] bfToValueDiff = CryptoHelper.GetDifferentialBlindingFactor(bfAttribute, bfValue);
            var surjectionProofValue = CryptoHelper.CreateSurjectionProof(blindedAttributeCommitment, new byte[][] { commitmentToValue }, 0, bfToValueDiff);
            byte[] commitmentToRootAndBinding = CryptoHelper.SumCommitments(blindedAttributeCommitment, commitmentToRoot);
            byte[] bfProtectionAndRoot = CryptoHelper.SumScalars(bfAttribute, bf);
            byte[] bfToRootDiff = CryptoHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfRoot);
            var surjectionProofRoot = CryptoHelper.CreateSurjectionProof(commitmentToRootAndBinding, new byte[][] { commitmentToRootBinding }, 0, bfToRootDiff);

        }

        private byte[] GetBlindingPoint(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);
            byte[] blindingPoint = CryptoHelper.GetPublicKey(blindingFactor);

            return blindingPoint;
        }

        private byte[] GetBlindingFactor(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);

            return blindingFactor;
        }
    }
}
