using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.ViewModels.EmbeddedIdPs
{
    public class EmbeddedIdpViewModel : BindableBase
    {
        private readonly IEmbeddedIdpsManager _embeddedIdpsManager;
        private readonly INavigationService _navigationService;

        public EmbeddedIdpViewModel(IEmbeddedIdpsManager embeddedIdpsManager, INavigationService navigationService)
        {
            _embeddedIdpsManager = embeddedIdpsManager;
            _navigationService = navigationService;
        }

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string BoundedToIssuer { get; set; }
        public string BoundedToAssetId { get; set; }

        public DelegateCommand RegisterCommand => new DelegateCommand(() =>
        {
            string arg = null;
            if (!string.IsNullOrEmpty(BoundedToIssuer) && !string.IsNullOrEmpty(BoundedToAssetId))
            {
                arg = $"issuer={BoundedToIssuer}&rootAssetId={BoundedToAssetId}";
            }
            _embeddedIdpsManager.GetInstance(Name).InvokeRegistration(_navigationService, arg);
        });
    }
}
