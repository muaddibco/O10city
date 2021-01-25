using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.ViewModels.Inherence
{
    public class InherenceVerifierViewModel : BindableBase
    {
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly INavigationService _navigationService;

        public InherenceVerifierViewModel(IVerifierInteractionsManager verifierInteractionsManager, INavigationService navigationService)
        {
            _verifierInteractionsManager = verifierInteractionsManager;
            _navigationService = navigationService;
        }

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public bool IsRegistered { get; set; }
        public long RootAttributeId { get; set; }

        public DelegateCommand RegisterAtVerifierCommand => new DelegateCommand(() =>
        {
            _verifierInteractionsManager.GetInstance(Name)?.InvokeRegistration(_navigationService, $"rootAttributeId={RootAttributeId}");
            // need to navigate to the page of this verifier
            // need to send request for registration using IdentityProofRequest
            // together with this request need to invoke Rest function
        });

        public DelegateCommand UnregisterAtVerifierCommand => new DelegateCommand(() => _verifierInteractionsManager.GetInstance(Name)?.InvokeUnregistration(_navigationService, $"rootAttributeId={RootAttributeId}"));

        public DelegateCommand VerifyCommand => new DelegateCommand(() => _verifierInteractionsManager.GetInstance(Name)?.InvokeVerification(_navigationService, $"rootAttributeId={RootAttributeId}"));
    }
}
