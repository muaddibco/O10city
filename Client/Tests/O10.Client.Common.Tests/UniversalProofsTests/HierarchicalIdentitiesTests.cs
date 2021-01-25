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

            byte[] rootAttributeAssetId = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] attributeAssetId = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] bf = ConfidentialAssetsHelper.GetRandomSeed();

            byte[] commitmentRoot = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] commitmentRootToRoot = ConfidentialAssetsHelper.BlindAssetCommitment(commitmentRoot, bf);

            byte[] commitmentAssociated = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] commitmentToRoot = ConfidentialAssetsHelper.BlindAssetCommitment(commitmentAssociated, bf);

            byte[] bindingKey = ConfidentialAssetsHelper.PasswordHash(pwd);
            byte[] bfRoot = GetBlindingFactor(bindingKey, rootAttributeAssetId);
            byte[] blindingPointRoot = GetBlindingPoint(bindingKey, rootAttributeAssetId);
            byte[] bfValue = GetBlindingFactor(bindingKey, rootAttributeAssetId, attributeAssetId);
            byte[] blindingPointValue = GetBlindingPoint(bindingKey, rootAttributeAssetId, attributeAssetId);
            byte[] nonBlindedRootCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] nonBlindedAttributeCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(attributeAssetId);
            byte[] commitmentToValue = ConfidentialAssetsHelper.SumCommitments(blindingPointValue, nonBlindedAttributeCommitment);
            byte[] commitmentToRootBinding = ConfidentialAssetsHelper.SumCommitments(blindingPointRoot, nonBlindedRootCommitment);
            commitmentToRootBinding = ConfidentialAssetsHelper.SumCommitments(commitmentToRootBinding, nonBlindedAttributeCommitment);

            byte[] bfAttribute = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] blindedAttributeCommitment = ConfidentialAssetsHelper.BlindAssetCommitment(nonBlindedAttributeCommitment, bfAttribute);
            byte[] bfToValueDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(bfAttribute, bfValue);
            var surjectionProofValue = ConfidentialAssetsHelper.CreateSurjectionProof(blindedAttributeCommitment, new byte[][] { commitmentToValue }, 0, bfToValueDiff);
            byte[] commitmentToRootAndBinding = ConfidentialAssetsHelper.SumCommitments(blindedAttributeCommitment, commitmentToRoot);
            byte[] bfProtectionAndRoot = ConfidentialAssetsHelper.SumScalars(bfAttribute, bf);
            byte[] bfToRootDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfRoot);
            var surjectionProofRoot = ConfidentialAssetsHelper.CreateSurjectionProof(commitmentToRootAndBinding, new byte[][] { commitmentToRootBinding }, 0, bfToRootDiff);

        }

        private byte[] GetBlindingPoint(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = ConfidentialAssetsHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = ConfidentialAssetsHelper.ReduceScalar32(blindingFactorSeed);
            byte[] blindingPoint = ConfidentialAssetsHelper.GetPublicKey(blindingFactor);

            return blindingPoint;
        }

        private byte[] GetBlindingFactor(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = ConfidentialAssetsHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = ConfidentialAssetsHelper.ReduceScalar32(blindingFactorSeed);

            return blindingFactor;
        }
    }
}
