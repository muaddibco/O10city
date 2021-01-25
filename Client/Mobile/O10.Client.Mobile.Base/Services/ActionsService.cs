using System.Collections.Generic;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IActionsService), Lifetime = LifetimeManagement.Singleton)]
    public class ActionsService : IActionsService
    {
        private readonly IEnumerable<IActionResolver> _actionResolvers;

        public ActionsService(IEnumerable<IActionResolver> actionResolvers)
        {
            _actionResolvers = actionResolvers;
        }

        public string ResolveAction(string encoded)
        {
            string action = encoded.DecodeUnescapedFromString64();

            foreach (var actionResolver in _actionResolvers)
            {
                string navigationUri = actionResolver.ResolveAction(action);
                if (!string.IsNullOrEmpty(navigationUri))
                {
                    return navigationUri;
                }
            }

            return null;
        }
    }
}
