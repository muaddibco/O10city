using System;
using O10.Client.Common.ExternalIdps.BlinkId;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.ExternalIdps.Validators
{
    [RegisterExtension(typeof(IExternalIdpDataValidator), Lifetime = LifetimeManagement.Singleton)]
    public class BlinkIdDrivingLicenseDataValidator : IExternalIdpDataValidator
    {
        public string Name => "BlinkID-DrivingLicense";

        public void Validate(object request)
        {
            if (!(request is BlinkIdIdentityRequest blinkIdIdentityRequest))
            {
                throw new ArgumentException($"Only argument of {nameof(BlinkIdIdentityRequest)} type can be accepted");
            }

            if (!string.IsNullOrEmpty(blinkIdIdentityRequest.LocalIdNumber))
            {

                if (!blinkIdIdentityRequest.LocalIdNumber.StartsWith("ID"))
                {
                    throw new ArgumentException($"{nameof(blinkIdIdentityRequest.LocalIdNumber)} must start with 'ID'");
                }

                string value = blinkIdIdentityRequest.LocalIdNumber.Remove(0, 2);

                if (value.Length != 9 || !IsDigitsOnly(value))
                {
                    throw new ArgumentException($"{nameof(blinkIdIdentityRequest.LocalIdNumber)} must contain 9 digits exactly");
                }
            }
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
