using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using O10.Core.ExtensionMethods;

namespace O10.Client.Mobile.Base.Services.ActionResolvers
{
    [RegisterExtension(typeof(IActionResolver), Lifetime = LifetimeManagement.Singleton)]
    public class O10SamlIdpActionResolver : IActionResolver
    {

        public string ResolveAction(string action)
        {
            if (action.StartsWith("saml://"))
            {
                return $"O10SamlIdp?actionInfo={action.Replace("saml://", "").EncodeToEscapedString64()}";
            }

            return null;
        }


    }
}
