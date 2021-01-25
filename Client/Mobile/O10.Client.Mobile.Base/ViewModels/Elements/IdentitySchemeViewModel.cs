using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Client.Mobile.Base.Enums;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.ViewModels.Elements;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class IdentitySchemeViewModel : BindableBase
    {
        private readonly INavigationService _navigationService;
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private string _issuerName;
        private AttributeState _state;
        private string _rootAttributeContent;
        private string _schemeName;
        private bool _isExpanded;
        private string _rootAssetId;
        private bool _hasInherenceProtection;

        public IdentitySchemeViewModel(INavigationService navigationService,
                                       IExecutionContext executionContext,
                                       IDataAccessService dataAccessService,
                                       IVerifierInteractionsManager verifierInteractionsManager)
        {
            _navigationService = navigationService;
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _verifierInteractionsManager = verifierInteractionsManager;
            RootAttributes = new List<RootAttributeViewModel>();
        }

        #region Properties

        public string Issuer { get; set; }

        public string IssuerName
        {
            get => _issuerName;
            set
            {
                SetProperty(ref _issuerName, value);
            }
        }

        public AttributeState State
        {
            get => _state;
            set
            {
                SetProperty(ref _state, value);
            }
        }

        public string RootAttributeContent
        {
            get => _rootAttributeContent;
            set
            {
                SetProperty(ref _rootAttributeContent, value);
            }
        }

        public string RootAssetId
        {
            get => _rootAssetId;
            set
            {
                SetProperty(ref _rootAssetId, value);
            }
        }

        public string SchemeName
        {
            get => _schemeName;
            set
            {
                SetProperty(ref _schemeName, value);
            }
        }

        public bool HasInherenceProtection
        {
            get => _hasInherenceProtection;
            set
            {
                SetProperty(ref _hasInherenceProtection, value);
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                SetProperty(ref _isExpanded, value);
            }
        }

        public List<RootAttributeViewModel> RootAttributes { get; set; }

        #endregion Properties

        #region Commands

        public DelegateCommand ProcessExpandCommand => new DelegateCommand(async () =>
        {
            if (IsExpanded)
            {
                IsExpanded = false;
                _executionContext.LastExpandedKey = null;
                return;
            }

            string key = $"{Issuer}-{RootAssetId}";

            var bindingKeySource = _executionContext.GetIssuerBindingKeySource(key) ?? await _executionContext.GetBindingKeySourceWithBio(key).ConfigureAwait(false);

            if (bindingKeySource != null)
            {
                _executionContext.RelationsBindingService.Initialize(bindingKeySource);
                IsExpanded = true;
                _executionContext.LastExpandedKey = key;

                var userRegistrations = _dataAccessService.GetUserRegistrations(_executionContext.AccountId);
                IEnumerable<InherenceServiceInfo> inherenceServiceInfos = _verifierInteractionsManager.GetInherenceServices();

                HasInherenceProtection = inherenceServiceInfos.Any(i =>
                    {
                        _executionContext.RelationsBindingService.GetBoundedCommitment(RootAssetId.HexStringToByteArray(), i.Target.HexStringToByteArray(), out byte[] blindingFactor, out byte[] commitment);
                        return userRegistrations.Any(r => r.Commitment == commitment.ToHexString());
                    });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => _navigationService.NavigateAsync($"Authentication?key={Issuer}-{RootAssetId}&updateLastExpanded=true"));
            }
        });

        public DelegateCommand EmbeddedIdpsCommand => new DelegateCommand(() =>
        {
            Device.BeginInvokeOnMainThread(() => _navigationService.NavigateAsync($"EmbeddedIdps?issuer={Issuer}&rootAssetId={RootAssetId}"));
        });

        public DelegateCommand InherenceProtectionCommand => new DelegateCommand(() =>
        {
            _executionContext.RelationsBindingService.Initialize(_executionContext.GetIssuerBindingKeySource($"{Issuer}-{RootAssetId}"));
            Device.BeginInvokeOnMainThread(() => _navigationService.NavigateAsync($"InherenceProtection?issuer={Issuer}&rootAssetId={RootAssetId}"));
        });

        public DelegateCommand ShowInfoCommand => new DelegateCommand(async () =>
        {
            string key = $"{Issuer}-{RootAssetId}";
            TaskCompletionSource<byte[]> bindingKeySource = _executionContext.GetIssuerBindingKeySource(key) ?? await _executionContext.GetBindingKeySourceWithBio(key).ConfigureAwait(false);

            if (bindingKeySource != null)
            {
                _executionContext.RelationsBindingService.Initialize(bindingKeySource);
                Device.BeginInvokeOnMainThread(() => _navigationService.NavigateAsync($"RootAttributeDetails?issuer={Issuer}&rootAssetId={RootAssetId}"));
            }
            else
            {
                string uri = $"RootAttributeDetails?issuer={Issuer}&rootAssetId={RootAssetId}".EncodeToEscapedString64();
                Device.BeginInvokeOnMainThread(() => _navigationService.NavigateAsync($"Authentication?key={key}&redirectUri={uri}"));
            }
        });

        #endregion Commands
    }
}
