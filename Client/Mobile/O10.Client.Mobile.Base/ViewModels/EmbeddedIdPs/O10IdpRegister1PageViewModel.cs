using Prism.Commands;
using Prism.Navigation;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Services.EmbeddedIdPs;
using Flurl;
using Flurl.Http;
using System.Threading.Tasks;
using Prism.Services;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.ExtensionMethods;
using O10.Client.Mobile.Base.ViewModels.EmbeddedIdPs;
using System;
using Xamarin.Forms;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Entities;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Crypto.ConfidentialAssets;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.ExtensionMethods;
using O10.Core.Cryptography;
using System.Collections.Generic;
using System.Linq;
using O10.Core;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class O10IdpRegister1PageViewModel : ViewModelBase
    {
        private readonly IO10IdpConfiguration _o10IdpConfiguration;
        private readonly ILogger _logger;
        private readonly IRestClientService _restClientService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IAccountsService _accountsService;
        private readonly IAssetsService _assetsService;
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private string _email;
        private string _password;
        private bool _isAccountExist;
        private bool _accountChecked;

        public O10IdpRegister1PageViewModel(INavigationService navigationService, IRestClientService restClientService, IConfigurationService configurationService,
            ILoggerService loggerService, IPageDialogService pageDialogService, IAccountsService accountsService,
            IAssetsService assetsService, IExecutionContext executionContext, IDataAccessService dataAccessService) : base(navigationService)
        {
            _o10IdpConfiguration = configurationService.Get<IO10IdpConfiguration>();
            _logger = loggerService.GetLogger(nameof(O10IdpRegister1PageViewModel));
            _restClientService = restClientService;
            _pageDialogService = pageDialogService;
            _accountsService = accountsService;
            _assetsService = assetsService;
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
        }

        #region Properties

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }

        public bool IsAccountExist
        {
            get => _isAccountExist;
            set
            {
                SetProperty(ref _isAccountExist, value);
            }
        }

        public bool AccountChecked
        {
            get => _accountChecked;
            set
            {
                SetProperty(ref _accountChecked, value);
            }
        }

        #endregion Properties

        #region Commands

        public DelegateCommand CheckAccountCommand => new DelegateCommand(async () =>
        {
            IsLoading = true;
            ActionDescription = AppResources.CAP_O10IDP_CHECK_REGISTRATION;

            string uri = _o10IdpConfiguration.ApiUri.AppendPathSegment("IsAccountExist").SetQueryParam("email", Email);
            await _restClientService
                .Request(uri)
                .GetJsonAsync<O10IdpAccountExistResponse>()
                .ContinueWith(t =>
                {
                    ActionDescription = string.Empty;
                    IsLoading = false;
                    if (t.IsCompletedSuccessfully)
                    {
                        AccountChecked = true;
                        IsAccountExist = t.Result.Exist;
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, AppResources.CAP_O10IDP_ALERT_ACCOUNTCHECK_FAILED, AppResources.BTN_OK);
                            NavigationService.NavigateToRoot(_logger);
                        });
                    }
                }, TaskScheduler.Current)
                .ConfigureAwait(false);
        });

        public DelegateCommand RequestReissueCommand => new DelegateCommand(async () =>
        {
            IsLoading = true;
            TaskCompletionSource<byte[]> bindingKeySource = _executionContext.GetBindingKeySource(Password);

            ActionDescription = AppResources.CAP_O10IDP_OBTAIN_DETAILS;
            IssuerActionDetails actionDetails = await _executionContext.GetActionDetails(_o10IdpConfiguration.ApiUri.AppendPathSegments("ReissuanceDetails", Guid.NewGuid().ToString())).ConfigureAwait(false);
            ActionDescription = AppResources.CAP_O10IDP_CALCULATING_CRYPTO;

            AccountDescriptor account = _accountsService.GetById(_executionContext.AccountId);

            var rootAttributeDefinition = await _assetsService.GetRootAttributeDefinition(actionDetails.Issuer).ConfigureAwait(false);
            byte[] rootAssetId = _assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, Email);

            //_assetsService.GetBlindingPoint(await bindingKeySource.Task.ConfigureAwait(false), rootAssetId, out byte[] blindingPoint, out byte[] blindingFactor);
            byte[] protectionAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString(), actionDetails.Issuer).ConfigureAwait(false);
            byte[] blindingPoint = _assetsService.GetBlindingPoint(await bindingKeySource.Task.ConfigureAwait(false), rootAssetId, protectionAssetId);
            byte[] blindingFactor = _assetsService.GetBlindingFactor(await bindingKeySource.Task.ConfigureAwait(false), rootAssetId, protectionAssetId);
            byte[] protectionAssetNonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(protectionAssetId);
            byte[] protectionAssetCommitment = CryptoHelper.SumCommitments(protectionAssetNonBlindedCommitment, blindingPoint);
            byte[] sessionBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] sessionCommitment = CryptoHelper.GetAssetCommitment(sessionBlindingFactor, protectionAssetId);
            byte[] diffBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(sessionBlindingFactor, blindingFactor);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(sessionCommitment, new byte[][] { protectionAssetCommitment }, 0, diffBlindingFactor);

            IdentityBaseData sessionData = new IdentityBaseData
            {
                PublicSpendKey = account.PublicSpendKey.ToHexString(),
                PublicViewKey = account.PublicViewKey.ToHexString(),
                Content = Email,
                SessionCommitment = sessionCommitment.ToHexString(),
                SignatureE = surjectionProof.Rs.E.ToHexString(),
                SignatureS = surjectionProof.Rs.S[0].ToHexString(),
                BlindingPoint = blindingPoint.ToHexString(),
                ImageContent = null
            };

            ActionDescription = AppResources.CAP_O10IDP_REQUEST_REISSUE;

            await _restClientService.Request(actionDetails.ActionUri.DecodeFromString64())
                .PostJsonAsync(sessionData)
                .ReceiveJson<IEnumerable<AttributeValue>>()
                .ContinueWith(t =>
                {
                    ActionDescription = string.Empty;

                    if (t.IsCompletedSuccessfully)
                    {
                        _dataAccessService.AddNonConfirmedRootAttribute(_executionContext.AccountId, Email, actionDetails.Issuer, rootAttributeDefinition.SchemeName, rootAssetId);

                        IEnumerable<AttributeValue> attributeValues = t.Result;
                        List<Tuple<string, string>> associatedAttributes = new List<Tuple<string, string>>();

                        foreach (var attributeValue in attributeValues.Where(a => !a.Definition.IsRoot))
                        {
                            Tuple<string, string> associatedAttribute = new Tuple<string, string>(attributeValue.Definition.SchemeName, attributeValue.Value);
                            associatedAttributes.Add(associatedAttribute);
                        }

                        if (associatedAttributes.Count > 0)
                        {
                            _dataAccessService.UpdateUserAssociatedAttributes(_executionContext.AccountId, actionDetails.Issuer, associatedAttributes, rootAssetId);
                        }
                    }
                    else
                    {
                        string response = null;

                        if (t.Exception?.InnerException is FlurlHttpException flurlHttpException)
                        {
                            string url = flurlHttpException.Call.FlurlRequest.Url.ToString();
                            string body = flurlHttpException.Call.RequestBody;
                            response = AsyncUtil.RunSync(async () => await flurlHttpException.GetResponseStringAsync().ConfigureAwait(false));
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _pageDialogService
                                .DisplayAlertAsync(AppResources.CAP_REISSUE_ATTR_ALERT_TITLE, string.Format(AppResources.CAP_REISSUE_ATTR_FAILED, response), AppResources.BTN_OK);
                        });
                    }
                    IsLoading = false;

                    Device.BeginInvokeOnMainThread(() => NavigationService.NavigateToRoot(_logger));
                }, TaskScheduler.Default).ConfigureAwait(false);
        });

        public DelegateCommand ConfirmCommand => new DelegateCommand(async () =>
        {
            IsLoading = true;
            ActionDescription = AppResources.CAP_O10IDP_REQUEST_MAIL;
            try
            {
                await _o10IdpConfiguration.ApiUri
                .AppendPathSegment("RegisterWithEmail")
                .PostJsonAsync(new ActivationEmail { Email = Email, Passphrase = Password, BaseUri = _o10IdpConfiguration.ConfirmationUri })
                .ContinueWith(t =>
                {
                    ActionDescription = string.Empty;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, string.Format(AppResources.CAP_O10IDP_ALERT_MAIL_SEND_SUCCEEDED, Email), AppResources.BTN_OK);
                        }
                        else
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, AppResources.CAP_O10IDP_ALERT_MAIL_SEND_FAILED, AppResources.BTN_OK);
                        }
                        NavigationService.NavigateToRoot(_logger);
                    });
                }, TaskScheduler.Current)
                .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("Registration failed", ex);
                Device.BeginInvokeOnMainThread(() => _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, string.Format(AppResources.CAP_O10IDP_ALERT_REGISTRATION_FAILURE, ex.Message), AppResources.BTN_OK));
                NavigationService.NavigateToRoot(_logger);
            }
            finally
            {
                IsLoading = false;
            }
        });

        #endregion Commands
    }
}
