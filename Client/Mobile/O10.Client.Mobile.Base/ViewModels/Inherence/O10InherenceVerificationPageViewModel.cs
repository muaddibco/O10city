using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.IO;
using O10.Client.DataLayer.Services;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.Services.Inherence;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class O10InherenceVerificationPageViewModel : ViewModelBase
    {
        private readonly IO10InherenceConfiguration _o10InherenceConfiguration;
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IVerifierInteractionService _verifierInteractionService;
        private readonly ILogger _logger;
        private ImageSource _photo;
        private byte[] _photoBytes;
        private long _attributeId;

        public O10InherenceVerificationPageViewModel(INavigationService navigationService,
                                                      IExecutionContext executionContext,
                                                      IVerifierInteractionsManager verifierInteractionsManager,
                                                      IDataAccessService dataAccessService,
                                                      IConfigurationService configurationService,
                                                      ILoggerService loggerService,
                                                      IPageDialogService pageDialogService) : base(navigationService)
        {
            _o10InherenceConfiguration = configurationService.Get<IO10InherenceConfiguration>();
            _executionContext = executionContext;
            _verifierInteractionService = verifierInteractionsManager.GetInstance("O10Inherence");
            _dataAccessService = dataAccessService;
            _pageDialogService = pageDialogService;
            _logger = loggerService.GetLogger(nameof(O10InherenceVerificationPageViewModel));
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
            var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions() { DefaultCamera = CameraDevice.Front, SaveToAlbum = false, SaveMetaData = false }).ConfigureAwait(false);

            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                photo.GetStream().CopyTo(ms);
                _photoBytes = ms.ToArray();
                Photo = ImageSource.FromStream(() => photo.GetStream());
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());
            }

            IsLoading = true;

            VerificationResult verificationResult = await _verifierInteractionService.Verify(_attributeId, _photoBytes).ConfigureAwait(false);

            IsLoading = false;

            if (verificationResult.SignedVerification != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    NavigationService.GoBackAsync(
                        new NavigationParameters
                        {
                                { "rootAttributeId", _attributeId.ToString() },
                                { "publicKey", verificationResult.SignedVerification.PublicKey },
                                { "signature", verificationResult.SignedVerification.Signature}
                        });
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, string.Format(AppResources.CAP_O10INHERENCE_ALERT_VERIFICATION_FAILED, verificationResult.ErrorMessage), AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            _attributeId = long.Parse(parameters.GetValue<string>("rootAttributeId"));

            TakePhotoCommand.Execute();
        }
    }
}
