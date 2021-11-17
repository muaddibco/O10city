using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Dtos;
using O10.Core.Architecture;
using O10.Core.Identity;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IAssetsService
    {
        Task<byte[]> GenerateAssetId(string schemeName, string attributeContent, string issuer, string miscName = null);
        byte[] GenerateAssetId(long schemeId, string attributeContent);
        byte[] GenerateAssetCommitment(long schemeId, string attributeContent);

        Task<long> GetSchemeId(string schemeName, string issuer);

        Task<string?> GetAttributeSchemeName(byte[] assetId, string issuer);
        Task<AttributeDefinitionDTO> GetAttributeDefinition(byte[] assetId, string issuer);

        void GetBlindingPoint(byte[] bindingKey, byte[] rootAssetId, out byte[] blindingPoint, out byte[] blindingFactor);
        void GetBlindingPointAndFactor(byte[] bindingKey, byte[] rootAssetId, byte[] assetId, out byte[] blindingPoint, out byte[] blindingFactor);
        byte[] GetBlindingPoint(params byte[][] scalars);
        byte[] GetBlindingFactor(params byte[][] scalars);
        (byte[] blindingFactor, byte[] blindingPoint) GetBlindingFactorAndPoint(params byte[][] scalars);


        byte[] GetCommitmentBlindedByPoint(byte[] assetId, byte[] blindingPoint);

        Task<AttributeDefinitionDTO> GetRootAttributeDefinition(string issuer);
        Task<AttributeDefinitionDTO> GetRootAttributeDefinition(IKey issuer);
        Task<IEnumerable<AttributeDefinitionDTO>> GetAssociatedAttributeDefinitions(string issuer);
        Task<IEnumerable<AttributeDefinitionDTO>> GetAssociatedAttributeDefinitions(IKey issuer);
    }
}
