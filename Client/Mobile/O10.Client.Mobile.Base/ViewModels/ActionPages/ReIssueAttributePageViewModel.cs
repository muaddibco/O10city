using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class ReIssueAttributePageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IDetailsService _detailsService;
        private readonly IAssetsService _assetsService;
        private readonly IRestClientService _restClientService;
        private readonly IPageDialogService _pageDialogService;
        private string _content;
        private string _password;
        private string _target;
        private ImageSource _photo;
        private byte[] _photoBytes;
        private bool _needPhoto;
        private string _issuer;
        private string _issuerAlias;

        public ReIssueAttributePageViewModel(INavigationService navigationService, IExecutionContext executionContext,
            IAccountsService accountsService, IDataAccessService dataAccessService, IDetailsService detailsService,
            IAssetsService assetsService, IRestClientService restClientService, IPageDialogService pageDialogService) : base(navigationService)
        {
            _executionContext = executionContext;
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _detailsService = detailsService;
            _assetsService = assetsService;
            _restClientService = restClientService;
            _pageDialogService = pageDialogService;
            IsLoading = true;
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

        public bool NeedPhoto
        {
            get => _needPhoto;
            set
            {
                SetProperty(ref _needPhoto, value);
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

        public string IssuerAlias
        {
            get => _issuerAlias;
            set
            {
                SetProperty(ref _issuerAlias, value);
            }
        }

        public DelegateCommand TakePhotoCommand => new DelegateCommand(async () =>
        {
            var photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front });

            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                photo.GetStream().CopyTo(ms);
                _photoBytes = ms.ToArray();
                Photo = ImageSource.FromStream(() => photo.GetStream());
            }
        });

        public DelegateCommand RequestCommand => new DelegateCommand(async () =>
        {
            IsLoading = true;
            AccountDescriptor account = _accountsService.GetById(_executionContext.AccountId);
            TaskCompletionSource<byte[]> bindingKeySource = _executionContext.GetBindingKeySource(Password);

            var rootAttributeDefinition = await _assetsService.GetRootAttributeDefinition(_issuer).ConfigureAwait(false);
            byte[] rootAssetId = _assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, Uri.UnescapeDataString(Content));

            byte[] bindingKey = await bindingKeySource.Task.ConfigureAwait(false);
            _assetsService.GetBlindingPoint(bindingKey, rootAssetId, out byte[] blindingPoint, out byte[] blindingFactor);

            byte[] protectionAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString(), _issuer).ConfigureAwait(false);
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
                Content = Content,
                SessionCommitment = sessionCommitment.ToHexString(),
                SignatureE = surjectionProof.Rs.E.ToHexString(),
                SignatureS = surjectionProof.Rs.S[0].ToHexString(),
                BlindingPoint = blindingPoint.ToHexString(),
                ImageContent = NeedPhoto ? Convert.ToBase64String(_photoBytes) : null
            };

            await _restClientService.Request(_target)
                .PostJsonAsync(sessionData)
                .ReceiveJson<IEnumerable<AttributeValue>>()
                .ContinueWith(t =>
                    {
                        if (t.Exception?.InnerException is FlurlHttpException flurlHttpException)
                        {
                            string url = flurlHttpException.Call.FlurlRequest.Url.ToString();
                            string body = flurlHttpException.Call.RequestBody;
                            string response = flurlHttpException.GetResponseStringAsync().Result;
                        }

                        if (t.IsCompletedSuccessfully)
                        {
                            _dataAccessService.AddOrUpdateUserIdentityIsser(_issuer, _issuerAlias, string.Empty);
                            _dataAccessService.AddNonConfirmedRootAttribute(_executionContext.AccountId, Content, _issuer, rootAttributeDefinition.SchemeName, rootAssetId);

                            IEnumerable<AttributeValue> attributeValues = t.Result;
                            List<Tuple<string, string>> associatedAttributes = new List<Tuple<string, string>>();

                            if (NeedPhoto)
                            {
                                Tuple<string, string> associatedAttribute = new Tuple<string, string>(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, Convert.ToBase64String(_photoBytes));
                                associatedAttributes.Add(associatedAttribute);
                            }

                            foreach (var attributeValue in attributeValues.Where(a => !a.Definition.IsRoot))
                            {
                                Tuple<string, string> associatedAttribute = new Tuple<string, string>(attributeValue.Definition.SchemeName, attributeValue.Value);
                                associatedAttributes.Add(associatedAttribute);
                            }

                            if (associatedAttributes.Any())
                            {
                                _dataAccessService.UpdateUserAssociatedAttributes(_executionContext.AccountId,
                                                                                  _issuer,
                                                                                  associatedAttributes,
                                                                                  rootAssetId);
                            }
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                _pageDialogService
                                    .DisplayAlertAsync(AppResources.CAP_REISSUE_ATTR_ALERT_TITLE, string.Format(AppResources.CAP_REISSUE_ATTR_FAILED, t.Exception.Message), AppResources.BTN_OK);
                            });
                        }

                        Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());
                    }, TaskScheduler.Default).ConfigureAwait(false);
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            IsLoading = true;
            base.OnNavigatedTo(parameters);

            string encoded = parameters["action"]?.ToString();

            if (!string.IsNullOrEmpty(encoded))
            {
                string detailsUri = encoded.DecodeUnescapedFromString64();
                _detailsService.GetActionDetails(detailsUri)
                    .ContinueWith(async t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            _target = t.Result.ActionUri.DecodeFromString64();
                            _issuer = t.Result.Issuer;
                            IssuerAlias = t.Result.IssuerAlias;
                            var schemeNames = await _assetsService.GetAssociatedAttributeDefinitions(t.Result.Issuer).ConfigureAwait(false);
                            NeedPhoto = schemeNames.Any(s => s.SchemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO);
                        }
                        else
                        {
                            IsError = true;
                            ErrorMessage = t.Exception.Message;
                        }

                        IsLoading = false;
                    }, TaskScheduler.Current);
            }
        }
    }
}
