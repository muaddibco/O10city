using Prism.Navigation;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Services.Inherence;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ExtensionPoint]
    public interface IVerifierInteractionService
    {
        string Name { get; }
        string Buffer { get; set; }

        InherenceServiceInfo ServiceInfo { get; set; }

        Task InvokeRegistration(INavigationService navigationService, string args = null);
        Task InvokeUnregistration(INavigationService navigationService, string args = null);
        Task InvokeVerification(INavigationService navigationService, string args = null);

        Task<VerificationResult> Verify(long rootAttributeId, byte[] photoBytes);
    }
}
