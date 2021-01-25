using System;
using System.Collections.Generic;
using O10.Client.Common.ExternalIdps.BlinkId;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.Architecture;

using O10.Core.Translators;

namespace O10.Client.Web.Portal.ExternalIdps
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class BlinkIdToAttributesTranslator : TranslatorBase<BlinkIdIdentityRequest, Dictionary<string, string>>
    {
        public override Dictionary<string, string> Translate(BlinkIdIdentityRequest obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Dictionary<string, string> attributes = new Dictionary<string, string>();

            if (obj.DocumentNationality == "Israel")
            {
                switch (obj.DocumentType)
                {
                    case "DrivingLicense":
                        FillDrivingLicenseAttributes(obj, attributes);
                        break;
                    case "Passport":
                        FillPassportAttributes(obj, attributes);
                        break;
                }
            }

            return attributes;
        }

        private static void FillDrivingLicenseAttributes(BlinkIdIdentityRequest obj, Dictionary<string, string> attributes)
        {
            if (!string.IsNullOrEmpty(obj.DocumentNumber))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_DRIVINGLICENSE, obj.DocumentNumber);
            }
            if (!string.IsNullOrEmpty(obj.DocumentNumber))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, obj.DocumentNumber);
            }
            if (!string.IsNullOrEmpty(obj.LocalIdNumber))
            {
                attributes.Add(
                    AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                    obj.LocalIdNumber.StartsWith("ID") ? obj.LocalIdNumber.Remove(0, 2) : obj.LocalIdNumber);
            }
            if (obj.DateOfBirth.HasValue)
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH, obj.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
            if (obj.IssuanceDate.HasValue)
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_ISSUANCEDATE, obj.IssuanceDate.Value.ToString("yyyy-MM-dd"));
            }
            if (obj.ExpirationDate.HasValue)
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_EXPIRATIONDATE, obj.ExpirationDate.Value.ToString("yyyy-MM-dd"));
            }
            if (!string.IsNullOrEmpty(obj.FirstName))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME, obj.FirstName);
            }
            if (!string.IsNullOrEmpty(obj.LastName))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_LASTNAME, obj.LastName);
            }
            if (!string.IsNullOrEmpty(obj.VehicleType))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_DL_VEHICLETYPE, obj.VehicleType);
            }
        }

        private static void FillPassportAttributes(BlinkIdIdentityRequest obj, Dictionary<string, string> attributes)
        {
            if (!string.IsNullOrEmpty(obj.DocumentNumber))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORT, obj.DocumentNumber);
            }
            if (!string.IsNullOrEmpty(obj.DocumentNumber))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, obj.DocumentNumber);
            }
            if (!string.IsNullOrEmpty(obj.LocalIdNumber))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_IDCARD, obj.LocalIdNumber);
            }
            if (obj.DateOfBirth.HasValue)
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH, obj.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
            if (obj.ExpirationDate.HasValue)
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_EXPIRATIONDATE, obj.ExpirationDate.Value.ToString("yyyy-MM-dd"));
            }
            if (!string.IsNullOrEmpty(obj.FirstName))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME, obj.FirstName);
            }
            if (!string.IsNullOrEmpty(obj.LastName))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_LASTNAME, obj.LastName);
            }
            if (!string.IsNullOrEmpty(obj.IssuerState))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_ISSUER, obj.IssuerState);
            }
            if (!string.IsNullOrEmpty(obj.Nationality))
            {
                attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_NATIONALITY, obj.Nationality);
            }
        }
    }
}
