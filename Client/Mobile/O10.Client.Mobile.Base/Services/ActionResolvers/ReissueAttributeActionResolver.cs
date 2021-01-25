using O10.Core.ExtensionMethods;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services.ActionResolvers
{
    [RegisterExtension(typeof(IActionResolver), Lifetime = LifetimeManagement.Singleton)]
    public class ReissueAttributeActionResolver : IActionResolver
    {
        public string ResolveAction(string action)
        {
            if (action.StartsWith("iss://"))
            {
                return $"ReIssueAttribute?action={action.Replace("iss://", "").EncodeToEscapedString64()}";
            }

            return null;
        }
    }
}
