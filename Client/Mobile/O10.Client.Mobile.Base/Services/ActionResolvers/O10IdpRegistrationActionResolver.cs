using O10.Core.ExtensionMethods;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services.ActionResolvers
{
    [RegisterExtension(typeof(IActionResolver), Lifetime = LifetimeManagement.Singleton)]
    public class O10IdpRegistrationActionResolver : IActionResolver
    {
        public string ResolveAction(string action)
        {
            if (action.StartsWith("wreg://"))
            {
                return $"O10IdpRegistration?action={action.Replace("wreg://", "").EncodeToEscapedString64()}";
            }

            return null;
        }
    }
}
