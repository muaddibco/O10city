using Prism.Navigation;
using System;
using System.Threading.Tasks;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services.EmbeddedIdPs
{
    //[RegisterExtension(typeof(IEmbeddedIdpService), Lifetime = LifetimeManagement.Singleton)]
    public class BlinkIdPassportInteractionService : IEmbeddedIdpService
    {
        public string Name => "BlinkIDPassport";

        public string Alias => "Blink ID Passport";

        public string Description => throw new NotImplementedException();

        public Task InvokeRegistration(INavigationService navigationService, string args = null)
        {
            throw new NotImplementedException();
        }

        public Task InvokeUnregistration(INavigationService navigationService, string args = null)
        {
            throw new NotImplementedException();
        }
    }
}
