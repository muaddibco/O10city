using O10.Core.ExtensionMethods;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services.ActionResolvers
{
    [RegisterExtension(typeof(IActionResolver), Lifetime = LifetimeManagement.Singleton)]
    public class RootAttributeRequestActionResolver : IActionResolver
    {
        public string ResolveAction(string action)
        {
            if (action.Contains("ProcessRootIdentityRequest"))
            {
                return $"RootAttributeRequest?action={action.EncodeToEscapedString64()}";
            }

            return null;
        }
    }
}
