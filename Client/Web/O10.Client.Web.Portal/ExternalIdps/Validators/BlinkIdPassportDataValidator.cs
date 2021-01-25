using System;
using O10.Client.Common.ExternalIdps.BlinkId;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.ExternalIdps.Validators
{
    [RegisterExtension(typeof(IExternalIdpDataValidator), Lifetime = LifetimeManagement.Singleton)]
    public class BlinkIdPassportDataValidator : IExternalIdpDataValidator
    {
        public string Name => "BlinkID-Passport";

        public void Validate(object request)
        {
            if (!(request is BlinkIdIdentityRequest blinkIdIdentityRequest))
            {
                throw new ArgumentException($"Only argument of {nameof(BlinkIdIdentityRequest)} type can be accepted");
            }

            if (!string.IsNullOrEmpty(blinkIdIdentityRequest.LocalIdNumber))
            {
                string value = blinkIdIdentityRequest.LocalIdNumber;

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
