using Prism.Navigation;
using System.Collections.Generic;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.ViewModels.EmbeddedIdPs;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class EmbeddedIdpsPageViewModel : ViewModelBase
    {
        private readonly IEmbeddedIdpsManager _embeddedIdpsManager;
        private List<EmbeddedIdpViewModel> _embeddedIdps;

        public EmbeddedIdpsPageViewModel(INavigationService navigationService, IEmbeddedIdpsManager embeddedIdpsManager) : base(navigationService)
        {
            _embeddedIdpsManager = embeddedIdpsManager;
        }

        public List<EmbeddedIdpViewModel> EmbeddedIdpList
        {
            get => _embeddedIdps;
            set
            {
                SetProperty(ref _embeddedIdps, value);
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            List<EmbeddedIdpViewModel> embeddedIdps = new List<EmbeddedIdpViewModel>();

            string issuer = parameters.GetValue<string>("issuer");
            string rootAssetId = parameters.GetValue<string>("rootAssetId");

            foreach (var item in _embeddedIdpsManager.GetAllServices())
            {
                embeddedIdps.Add(new EmbeddedIdpViewModel(_embeddedIdpsManager, NavigationService)
                {
                    Name = item.Name,
                    Alias = item.Alias,
                    Description = item.Description,
                    BoundedToIssuer = issuer,
                    BoundedToAssetId = rootAssetId
                });
            }

            EmbeddedIdpList = embeddedIdps;
        }
    }
}
