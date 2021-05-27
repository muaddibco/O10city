using System;
using System.Collections.Generic;
using System.Text;
using Prism.Commands;
using Prism.Navigation;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;
using Flurl.Http;
using O10.Core.ExtensionMethods;
using Prism.Services;
using O10.Client.Mobile.Base.Resx;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.Logging;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class O10IdpRegistrationPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IAssetsService _assetsService;
        private readonly IPageDialogService _pageDialogService;
        private readonly ILogger _logger;
        private string _rootAttributeContent;
        private string _oneTimePassphrase;
        private string _password;
        private string _passwordConfirm;
        private string _targetUri;

        private TaskCompletionSource<byte[]> _bindingKeySource;

        public O10IdpRegistrationPageViewModel(INavigationService navigationService, IExecutionContext executionContext,
            IAccountsService accountsService, IDataAccessService dataAccessService,
            IAssetsService assetsService, IPageDialogService pageDialogService, ILoggerService loggerService) : base(navigationService)
        {
            _executionContext = executionContext;
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _assetsService = assetsService;
            _pageDialogService = pageDialogService;
            _logger = loggerService.GetLogger(nameof(O10IdpRegistrationPageViewModel));
        }

        #region Properties

        public string RootAttributeContent
        {
            get => _rootAttributeContent;
            set
            {
                SetProperty(ref _rootAttributeContent, value);
            }
        }

        public string OneTimePassphrase
        {
            get => _oneTimePassphrase;
            set
            {
                SetProperty(ref _oneTimePassphrase, value);
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

        public string PasswordConfirm
        {
            get => _passwordConfirm;
            set
            {
                SetProperty(ref _passwordConfirm, value);
            }
        }

        #endregion Properties

        public DelegateCommand ConfirmRegistrationCommand => new DelegateCommand(async () =>
        {
            if (Password != PasswordConfirm)
            {
                Device.BeginInvokeOnMainThread(() => _pageDialogService.DisplayAlertAsync(AppResources.CAP_REGISTER_IDENTITY_ALERT_TITLE, AppResources.CAP_REGISTER_IDENTITY_ALERT_PASSWORDS_MISMATCH, AppResources.BTN_OK));
                return;
            }

            IsLoading = true;

            _bindingKeySource = _executionContext.GetBindingKeySource(Password);

            AccountDescriptor account = _accountsService.GetById(_executionContext.AccountId);
            IssuerActionDetails actionDetails = await _executionContext.GetActionDetails(_targetUri).ConfigureAwait(false);
            byte[] rootAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, RootAttributeContent, actionDetails.Issuer).ConfigureAwait(false);
            byte[] sessionBlindingFactor = CryptoHelper.ReduceScalar32(CryptoHelper.FastHash256(Encoding.ASCII.GetBytes(OneTimePassphrase)));
            byte[] sessionCommitment = CryptoHelper.BlindAssetCommitment(CryptoHelper.GetNonblindedAssetCommitment(rootAssetId), sessionBlindingFactor);
            byte[] protectionAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString(), actionDetails.Issuer).ConfigureAwait(false);

            try
            {
                var request = new IssueAttributesRequestDTO
                {
                    Content = RootAttributeContent,
                    Attributes = await GenerateAttributesAsync(rootAssetId, protectionAssetId).ConfigureAwait(false),
                    SessionCommitment = sessionCommitment.ToHexString(),
                    PublicSpendKey = account.PublicSpendKey.ToHexString(),
                    PublicViewKey = account.PublicViewKey.ToHexString()
                };

                await actionDetails.ActionUri.DecodeFromString64()
                    .PostJsonAsync(request)
                    .ContinueWith(t =>
                    {
                        if (t.Result.IsSuccessStatusCode)
                        {
                            _dataAccessService.AddNonConfirmedRootAttribute(_executionContext.AccountId, RootAttributeContent, actionDetails.Issuer, AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, rootAssetId);
                        }

                        Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());
                    }, TaskScheduler.Default).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is FlurlHttpException httpException)
                {
                    string msg = await httpException.GetResponseStringAsync().ConfigureAwait(false);
                    _logger.Error($"Sending identity attribute failed due to {msg}", httpException);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, string.Format(AppResources.CAP_O10IDP_ALERT_SENDING_ATTR_FAILED_MSG, msg), AppResources.BTN_OK);
                        NavigationService.GoBackAsync();
                    });
                }
                else
                {
                    _logger.Error("Sending identity attribute failed", ex);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, AppResources.CAP_O10IDP_ALERT_SENDING_ATTR_FAILED, AppResources.BTN_OK);
                        NavigationService.GoBackAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Sending identity attribute failed", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10IDP_ALERT_TITLE, AppResources.CAP_O10IDP_ALERT_SENDING_ATTR_FAILED, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            finally
            {
                IsLoading = false;
            }
        });

        private async Task<Dictionary<string, IssueAttributesRequestDTO.AttributeValue>> GenerateAttributesAsync(byte[] rootAssetId, byte[] attrAssetId)
            => new Dictionary<string, IssueAttributesRequestDTO.AttributeValue>
            {
                {
                    AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                    new IssueAttributesRequestDTO.AttributeValue
                    {
                        Value = null,
                        BlindingPointRoot = _assetsService.GetBlindingPoint(await _bindingKeySource.Task.ConfigureAwait(false), rootAssetId),
                        BlindingPointValue = _assetsService.GetBlindingPoint(await _bindingKeySource.Task.ConfigureAwait(false), rootAssetId, attrAssetId)
                    }
                }
            };

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            _targetUri = parameters["action"].ToString().DecodeUnescapedFromString64();
        }
    }
}
