using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Crypto.Models;

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
        
        Task<(byte[] blindingFactor, byte[] boundedCommitment)> GetBoundedCommitment(Memory<byte> receiverPublicKey, Memory<byte> rootAssetId, IEnumerable<Memory<byte>>? assetIds = null);
        
        Task<SurjectionProof> CreateProofToRegistration(Memory<byte> receiverPublicKey, Memory<byte> blindingFactor, Memory<byte> assetCommitment, Memory<byte> rootAssetId, IEnumerable<Memory<byte>>? assetIds = null);

        Task<AttributeProofs> GetRootAttributeProofs(Memory<byte> bf, UserRootAttribute rootAttribute, IKey? target = null, IEnumerable<Memory<byte>>? assetIds = null);
        
        Task<AttributeProofs> GetAssociatedAttributeProofs(BlindingAssetInput assetInput, BlindingAssetInput parentAssetInput, string attributeSchemeName, byte[]? externalBindingKey = null);

        Task<AttributeProofs> GetProtectionAttributeProofs(BlindingAssetInput rootAssetInput, string issuer, byte[]? externalBindingKey = null);

        Task<SurjectionProof> GenerateBindingProofToRoot(BlindingAssetInput assetInput, BlindingAssetInput parentAssetInput);

        Task<RootIssuer> GetAttributeProofs(byte[] bf, UserRootAttribute rootAttribute, IKey? target = null, IEnumerable<UserAssociatedAttribute>? associatedAttributes = null, byte[]? externalBindingKey = null);
    }
}
