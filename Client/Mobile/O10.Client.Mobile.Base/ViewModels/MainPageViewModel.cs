using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using O10.Client.Mobile.Base.Interfaces;
using O10.Core.ExtensionMethods;
using O10.Client.Common.Interfaces;
using System.Threading.Tasks.Dataflow;
using O10.Client.Mobile.Base.Models.StateNotifications;
using System.Linq;
using Xamarin.Forms;
using System.Threading.Tasks;
using O10.Core.Configuration;
using System;
using Prism.Services;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Common.Configuration;
using O10.Client.DataLayer.Model;
using O10.Client.Mobile.Base.ViewModels.Elements;
using System.Collections.Generic;
using O10.Client.Common.Entities;
using O10.Client.Mobile.Base.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.DataLayer.Services;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IStateNotificationService _stateNotificationService;
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IAssetsService _assetsService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly IPageDialogService _pageDialogService;
        private readonly IRestApiConfiguration _walletSettings;
        private readonly ILogger _logger;
        private ObservableCollection<IdentitySchemeViewModel> _identitySchemes;
        private readonly ActionBlock<string> _navigationMessagesHandler;
        private IDisposable _navigationMessagesHandlerUnsubscriber;

        public MainPageViewModel(INavigationService navigationService,
                                 IStateNotificationService stateNotificationService,
                                 IAccountsService accountsService,
                                 IExecutionContext executionContext,
                                 IDataAccessService dataAccessService,
                                 ISchemeResolverService schemeResolverService,
                                 IAssetsService assetsService,
                                 IVerifierInteractionsManager verifierInteractionsManager,
                                 IConfigurationService configurationService,
                                 ILoggerService loggerService,
                                 IPageDialogService pageDialogService) : base(navigationService)
        {
            _stateNotificationService = stateNotificationService;
            _accountsService = accountsService;
            _stateNotificationService.NotificationsPipe.LinkTo(new ActionBlock<StateNotificationBase>(ProcessStateNotification));
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _schemeResolverService = schemeResolverService;
            _assetsService = assetsService;
            _verifierInteractionsManager = verifierInteractionsManager;
            _pageDialogService = pageDialogService;
            _walletSettings = configurationService.Get<IRestApiConfiguration>();
            _logger = loggerService.GetLogger(nameof(MainPageViewModel));
            IdentitySchemes = new ObservableCollection<IdentitySchemeViewModel>();
            _navigationMessagesHandler = new ActionBlock<string>(s =>
            {
                Device.BeginInvokeOnMainThread(() => NavigationService.NavigateWithLogging(s, _logger));
            });
        }

        public ObservableCollection<IdentitySchemeViewModel> IdentitySchemes
        {
            get => _identitySchemes;
            set
            {
                SetProperty(ref _identitySchemes, value);
            }
        }

        public DelegateCommand ScanQrCommand => new DelegateCommand(() => NavigationService.NavigateAsync("./QrScanner"));

        public DelegateCommand EmbeddedIdpsCommand => new DelegateCommand(() => NavigationService.NavigateAsync("./EmbeddedIdps"));

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("redirectUri"))
            {
                string uri = parameters.GetValue<string>("redirectUri")?.DecodeUnescapedFromString64();
                NavigationService.NavigateWithLogging(uri, _logger);
                return;
            }

            if (parameters.ContainsKey("accountId"))
            {
                long accountId = long.Parse(parameters["accountId"].ToString());
                AccountDescriptor accountDescriptor = _accountsService.GetById(accountId);
                _executionContext.InitializeUtxoExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey);
            }

            _navigationMessagesHandlerUnsubscriber = _executionContext.NavigationPipe.LinkTo(_navigationMessagesHandler);

            Initalize().ContinueWith(t =>
            {
                if (_executionContext.LastExpandedKey != null)
                {
                    string issuer = _executionContext.LastExpandedKey.Split('-')[0];
                    string rootAssetId = _executionContext.LastExpandedKey.Split('-')[1];
                    var identityScheme = IdentitySchemes.FirstOrDefault(i => i.Issuer == issuer && i.RootAssetId == rootAssetId);
                    if (identityScheme != null)
                    {
                        identityScheme.IsExpanded = false;
                        bool bindingKeyValid = _executionContext.IsBindingKeyValid(_executionContext.LastExpandedKey);

                        if (bindingKeyValid)
                        {
                            identityScheme.ProcessExpandCommand.Execute();
                        }
                    }
                }
            }, TaskScheduler.Current);
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            _navigationMessagesHandlerUnsubscriber.Dispose();
            base.OnNavigatedFrom(parameters);
        }

        private async Task Initalize()
        {
            IsLoading = true;

            try
            {
                var userRootAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId);

                ObservableCollection<IdentitySchemeViewModel> identitySchemes = new ObservableCollection<IdentitySchemeViewModel>();

                foreach (var rootAttribute in userRootAttributes)
                {
                    var userAttributeViewModel = GetRootAttributeViewModel(rootAttribute);
                    IdentitySchemeViewModel identityScheme = identitySchemes.FirstOrDefault(i => i.Issuer == rootAttribute.Source && i.RootAssetId == rootAttribute.AssetId.ToHexString());
                    if (identityScheme == null)
                    {
                        identityScheme = await GetIdentitySchemeAsync(rootAttribute.Source, rootAttribute.Content, rootAttribute.AssetId.ToHexString(), rootAttribute.SchemeName).ConfigureAwait(false);
                        identitySchemes.Add(identityScheme);
                    }
                    identityScheme.RootAttributes.Add(userAttributeViewModel);
                }

                foreach (IdentitySchemeViewModel identityScheme in identitySchemes)
                {
                    SetIdentitySchemeState(identityScheme);
                }

                IdentitySchemes = identitySchemes;
            }
            catch (Exception ex)
            {
                await _pageDialogService.DisplayAlertAsync(AppResources.CAP_MAIN_PAGE_ALERT_TITLE_FAILURE, ex.Message, AppResources.BTN_OK).ConfigureAwait(false);
            }
            finally
            {
                IsLoading = false;
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
                AttributeState = userRootAttribute.IsOverriden ? Enums.AttributeState.Disabled : (userRootAttribute.LastCommitment.ToHexString() == "0000000000000000000000000000000000000000000000000000000000000000" ? Enums.AttributeState.NotConfirmed : Enums.AttributeState.Confirmed),
                CreationTime = userRootAttribute.CreationTime ?? DateTime.MinValue,
                ConfirmationTime = userRootAttribute.ConfirmationTime ?? DateTime.MinValue,
                LastUpdateTime = userRootAttribute.LastUpdateTime ?? DateTime.MinValue,
                Issuer = userRootAttribute.Source
            };
        }

        private static void SetIdentitySchemeState(IdentitySchemeViewModel identityScheme)
        {
            identityScheme.State = Enums.AttributeState.NotConfirmed;

            foreach (RootAttributeViewModel rootAttribute in identityScheme.RootAttributes)
            {
                if (rootAttribute.AttributeState == Enums.AttributeState.Confirmed)
                {
                    identityScheme.State = Enums.AttributeState.Confirmed;
                }
                else if (rootAttribute.AttributeState == Enums.AttributeState.Disabled && identityScheme.State != Enums.AttributeState.Confirmed)
                {
                    identityScheme.State = Enums.AttributeState.Disabled;
                }
            }
        }

        private async Task<IdentitySchemeViewModel> GetIdentitySchemeAsync(string issuer, string content, string assetId, string schemeName)
        {
            IdentitySchemeViewModel identityScheme = new IdentitySchemeViewModel(NavigationService, _executionContext, _dataAccessService, _verifierInteractionsManager)
            {
                Issuer = issuer,
                IssuerName = _dataAccessService.GetUserIdentityIsserAlias(issuer),
                RootAttributeContent = content,
                RootAssetId = assetId,
                SchemeName = schemeName
            };

            if (string.IsNullOrEmpty(identityScheme.IssuerName))
            {
                await _schemeResolverService.ResolveIssuer(issuer)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            _dataAccessService.AddOrUpdateUserIdentityIsser(issuer, t.Result, string.Empty);
                            identityScheme.Issuer = t.Result;
                        }
                        else
                        {
                            identityScheme.IssuerName = issuer;
                        }
                    }, TaskScheduler.Default).ConfigureAwait(false);
            }

            return identityScheme;
        }

        private async Task ProcessStateNotification(StateNotificationBase stateNotification)
        {
            try
            {
                if (stateNotification is RootAttributeAddedStateNotification rootAttributeAdded)
                {
                    IdentitySchemeViewModel identityScheme = IdentitySchemes.FirstOrDefault(i => i.Issuer == rootAttributeAdded.Attribute.Source && i.RootAssetId == rootAttributeAdded.Attribute.AssetId);
                    RootAttributeViewModel rootAttribute = identityScheme.RootAttributes.FirstOrDefault(a => a.Content == rootAttributeAdded.Attribute.Content && a.AttributeState == Enums.AttributeState.NotConfirmed);
                    rootAttribute.AttributeState = Enums.AttributeState.Confirmed;
                    rootAttribute.AttributeSchemeName = (await _assetsService.GetRootAttributeDefinition(rootAttributeAdded.Attribute.Source)).SchemeName;
                    rootAttribute.AssetId = rootAttributeAdded.Attribute.AssetId;
                    rootAttribute.Issuer = rootAttributeAdded.Attribute.Source;

                    SetIdentitySchemeState(identityScheme);
                }
                else if (stateNotification is RootAttributeDisabledStateNotification rootAttributeDisabled)
                {
                    IdentitySchemeViewModel identityScheme = IdentitySchemes.FirstOrDefault(i => i.RootAttributes.Any(r => r.AttributeId == rootAttributeDisabled.AttributeId));
                    RootAttributeViewModel rootAttribute = identityScheme.RootAttributes.FirstOrDefault(a => a.AttributeId == rootAttributeDisabled.AttributeId);
                    rootAttribute.AttributeState = Enums.AttributeState.Disabled;

                    SetIdentitySchemeState(identityScheme);
                }
                else if (stateNotification is AccountCompomisedStateNotification accountCompomised)
                {
                    Device.BeginInvokeOnMainThread(() => NavigationService.NavigateAsync("/AccountCompromised"));
                }
                else if (stateNotification is KeyImageCorruptedStateNotification keyImageCorrupted)
                {
                    Device.BeginInvokeOnMainThread(() => _pageDialogService.DisplayAlertAsync("Key Image Corrupted", $"Key Image {keyImageCorrupted.KeyImage.ToHexString()} is corrupted", AppResources.BTN_OK));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static string ResolveValue(IEnumerable<(string key1, string value1)> items, string key2, string value2 = null)
        {
            foreach (var (key1, value1) in items)
            {
                if (key1 == key2)
                {
                    return value1;
                }
            }
            return value2 ?? key2;
        }
    }
}
