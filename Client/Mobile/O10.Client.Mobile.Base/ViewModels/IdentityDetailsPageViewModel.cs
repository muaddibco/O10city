using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.ViewModels.Elements;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class IdentityDetailsPageViewModel : ViewModelBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContext _executionContext;
        private readonly IToastService _toastService;
        private List<RootAttributeDetailsViewModel> _rootAttributes;
        private string _issuer;
        private string _issuerVarbinary;
        private readonly string _title;

        public IdentityDetailsPageViewModel(INavigationService navigationService,
                                            IDataAccessService dataAccessService,
                                            IExecutionContext executionContext,
                                            IToastService toastService) : base(navigationService)
        {
            _dataAccessService = dataAccessService;
            _executionContext = executionContext;
            _toastService = toastService;
        }

        public string Issuer
        {
            get => _issuer;
            set
            {
                SetProperty(ref _issuer, value);
            }
        }

        public string IssuerVarbinary
        {
            get => _issuerVarbinary;
            set
            {
                SetProperty(ref _issuerVarbinary, value);
            }
        }

        public List<RootAttributeDetailsViewModel> RootAttributes
        {
            get => _rootAttributes;
            set
            {
                SetProperty(ref _rootAttributes, value);
            }
        }

        public DelegateCommand<string> CopyToClipboardCommand => new DelegateCommand<string>(s =>
        {
            Clipboard.SetTextAsync(s)
            .ContinueWith(t =>
            {
                Device.BeginInvokeOnMainThread(() => _toastService.LongMessage(string.Format(AppResources.MSG_COPIED_TO_CP, s)));
            }, TaskScheduler.Current);
        });

        public DelegateCommand<string> ShareCommand => new DelegateCommand<string>(s => Share.RequestAsync(new ShareTextRequest(s)));

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            Issuer = parameters["issuer"].ToString();
            string rootAssetId = parameters["assetId"].ToString();
            IssuerVarbinary = Issuer.ToVarBinary();

            var userRootAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId);

            List<RootAttributeDetailsViewModel> rootAttributeDetails = new List<RootAttributeDetailsViewModel>();

            foreach (var userRootAttribute in userRootAttributes.Where(r => r.Source == Issuer && r.AssetId.Equals32(rootAssetId.HexStringToByteArray())))
            {
                var vm = GetRootAttributeViewModel(userRootAttribute);
                rootAttributeDetails.Add(vm);
            }

            string content = rootAttributeDetails.FirstOrDefault(r => !string.IsNullOrEmpty(r.Content))?.Content;

            if (!string.IsNullOrEmpty(content))
            {
                Title = string.Format(AppResources.CAP_IDENTITY_DET_TITLE, content);
            }
            else
            {
                Title = AppResources.CAP_IDENTITY_DET_TITLE2;
            }

            RootAttributes = rootAttributeDetails;
        }

        private static RootAttributeDetailsViewModel GetRootAttributeViewModel(UserRootAttribute userRootAttribute)
        {
            return new RootAttributeDetailsViewModel
            {
                AttributeId = userRootAttribute.UserAttributeId,
                AssetId = userRootAttribute.AssetId.ToHexString(),
                AttributeSchemeName = userRootAttribute.SchemeName,
                Content = userRootAttribute.Content,
                AttributeState = userRootAttribute.IsOverriden ? Enums.AttributeState.Disabled : (userRootAttribute.LastCommitment.ToHexString() == "0000000000000000000000000000000000000000000000000000000000000000" ? Enums.AttributeState.NotConfirmed : Enums.AttributeState.Confirmed),
                CreationTime = userRootAttribute.CreationTime ?? DateTime.MinValue,
                ConfirmationTime = userRootAttribute.ConfirmationTime ?? DateTime.MinValue,
                LastUpdateTime = userRootAttribute.LastUpdateTime ?? DateTime.MinValue,
                Issuer = userRootAttribute.Source,
                IssuanceCommitment = userRootAttribute.AnchoringOriginationCommitment.ToHexString(),
                IssuanceCommitmentVarbinary = userRootAttribute.AnchoringOriginationCommitment.ToHexString().ToVarBinary(),
                OriginalCommitment = userRootAttribute.IssuanceCommitment.ToHexString(),
                OriginalCommitmentVarbinary = userRootAttribute.IssuanceCommitment.ToHexString().ToVarBinary(),
                OriginalBlindingFactor = userRootAttribute.OriginalBlindingFactor.ToHexString(),
                LastBlindingFactor = userRootAttribute.LastBlindingFactor.ToHexString(),
                LastCommitment = userRootAttribute.LastCommitment.ToHexString(),
                LastDestinationKey = userRootAttribute.LastDestinationKey.ToHexString(),
                LastTransactionKey = userRootAttribute.LastTransactionKey.ToHexString(),
                NextKeyImage = userRootAttribute.NextKeyImage,
                NextKeyImageVarbinary = userRootAttribute.NextKeyImage.ToVarBinary()
            };
        }
    }
}
