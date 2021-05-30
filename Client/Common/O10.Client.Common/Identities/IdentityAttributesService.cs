using System;
using System.Collections.Generic;
using System.Text;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using O10.Core;
using O10.Core.ExtensionMethods;
using O10.Core.Architecture;

using O10.Core.HashCalculations;
using System.Threading.Tasks;
using O10.Client.DataLayer.AttributesScheme;

namespace O10.Client.Common.Identities
{
    [RegisterDefaultImplementation(typeof(IIdentityAttributesService), Lifetime = LifetimeManagement.Singleton)]
    public class IdentityAttributesService : IIdentityAttributesService
    {
        private readonly IHashCalculation _hashCalculation;
        private readonly ISchemeResolverService _schemeResolverService;

        public IdentityAttributesService(IHashCalculationsRepository hashCalculationsRepository, ISchemeResolverService schemeResolverService)
        {
            _hashCalculation = hashCalculationsRepository.Create(Globals.ASSET_CREATION_HASH_TYPE);
            _schemeResolverService = schemeResolverService;
        }

        public IEnumerable<(string validationType, string validationDescription)> GetAssociatedValidationTypes()
        {
            List<(string validationType, string validationDescription)> values = new List<(string validationType, string validationDescription)>();

            foreach (var enumValueObj in Enum.GetValues(typeof(ValidationType)))
            {
                ValidationType enumValue = (ValidationType)enumValueObj;

                values.Add((enumValue.ToString(), enumValue.GetDescription()));
            }

            return values;
        }

        public async Task<List<IdentityAttributeValidationDescriptor>> GetIdentityAttributeValidationDescriptors(string issuer, bool activeOnly)
        {
            IEnumerable<Entities.AttributeDefinition> attributeSchemes = await _schemeResolverService.ResolveAttributeSchemes(issuer, activeOnly).ConfigureAwait(false);
            List<IdentityAttributeValidationDescriptor> identityAttributeValidationDescriptors = new List<IdentityAttributeValidationDescriptor>();
            foreach (var item in attributeSchemes)
            {
                IdentityAttributeValidationDescriptor identityAttributeValidationDescriptor =
                    new IdentityAttributeValidationDescriptor
                    {
                        SchemeName = item.SchemeName,
                        SchemeAlias = item.Alias,
                        ValidationType = GetValidationType(item.SchemeName),
                        ValidationTypeName = GetValidationType(item.SchemeName).GetDescription()
                    };
            }

            return identityAttributeValidationDescriptors;
        }

        private ValidationType GetValidationType(string schemeName)
        {
            return AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH.Equals(schemeName) ? ValidationType.AgeInYears : ValidationType.MatchValue;
        }
    }
}
