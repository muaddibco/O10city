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

		public async Task<byte[]> GetGroupId(string attributeSchemeName, string issuer)
        {
			Entities.AttributeDefinition attributeScheme = await _schemeResolverService.ResolveAttributeScheme(issuer, attributeSchemeName).ConfigureAwait(false);
			
			byte[] groupId = new byte[32];
            Array.Copy(BitConverter.GetBytes(attributeScheme.SchemeId), 0, groupId, _hashCalculation.HashSize, sizeof(long));

            return groupId;
        }

        public async Task<byte[]> GetGroupId(string attributeSchemeName, string content, string issuer)
        {
			Entities.AttributeDefinition attributeScheme = await _schemeResolverService.ResolveAttributeScheme(issuer, attributeSchemeName).ConfigureAwait(false);
			byte[] groupId = new byte[32];

            byte[] hash = _hashCalculation.CalculateHash(Encoding.ASCII.GetBytes(content));
            Array.Copy(hash, 0, groupId, 0, hash.Length);
            Array.Copy(BitConverter.GetBytes(attributeScheme.SchemeId), 0, groupId, hash.Length, sizeof(long));

            return groupId;
        }

        public async Task<byte[]> GetGroupId(string attributeSchemeName, DateTime content, string issuer)
        {
			Entities.AttributeDefinition attributeScheme = await _schemeResolverService.ResolveAttributeScheme(issuer, attributeSchemeName).ConfigureAwait(false);
			TimeSpan diff = content - new DateTime(1900, 1, 1);

            ulong days = (ulong)diff.TotalDays;
            byte[] groupId = new byte[32];
            Array.Copy(BitConverter.GetBytes(days), 0, groupId, 0, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(attributeScheme.SchemeId), 0, groupId, _hashCalculation.HashSize, sizeof(long));

            return groupId;
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
