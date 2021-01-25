using Prism.Navigation;
using System;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Services.EmbeddedIdPs
{
    [RegisterExtension(typeof(IEmbeddedIdpService), Lifetime = LifetimeManagement.Singleton)]
    public class O10IdpInteractionService : IEmbeddedIdpService
    {
        public string Name => "O10IdP";
        public string Alias => "O10 Identity Provider";
        public string Description => "Provides an Identity based on your email address";

        public async Task InvokeRegistration(INavigationService navigationService, string args = null)
        {
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("O10IdpRegister1" + (!string.IsNullOrEmpty(args) ? $"?{args}" : "")));
        }

        public Task InvokeUnregistration(INavigationService navigationService, string args = null)
        {
            throw new NotImplementedException();
        }
    }
}
