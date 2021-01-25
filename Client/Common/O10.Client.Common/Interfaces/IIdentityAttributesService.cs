using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Identities;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IIdentityAttributesService
    {
        [Obsolete("This function will be moved to IAssetsService")]
		Task<byte[]> GetGroupId(string attributeSchemeName, string issuer);
        [Obsolete("This function will be moved to IAssetsService")]
        Task<byte[]> GetGroupId(string attributeSchemeName, string content, string issuer);
        [Obsolete("This function will be moved to IAssetsService")]
        Task<byte[]> GetGroupId(string attributeSchemeName, DateTime content, string issuer);

        IEnumerable<(string validationType, string validationDescription)> GetAssociatedValidationTypes();

		Task<List<IdentityAttributeValidationDescriptor>> GetIdentityAttributeValidationDescriptors(string issuer, bool activeOnly);
	}
}
