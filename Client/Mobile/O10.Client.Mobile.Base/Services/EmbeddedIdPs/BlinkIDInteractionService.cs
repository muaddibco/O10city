using Prism.Navigation;
using System;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Services.EmbeddedIdPs
{
    [RegisterExtension(typeof(IEmbeddedIdpService), Lifetime = LifetimeManagement.Singleton)]
    public class BlinkIDInteractionService : IEmbeddedIdpService
    {
        public string Name => "BlinkID";
        public string Alias => "Blink ID Driver License";
        public string Description => "Provides an Identity based on scan of your Driver License";

        public async Task InvokeRegistration(INavigationService navigationService, string args = null)
        {
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("BlinkID" + (!string.IsNullOrEmpty(args) ? $"?{args}" : "")));
        }

        public Task InvokeUnregistration(INavigationService navigationService, string args = null)
        {
            throw new NotImplementedException();
        }
    }
}
