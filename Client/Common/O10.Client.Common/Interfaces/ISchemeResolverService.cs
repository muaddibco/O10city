using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Dtos;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface ISchemeResolverService
    {
        Task<string> ResolveIssuer(string issuer);
        Task<IEnumerable<AttributeDefinitionDTO>> ResolveAttributeSchemes(string issuer, bool activeOnly = false);
        Task<AttributeDefinitionDTO?> ResolveAttributeScheme(string issuer, long schemeId);
        Task<AttributeDefinitionDTO> ResolveAttributeScheme(string issuer, string schemeName);

        Task<AttributeDefinitionDTO> GetRootAttributeScheme(string issuer);

        Task StoreGroupRelation(string issuer, string assetId, string groupOwnerKey, string groupName);

        Task<IEnumerable<RegistrationKeyDescriptionStoreDTO>> GetGroupRelations(string issuer, string assetId);

        Task<bool> StoreRegistrationCommitment(string issuer, string assetId, string commtiment, string description);

        Task<IEnumerable<RegistrationKeyDescriptionStoreDTO>> GetRegistrationCommitments(string issuer, string assetId);

        Task BackupAssociatedAttributes(string rootIsser, string assetId, AssociatedAttributeBackupDTO[] associatedAttributeBackups);

        Task<IEnumerable<AssociatedAttributeBackupDTO>> GetAssociatedAttributeBackups(string issuer, string assetId);
    }
}
