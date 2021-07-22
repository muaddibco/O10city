using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface ISchemeResolverService
    {
        Task<string> ResolveIssuer(string issuer);
        Task<IEnumerable<AttributeDefinition>> ResolveAttributeSchemes(string issuer, bool activeOnly = false);
        Task<AttributeDefinition?> ResolveAttributeScheme(string issuer, long schemeId);
        Task<AttributeDefinition> ResolveAttributeScheme(string issuer, string schemeName);

        Task<AttributeDefinition> GetRootAttributeScheme(string issuer);

        Task StoreGroupRelation(string issuer, string assetId, string groupOwnerKey, string groupName);

        Task<IEnumerable<RegistrationKeyDescriptionStore>> GetGroupRelations(string issuer, string assetId);

        Task<bool> StoreRegistrationCommitment(string issuer, string assetId, string commtiment, string description);

        Task<IEnumerable<RegistrationKeyDescriptionStore>> GetRegistrationCommitments(string issuer, string assetId);

        Task BackupAssociatedAttributes(string rootIsser, string assetId, AssociatedAttributeBackupDTO[] associatedAttributeBackups);

        Task<IEnumerable<AssociatedAttributeBackupDTO>> GetAssociatedAttributeBackups(string issuer, string assetId);
    }
}
