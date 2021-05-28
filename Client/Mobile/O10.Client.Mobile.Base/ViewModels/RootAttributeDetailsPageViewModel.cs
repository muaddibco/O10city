using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Enums;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.ViewModels.Elements;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class RootAttributeDetailsPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService _pageDialogService;
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IAssetsService _assetsService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly ILogger _logger;
        private string _issuerName;
        private AttributeState _state;
        private string _rootAttributeContent;
        private string _schemeName;
        private string _issuer;
        private string _rootAssetId;
        private List<RootAttributeViewModel> _rootAttributes;
        private List<AssociatedAttributesViewModel> _associatedAttributes;
        private bool _inherenceProtectionCommandEnabled;
        private bool _hasInherenceProtection;

        public RootAttributeDetailsPageViewModel(INavigationService navigationService,
                                                 IPageDialogService pageDialogService,
                                                 IExecutionContext executionContext,
                                                 IDataAccessService dataAccessService,
                                                 IAssetsService assetsService,
                                                 ISchemeResolverService schemeResolverService,
                                                 IVerifierInteractionsManager verifierInteractionsManager,
                                                 ILoggerService loggerService)
            : base(navigationService)
        {
            _pageDialogService = pageDialogService;
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _assetsService = assetsService;
            _schemeResolverService = schemeResolverService;
            _verifierInteractionsManager = verifierInteractionsManager;
            _logger = loggerService.GetLogger(nameof(RootAttributeDetailsPageViewModel));
        }

        #region Properties

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

        public string SchemeName
        {
            get => _schemeName;
            set
            {
                SetProperty(ref _schemeName, value);
            }
        }

        public List<RootAttributeViewModel> RootAttributes
        {
            get => _rootAttributes;
            set
            {
                SetProperty(ref _rootAttributes, value);
            }
        }

        public List<AssociatedAttributesViewModel> AssociatedAttributes
        {
            get => _associatedAttributes;
            set
            {
                SetProperty(ref _associatedAttributes, value);
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

        public bool InherenceProtectionCommandEnabled
        {
            get => _inherenceProtectionCommandEnabled;
            set
            {
                SetProperty(ref _inherenceProtectionCommandEnabled, value);
            }
        }

        #endregion Properties

        #region Commands

        public DelegateCommand EmbeddedIdpsCommand => new DelegateCommand(() =>
        {
            Device.BeginInvokeOnMainThread(() => NavigationService.NavigateAsync($"EmbeddedIdps?issuer={_issuer}&rootAssetId={_rootAssetId}"));
        });

        public DelegateCommand InherenceProtectionCommand => new DelegateCommand(() =>
        {
            _executionContext.RelationsBindingService.Initialize(_executionContext.GetIssuerBindingKeySource($"{_issuer}-{_rootAssetId}"));

            Device.BeginInvokeOnMainThread(() => NavigationService.NavigateAsync($"InherenceProtection?rootAttributeId={RootAttributes.FirstOrDefault(r => r.AttributeState == AttributeState.Confirmed).AttributeId}"));
        });

        public DelegateCommand ShowInfoCommand => new DelegateCommand(() =>
        {
            Device.BeginInvokeOnMainThread(() => NavigationService.NavigateAsync($"IdentityDetails?issuer={_issuer}&assetId={_rootAssetId}"));
        });

        #endregion Commands

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("issuer"))
            {
                _issuer = parameters.GetValue<string>("issuer");
            }

            if (parameters.ContainsKey("rootAssetId"))
            {
                _rootAssetId = parameters.GetValue<string>("rootAssetId");
            }

            Initialize();
        }

        private async Task Initialize()
        {
            await Task.Yield();

            IsLoading = true;

            _executionContext.RelationsBindingService.Initialize(_executionContext.GetIssuerBindingKeySource($"{_issuer}-{_rootAssetId}"));

            try
            {
                byte[] rootAssetId = _rootAssetId.HexStringToByteArray();

                var userRootAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId)?
                                            .Where(a => a.Source == _issuer && a.AssetId.Equals32(rootAssetId));
                var userAssociatedAttributes = _dataAccessService.GetUserAssociatedAttributes(_executionContext.AccountId)
                                            .Where(a => a.RootAssetId.Equals32(rootAssetId));

                List<string> issuers = userAssociatedAttributes.Select(a => a.Source).Union(userAssociatedAttributes.Select(a => a.Source)).Distinct().ToList();
                Dictionary<string, Dictionary<string, string>> schemeNamesMap =
                    issuers.ToDictionary(
                        s => s,
                        s => AsyncUtil.RunSync(() =>
                            _assetsService.GetAssociatedAttributeDefinitions(s))
                            .ToDictionary(k => k.SchemeName, v => v.Alias));

                await SetIssuerName().ConfigureAwait(false);

                RootAttributeContent = userRootAttributes.FirstOrDefault()?.Content;
                SchemeName = userRootAttributes.FirstOrDefault()?.SchemeName;

                List<RootAttributeViewModel> rootAttributes = new List<RootAttributeViewModel>();
                foreach (var userRootAttribute in userRootAttributes)
                {
                    RootAttributeViewModel rootAttribute = GetRootAttributeViewModel(userRootAttribute);

                    rootAttributes.Add(rootAttribute);
                }

                RootAttributes = rootAttributes;

                List<AssociatedAttributesViewModel> associatedAttributesViewModels = GetAssociatedAttributeViewModels(_rootAssetId.HexStringToByteArray(), userAssociatedAttributes, schemeNamesMap);

                AssociatedAttributes = associatedAttributesViewModels;
                //identityScheme.PhotoAssociatedAttributeViewModel = associatedAttributeViewModels.FirstOrDefault(a => a.SchemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO);
                SetIdentitySchemeState();

                var userRegistrations = _dataAccessService.GetUserRegistrations(_executionContext.AccountId);
                IEnumerable<InherenceServiceInfo> inherenceServiceInfos = _verifierInteractionsManager.GetInherenceServices();

                HasInherenceProtection = inherenceServiceInfos.Any(i =>
                {
                    _executionContext.RelationsBindingService.GetBoundedCommitment(rootAssetId, i.Target.HexStringToByteArray(), out byte[] blindingFactor, out byte[] commitment);
                    return userRegistrations.Any(r => r.Commitment == commitment.ToHexString());
                });


                InherenceProtectionCommandEnabled = RootAttributes.Any(r => r.AttributeState == AttributeState.Confirmed);
            }
            catch (Exception ex)
            {
                _logger.Error("Initialization failed", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_ROOTATTRDET_ALERT_TITLE, string.Format(AppResources.CAP_ROOTATTRDET_ALERT_INIT_FAILED, ex.Message), AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SetIssuerName()
        {
            IssuerName = _dataAccessService.GetUserIdentityIsserAlias(_issuer);
            if (string.IsNullOrEmpty(IssuerName))
            {
                await _schemeResolverService.ResolveIssuer(_issuer)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            _dataAccessService.AddOrUpdateUserIdentityIsser(_issuer, t.Result, string.Empty);
                            IssuerName = t.Result;
                        }
                        else
                        {
                            IssuerName = _issuer;
                        }
                    }, TaskScheduler.Default).ConfigureAwait(false);
            }
        }

        private List<AssociatedAttributesViewModel> GetAssociatedAttributeViewModels(byte[] rootAssetId, IEnumerable<UserAssociatedAttribute> associatedAttributes, Dictionary<string, Dictionary<string, string>> schemeNamesMap)
        {
            List<AssociatedAttributesViewModel> associatedAttributeViewModels = associatedAttributes
                .Where(a => a.RootAssetId.Equals32(rootAssetId))
                .GroupBy(a => a.Source)
                .Select(g => new AssociatedAttributesViewModel(schemeNamesMap[g.Key]
                    .Where(kv => kv.Key != AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD)
                    .Select(kv =>
                    {
                        UserAssociatedAttribute attr = g.FirstOrDefault(a => a.AttributeSchemeName == kv.Key);
                        string content = attr?.Content ?? string.Empty;

                        if (attr != null && AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO.Equals(kv.Key))
                        {
                            byte[] associatedAssetId = AsyncUtil.RunSync(async () => await _assetsService.GenerateAssetId(kv.Key, attr.Content, attr.Source).ConfigureAwait(false));
                            var userRegistrations = _dataAccessService.GetUserRegistrations(_executionContext.AccountId);

                            IEnumerable<InherenceServiceInfo> inherenceServiceInfos = _verifierInteractionsManager.GetInherenceServices();

                            bool hasInherenceProtection = inherenceServiceInfos.Any(i =>
                            {
                                byte[] commitment;
                                if (associatedAssetId.Equals32(rootAssetId))
                                {
                                    (_, byte[] registrationCommitment) = AsyncUtil.RunSync(async () => await _executionContext.RelationsBindingService.GetBoundedCommitment(i.Target.HexStringToByteArray(), associatedAssetId).ConfigureAwait(false));
                                    commitment = registrationCommitment;
                                }
                                else
                                {
                                    (_, byte[] registrationCommitment) = AsyncUtil.RunSync(async () => await _executionContext.RelationsBindingService.GetBoundedCommitment(i.Target.HexStringToByteArray(), associatedAssetId, rootAssetId).ConfigureAwait(false));
                                    commitment = registrationCommitment;
                                }

                                return userRegistrations.Any(r => r.Commitment == commitment.ToHexString());
                            });

                            if (!hasInherenceProtection)
                            {
                                content = string.Empty;
                            }
                        }

                        return new AssociatedAttributeViewModel
                        {
                            Alias = kv.Value,
                            SchemeName = kv.Key,
                            Content = content
                        };
                    }))
                {
                    IssuerName = _dataAccessService.GetUserIdentityIsserAlias(g.Key)
                })
                .ToList();

            return associatedAttributeViewModels;
        }

        private void SetIdentitySchemeState()
        {
            State = AttributeState.NotConfirmed;

            foreach (RootAttributeViewModel rootAttribute in RootAttributes)
            {
                if (rootAttribute.AttributeState == AttributeState.Confirmed)
                {
                    State = AttributeState.Confirmed;
                }
                else if (rootAttribute.AttributeState == AttributeState.Disabled && State != AttributeState.Confirmed)
                {
                    State = AttributeState.Disabled;
                }
            }
        }

        private static RootAttributeViewModel GetRootAttributeViewModel(UserRootAttribute userRootAttribute)
        {
            return new RootAttributeViewModel
            {
                AttributeId = userRootAttribute.UserAttributeId,
                AssetId = userRootAttribute.AssetId.ToHexString(),
                AttributeSchemeName = userRootAttribute.SchemeName,
                Content = userRootAttribute.Content,
                AttributeState = userRootAttribute.IsOverriden ? AttributeState.Disabled : (userRootAttribute.LastCommitment.ToHexString() == "0000000000000000000000000000000000000000000000000000000000000000" ? AttributeState.NotConfirmed : AttributeState.Confirmed),
                CreationTime = userRootAttribute.CreationTime ?? DateTime.MinValue,
                ConfirmationTime = userRootAttribute.ConfirmationTime ?? DateTime.MinValue,
                LastUpdateTime = userRootAttribute.LastUpdateTime ?? DateTime.MinValue,
                Issuer = userRootAttribute.Source
            };
        }
    }
}
