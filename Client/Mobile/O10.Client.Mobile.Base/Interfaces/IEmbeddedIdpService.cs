using Prism.Navigation;
using System.Threading.Tasks;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ExtensionPoint]
    public interface IEmbeddedIdpService
    {
        string Name { get; }
        string Alias { get; }
        string Description { get; }
        Task InvokeRegistration(INavigationService navigationService, string args = null);
        Task InvokeUnregistration(INavigationService navigationService, string args = null);
    }
}
