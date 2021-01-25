using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IToastService), Lifetime = LifetimeManagement.Singleton)]
    public class ToastService : IToastService
    {
        public void LongMessage(string message)
        {
            DependencyService.Get<IToast>().LongMessage(message);
        }

        public void ShortMessage(string message)
        {
            DependencyService.Get<IToast>().ShortMessage(message);
        }
    }
}
