using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;
using O10.Core.Cryptography;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IBoundedAssetsService
    {
        void Initialize(TaskCompletionSource<byte[]> bindingKeySource);
        Task Initialize(string pwd, bool replace = true);

        bool IsBindingKeySet();

        byte[] GetBindingKey(string pwd);
        Task<byte[]> GetBindingKey();

        void GetBoundedCommitment(Memory<byte> assetId, Memory<byte> receiverPublicKey, out byte[] blindingFactor, out byte[] assetCommitment, params byte[][] scalars);
        Task<(byte[] blindingFactor, byte[] boundedCommitment)> GetBoundedCommitment(Memory<byte> receiverPublicKey, params Memory<byte>[] assetIds);
        Task<SurjectionProof> CreateProofToRegistration(byte[] receiverPublicKey, byte[] blindingFactor, byte[] assetCommitment, params Memory<byte>[] assetIds);

        Task<AttributeProofs> GetRootAttributeProofs(byte[] bf, UserRootAttribute rootAttribute);
        Task<AttributeProofs> GetAssociatedAttributeProofs(BlindingAssetInput assetInput, BlindingAssetInput parentAssetInput, string attributeSchemeName);

        Task<AttributeProofs> GetProtectionAttributeProofs(BlindingAssetInput rootAssetInput, string issuer);

        Task<SurjectionProof> GenerateBindingProofToRoot(BlindingAssetInput assetInput, BlindingAssetInput parentAssetInput);

        Task<RootIssuer> GetAttributeProofs(byte[] bf, UserRootAttribute rootAttribute, IEnumerable<UserAssociatedAttribute> associatedAttributes = null, bool withProtectionAttribute = false);
    }
}
