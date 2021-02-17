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
        IEnumerable<(string validationType, string validationDescription)> GetAssociatedValidationTypes();

		Task<List<IdentityAttributeValidationDescriptor>> GetIdentityAttributeValidationDescriptors(string issuer, bool activeOnly);
	}
}
