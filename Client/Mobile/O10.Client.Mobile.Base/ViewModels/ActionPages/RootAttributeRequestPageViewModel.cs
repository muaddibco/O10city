using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Plugin.Media;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Client.Mobile.Base.Dtos;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class RootAttributeRequestPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IRestClientService _restClientService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IAssetsService _assetsService;
        private string _content;
        private string _password;
        private string _passwordConfirm;
        private string _target;
        private ImageSource _photo;
        private byte[] _photoBytes;

        public RootAttributeRequestPageViewModel(INavigationService navigationService, IExecutionContext executionContext,
            IAccountsService accountsService, IDataAccessService dataAccessService, IRestClientService restClientService,
            IPageDialogService pageDialogService, IAssetsService assetsService) : base(navigationService)
        {
            _executionContext = executionContext;
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _restClientService = restClientService;
            _pageDialogService = pageDialogService;
            _assetsService = assetsService;
        }

        public string Content
        {
            get => _content;
            set
            {
                SetProperty(ref _content, value);
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

        public ImageSource Photo
        {
            get => _photo;
            set
            {
                SetProperty(ref _photo, value);
            }
        }

        public DelegateCommand TakePhotoCommand => new DelegateCommand(async () =>
        {
            if (Password != PasswordConfirm)
            {
                await _pageDialogService.DisplayAlertAsync(AppResources.CAP_ROOT_ATTR_REQ_ALERT_TITLE, AppResources.CAP_ROOT_ATTR_REQ_PASSWORD_MISMATCH, AppResources.BTN_OK);
                return;
            }

            var photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front });

            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                photo.GetStream().CopyTo(ms);
                _photoBytes = ms.ToArray();
                Photo = ImageSource.FromStream(() => photo.GetStream());

                await RequestRootIdentity();
            }
        });

        public DelegateCommand RequestCommand => new DelegateCommand(async () => await RequestRootIdentity());

        private async Task RequestRootIdentity()
        {
            AccountDescriptor account = _accountsService.GetById(_executionContext.AccountId);

            TaskCompletionSource<byte[]> bindingKeySource = _executionContext.GetBindingKeySource(Password);

            IssuerActionDetails actionDetails = await _executionContext.GetActionDetails(_target).ConfigureAwait(false);

            var rootAttributeDefinition = await _assetsService.GetRootAttributeDefinition(actionDetails.Issuer).ConfigureAwait(false);
            byte[] rootAssetId = _assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, Content);

            byte[] bindingKey = await bindingKeySource.Task.ConfigureAwait(false);
            _assetsService.GetBlindingPoint(bindingKey, rootAssetId, out byte[] blindingPoint, out byte[] blindingFactor);

            IdentityRequestDto identityRequest = new IdentityRequestDto
            {
                RequesterPublicSpendKey = account.PublicSpendKey.ToHexString(),
                RequesterPublicViewKey = account.PublicViewKey.ToHexString(),
                RootAttributeContent = Content,
                BlindingPoint = blindingPoint.ToHexString(),
                FaceImageContent = _photoBytes?.ToHexString() ?? string.Empty
            };

            try
            {
                HttpResponseMessage httpResponse = await _restClientService.Request(actionDetails.ActionUri.DecodeFromString64()).PostJsonAsync(identityRequest).ConfigureAwait(false);
                if (httpResponse.IsSuccessStatusCode)
                {
                    byte[] assetId = _assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, Content);
                    _dataAccessService.AddNonConfirmedRootAttribute(_executionContext.AccountId, Content, actionDetails.Issuer, rootAttributeDefinition.SchemeName, assetId);
                    //TODO: this step should be done if Identity Provider API returned OK
                    _dataAccessService.UpdateUserAssociatedAttributes(_executionContext.AccountId,
                                                                      actionDetails.Issuer,
                                                                      new List<Tuple<string, string>>
                                                                      {
                                                                          new Tuple<string, string>(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, _photoBytes?.ToHexString())
                                                                      },
                                                                      assetId);
                }
            }
            catch (Exception ex)
            {
                await _pageDialogService.DisplayAlertAsync(AppResources.CAP_ROOT_ATTR_REQ_ALERT_TITLE, ex.Message, AppResources.BTN_OK);
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            string encoded = parameters["action"]?.ToString();

            if (!string.IsNullOrEmpty(encoded))
            {
                _target = encoded.DecodeUnescapedFromString64();
            }
        }
    }
}
