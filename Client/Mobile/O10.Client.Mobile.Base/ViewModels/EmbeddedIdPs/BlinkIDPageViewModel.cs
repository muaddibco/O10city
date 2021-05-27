using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Flurl;
using Flurl.Http;
using Microblink.Forms.Core;
using Microblink.Forms.Core.Overlays;
using Microblink.Forms.Core.Recognizers;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Client.Common.Configuration;
using O10.Client.Common.Entities;
using O10.Client.Common.ExternalIdps;
using O10.Client.Common.ExternalIdps.BlinkId;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Configuration;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.ExtensionMethods;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models.StateNotifications;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.ViewModels.Inherence;
using Xamarin.Forms;
using DependencyService = Xamarin.Forms.DependencyService;

namespace O10.Client.Mobile.Base.ViewModels
{
    // if BlinkID is the Root Identity then photo becomes Root Inherence Protection Factor
    // if BlinkID is the Associated Identity then there can be following cases:
    //   - There is already created Root Inherence Protection Factor
    //   - There is no created Root Inherence Protection Factor
    //
    // If there is already created Root Inherence Protection Factor, following actions are done:
    //   1. Photo obtained from scanning must be attested with the Root Inherence Protection Factor
    //   2. If attestation passed then scanned photo becomes the Associated Inherence Protection Factor
    // This will be executed by the conditional registration via REST API of biometric provider
    // These steps are done during Registration Inherence factor, so it is needed to register inherence factor right after issuance of Associated Attributes.
    // This means that it is needed to pass to the page of registration of Inherence factor with the image of photo in the Buffer
    //
    // If there is no created Root Inherence Protection Factor, following actions are done:
    //   1. The user must gets registered at one of the Inherence Verifiers
    //   2. Photo obtained from scanning gets attested with the Root Inherence Protection Factor
    //   3. If attestation passed then scanned photo becomes the Associated Inherence Protection Factor
    public class BlinkIDPageViewModel : ViewModelBase
    {
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly IPageDialogService _pageDialogService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IAssetsService _assetsService;
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly IStateNotificationService _stateNotificationService;
        private readonly IO10LogicService _o10LogicService;
        private readonly ILogger _logger;

        private readonly ActionBlock<StateNotificationBase> _rootAttributeHandler;
        private IDisposable _rootAttributeHandlerUnsubscriber;

        /// <summary>
        /// Microblink scanner is used for scanning the identity documents.
        /// </summary>
        private IMicroblinkScanner _blinkID;

        /// <summary>
        /// BlinkID Combined recognizer will be used for automatic detection and data extraction from the supported document.
        /// </summary>
        private IBlinkIdCombinedRecognizer _blinkidRecognizer;
        private IPassportRecognizer _passportRecognizer;

        private ImageSource _documentFront;
        private ImageSource _documentBack;
        private ImageSource _face;
        private byte[] _faceBytes;

        private UserRootAttribute _rootAttributeMaster;
        private string _password;
        private bool _scanCompleted;
        private string _firstName;
        private string _lastName;
        private string _documentNumber;
        private string _idCardNumber;
        private string _dateOfBirth;
        private string _issuanceDate;
        private string _expirationDate;
        //private bool _suggestInherenceProtection;
        private bool _useInherenceProtection;
        private List<InherenceVerifierViewModel> _inherenceVerifiers;
        private InherenceVerifierViewModel _selectedInherenceVerifier;

        private readonly bool _scanCombined = true;
        private readonly bool _scanPassport = true;
        private string _issuerState;
        private string _nationality;
        private string _documentType;
        private TaskCompletionSource<byte[]> _bindingKeySource;

        public BlinkIDPageViewModel(INavigationService navigationService,
                                    IPageDialogService pageDialogService,
                                    IConfigurationService configurationService,
                                    IDataAccessService dataAccessService,
                                    IAssetsService assetsService,
                                    IExecutionContext executionContext,
                                    IAccountsService accountsService,
                                    IVerifierInteractionsManager verifierInteractionsManager,
                                    IStateNotificationService stateNotificationService,
                                    IO10LogicService o10LogicService,
                                    ILoggerService loggerService) : base(navigationService)
        {
            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _pageDialogService = pageDialogService;
            _dataAccessService = dataAccessService;
            _assetsService = assetsService;
            _executionContext = executionContext;
            _accountsService = accountsService;
            _verifierInteractionsManager = verifierInteractionsManager;
            _stateNotificationService = stateNotificationService;
            _o10LogicService = o10LogicService;
            _logger = loggerService.GetLogger(nameof(BlinkIDPageViewModel));

            _rootAttributeHandler = new ActionBlock<StateNotificationBase>(n =>
            {
                if (n is RootAttributeAddedStateNotification rootAttributeAdded)
                {
                    _rootAttributeHandlerUnsubscriber.Dispose();
                    _rootAttributeHandlerUnsubscriber = null;

                    try
                    {
                        _executionContext.RelationsBindingService.Initialize(_bindingKeySource);
                        var verifierInteraction = _verifierInteractionsManager.GetInstance(SelectedInherenceVerifier.Name);

                        verifierInteraction.Buffer = Convert.ToBase64String(_faceBytes);
                        verifierInteraction.InvokeRegistration(NavigationService, $"rootAttributeId={rootAttributeAdded.Attribute.UserAttributeId}");
                    }
                    catch (Exception ex)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _logger.Error($"Failed to pass to the Inherence Verifier due to error: '{ex.Message}'", ex);
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_BLINKID_ALERT_TITLE, string.Format(AppResources.CAP_BLINKID_ALERT_SETTING_INHERENCE_FAILED, ex.Message), AppResources.BTN_OK);
                            NavigationService.NavigateToRoot(_logger);
                        });
                    }
                }
            });
        }

        #region Properties

        public string DocumentType
        {
            get => _documentType;
            set
            {
                SetProperty(ref _documentType, value);
            }
        }

        public ImageSource DocumentFront
        {
            get => _documentFront;
            set
            {
                SetProperty(ref _documentFront, value);
            }
        }

        public ImageSource DocumentBack
        {
            get => _documentBack;
            set
            {
                SetProperty(ref _documentBack, value);
            }
        }

        public ImageSource Face
        {
            get => _face;
            set
            {
                SetProperty(ref _face, value);
            }
        }

        public bool ScanCompleted
        {
            get => _scanCompleted;
            set
            {
                SetProperty(ref _scanCompleted, value);
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
            }
        }

        public string DocumentNumber
        {
            get => _documentNumber;
            set
            {
                SetProperty(ref _documentNumber, value);
            }
        }

        public string IdCardNumber
        {
            get => _idCardNumber;
            set
            {
                SetProperty(ref _idCardNumber, value);
            }
        }

        public string DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                SetProperty(ref _dateOfBirth, value);
            }
        }

        public string IssuanceDate
        {
            get => _issuanceDate;
            set
            {
                SetProperty(ref _issuanceDate, value);
            }
        }

        public string ExpirationDate
        {
            get => _expirationDate;
            set
            {
                SetProperty(ref _expirationDate, value);
            }
        }

        public string IssuerState
        {
            get => _issuerState;
            set
            {
                SetProperty(ref _issuerState, value);
            }
        }

        public string Nationality
        {
            get => _nationality;
            set
            {
                SetProperty(ref _nationality, value);
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

        //public bool SuggestInherenceProtection
        //{
        //    get => _suggestInherenceProtection;
        //    set
        //    {
        //        SetProperty(ref _suggestInherenceProtection, value);
        //    }
        //}

        public bool UseInherenceProtection
        {
            get => _useInherenceProtection;
            set
            {
                SetProperty(ref _useInherenceProtection, value);
            }
        }

        public List<InherenceVerifierViewModel> InherenceVerifiers
        {
            get => _inherenceVerifiers;
            set
            {
                SetProperty(ref _inherenceVerifiers, value);
            }
        }

        public InherenceVerifierViewModel SelectedInherenceVerifier
        {
            get => _selectedInherenceVerifier;
            set
            {
                SetProperty(ref _selectedInherenceVerifier, value);
            }
        }

        #endregion Properties

        public DelegateCommand StartScanCommand => new DelegateCommand(() =>
        {
            IOverlaySettings overlaySettings = InitializeOverlaySettings();

            _blinkID.Scan(overlaySettings);
        });

        public DelegateCommand ConfimRequestCommand => new DelegateCommand(async () =>
        {
            if (ScanCompleted)
            {
                IsLoading = true;
                string issuerBlinkID = null;
                string actionUri = null;
                Dictionary<string, string> attributes = null;

                _bindingKeySource = _executionContext.GetBindingKeySource(Password);

                BlinkIdIdentityRequest translateRequest = CreateTranslateRequest();

                await _restApiConfiguration
                    .ExternalIdpsUri
                    .AppendPathSegments("TranslateToAttributes", $"BlinkID-{DocumentType}")
                    .PostJsonAsync(translateRequest)
                    .ReceiveJson<TranslationResponse>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            issuerBlinkID = t.Result.Issuer;
                            actionUri = t.Result.ActionUri;
                            attributes = t.Result.Attributes;
                        }
                        else
                        {
                            _logger.Error("Failed to translate attributes", t.Exception.InnerException);

                            Device.BeginInvokeOnMainThread(() => _pageDialogService.DisplayAlertAsync(AppResources.CAP_BLINKID_ALERT_TITLE, string.Format(AppResources.CAP_BLINKID_ALERT_ATTRIBUTES_TRANSLATION_FAILED, t.Exception.InnerException.Message), AppResources.BTN_OK));
                        }
                    }, TaskScheduler.Current)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(actionUri))
                {
                    AccountDescriptor account = _accountsService.GetById(_executionContext.AccountId);

                    var rootAttributeScheme = await _assetsService.GetRootAttributeDefinition(issuerBlinkID).ConfigureAwait(false);

                    byte[] blindingPointRootToRoot = null;

                    if (_rootAttributeMaster != null)
                    {
                        byte[] blindingPointRoot = _assetsService.GetBlindingPoint(await _bindingKeySource.Task.ConfigureAwait(false), _rootAttributeMaster.AssetId);
                        blindingPointRootToRoot = _assetsService.GetCommitmentBlindedByPoint(_rootAttributeMaster.AssetId, blindingPointRoot);
                    }

                    string rootAttributeContent = attributes.FirstOrDefault(a => a.Key == rootAttributeScheme.AttributeName).Value;
                    byte[] rootAssetId = _assetsService.GenerateAssetId(rootAttributeScheme.SchemeId, rootAttributeContent);

                    IssueAttributesRequestDTO request = new IssueAttributesRequestDTO
                    {
                        Attributes = await GenerateAttributeValuesAsync(attributes, rootAssetId, rootAttributeScheme.SchemeName, issuerBlinkID, blindingPointRootToRoot).ConfigureAwait(false),
                        PublicSpendKey = _rootAttributeMaster == null ? account.PublicSpendKey.ToHexString() : null,
                        PublicViewKey = _rootAttributeMaster == null ? account.PublicViewKey.ToHexString() : null,
                    };

                    if (_rootAttributeMaster == null)
                    {
                        // Need only in case when _rootAttribute is null, i.e. BlinkID is the Root Attribute
                        // =======================================================================================================================
                        byte[] protectionAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString(), issuerBlinkID).ConfigureAwait(false);
                        _assetsService.GetBlindingPoint(await _bindingKeySource.Task.ConfigureAwait(false), rootAssetId, protectionAssetId, out byte[] blindingPoint, out byte[] blindingFactor);
                        byte[] protectionAssetNonBlindedCommitment = Crypto.ConfidentialAssets.CryptoHelper.GetNonblindedAssetCommitment(protectionAssetId);
                        byte[] protectionAssetCommitment = Crypto.ConfidentialAssets.CryptoHelper.SumCommitments(protectionAssetNonBlindedCommitment, blindingPoint);
                        byte[] sessionBlindingFactor = Crypto.ConfidentialAssets.CryptoHelper.GetRandomSeed();
                        byte[] sessionCommitment = Crypto.ConfidentialAssets.CryptoHelper.GetAssetCommitment(sessionBlindingFactor, protectionAssetId);
                        byte[] diffBlindingFactor = Crypto.ConfidentialAssets.CryptoHelper.GetDifferentialBlindingFactor(sessionBlindingFactor, blindingFactor);
                        SurjectionProof surjectionProof = Crypto.ConfidentialAssets.CryptoHelper.CreateSurjectionProof(sessionCommitment, new byte[][] { protectionAssetCommitment }, 0, diffBlindingFactor);
                        // =======================================================================================================================

                        byte[] bindingKey = await _bindingKeySource.Task.ConfigureAwait(false);
                        byte[] blindingPointAssociatedToParent = _assetsService.GetBlindingPoint(bindingKey, rootAssetId);
                        request.Attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, new IssueAttributesRequestDTO.AttributeValue
                        {
                            BlindingPointValue = blindingPoint,
                            BlindingPointRoot = blindingPointAssociatedToParent,
                            Value = rootAssetId.ToHexString()
                        });

                        request.SessionCommitment = sessionCommitment.ToHexString();
                        request.SignatureE = surjectionProof.Rs.E.ToHexString();
                        request.SignatureS = surjectionProof.Rs.S[0].ToHexString();
                    }


                    string req = JsonConvert.SerializeObject(request);

                    await (await actionUri.PostJsonAsync(request)
                        .ReceiveJson<IEnumerable<AttributeValue>>()
                        .ContinueWith(async t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                if (_rootAttributeMaster == null && UseInherenceProtection)
                                {
                                    _rootAttributeHandlerUnsubscriber = _stateNotificationService.NotificationsPipe.LinkTo(_rootAttributeHandler);
                                }

                                List<Tuple<string, string>> associatedAttributes = new List<Tuple<string, string>>();
                                var attributeValues = t.Result;

                                var rootAttr = attributeValues.FirstOrDefault(a => a.Definition.IsRoot);
                                byte[] rootAssetId = null;

                                if (_rootAttributeMaster == null && rootAttr != null)
                                {
                                    var rootAttributeScheme = await _assetsService.GetRootAttributeDefinition(issuerBlinkID).ConfigureAwait(false);
                                    rootAssetId = _assetsService.GenerateAssetId(rootAttributeScheme.SchemeId, rootAttr.Value);
                                    _dataAccessService.AddNonConfirmedRootAttribute(_executionContext.AccountId, rootAttr.Value, issuerBlinkID, rootAttributeScheme.SchemeName, rootAssetId);
                                }

                                await _o10LogicService
                                    .StoreAssociatedAttributes(
                                        _rootAttributeMaster?.Source ?? issuerBlinkID,
                                        _rootAttributeMaster?.AssetId ?? rootAssetId,
                                        issuerBlinkID,
                                        attributeValues.Where(a => !a.Definition.IsRoot))
                                    .ConfigureAwait(false);

                                if (_rootAttributeMaster != null && UseInherenceProtection)
                                {
                                    var photoAttribute = _dataAccessService.GetUserAssociatedAttributes(_executionContext.AccountId)
                                                                            .FirstOrDefault(a =>
                                                                                a.RootAssetId.Equals32(_rootAttributeMaster.AssetId) &&
                                                                                a.Source == issuerBlinkID &&
                                                                                a.AttributeSchemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO);
                                    var verifierInteraction = _verifierInteractionsManager.GetInstance(SelectedInherenceVerifier.Name);

                                    verifierInteraction.Buffer = Convert.ToBase64String(_faceBytes);
                                    await verifierInteraction.InvokeRegistration(NavigationService, $"rootAttributeId={_rootAttributeMaster.UserAttributeId}&associatedAttributeId={photoAttribute.UserAssociatedAttributeId}").ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                _logger.Error($"Failed to issue attributes due to error {t.Exception.InnerException.Message}", t.Exception.InnerException);

                                Device.BeginInvokeOnMainThread(() => _pageDialogService.DisplayAlertAsync(AppResources.CAP_BLINKID_ALERT_TITLE, string.Format(AppResources.CAP_BLINKID_ALERT_ATTRIBUTES_ISSUANCE_FAILED, t.Exception.InnerException.Message), AppResources.BTN_OK));
                            }

                            if (!UseInherenceProtection)
                            {
                                Device.BeginInvokeOnMainThread(() => NavigationService.NavigateToRoot(_logger));
                            }
                        }, TaskScheduler.Current).ConfigureAwait(false)).ConfigureAwait(false);
                }

                IsLoading = false;
            }
        });

        private IOverlaySettings InitializeOverlaySettings()
        {
            List<IRecognizer> recognizers = new List<IRecognizer>();

            if (_scanCombined)
            {
                // license keys must be set before creating Recognizer, otherwise InvalidLicenseKeyException will be thrown
                // the following code creates and sets up implementation of MrtdRecognizer
                _blinkidRecognizer = DependencyService.Get<IBlinkIdCombinedRecognizer>(DependencyFetchTarget.NewInstance);
                _blinkidRecognizer.ReturnFullDocumentImage = true;
                _blinkidRecognizer.ReturnFaceImage = true;
                _blinkidRecognizer.SkipUnsupportedBack = true;

                // the following code creates and sets up implementation of UsdlRecognizer
                //var usdlRecognizer = DependencyService.Get<IUsdlRecognizer>(DependencyFetchTarget.NewInstance);

                // success frame grabber recognizer must be constructed with reference to its slave recognizer,
                // so we need to use factory to avoid DependencyService's limitations
                //usdlSuccessFrameGrabberRecognizer = DependencyService.Get<ISuccessFrameGrabberRecognizerFactory>(DependencyFetchTarget.NewInstance).CreateSuccessFrameGrabberRecognizer(usdlRecognizer);

                recognizers.Add(_blinkidRecognizer);
            }

            if (_scanPassport)
            {
                _passportRecognizer = DependencyService.Get<IPassportRecognizer>(DependencyFetchTarget.NewInstance);
                _passportRecognizer.ReturnFaceImage = true;
                _passportRecognizer.ReturnFullDocumentImage = true;

                recognizers.Add(_passportRecognizer);
            }
            // first create a recognizer collection from all recognizers that you want to use in recognition
            // if some recognizer is wrapped with SuccessFrameGrabberRecognizer, then you should use only the wrapped one
            var recognizerCollection = DependencyService.Get<IRecognizerCollectionFactory>().CreateRecognizerCollection(recognizers.ToArray()/*, usdlSuccessFrameGrabberRecognizer*/);

            // using recognizerCollection, create overlay settings that will define the UI that will be used
            // there are several available overlay settings classes in Microblink.Forms.Core.Overlays namespace
            // document overlay settings is best for scanning identity documents
            var overlaySettings = DependencyService.Get<IBlinkIdOverlaySettingsFactory>().CreateBlinkIdOverlaySettings(recognizerCollection);

            return overlaySettings;
        }

        private async Task<Dictionary<string, IssueAttributesRequestDTO.AttributeValue>> GenerateAttributeValuesAsync(Dictionary<string, string> attributes, byte[] rootAssetId, string rootSchemeName, string issuer, byte[] blindingPointRootToRoot)
        {
            byte[] bindingKey = await _bindingKeySource.Task.ConfigureAwait(false);
            byte[] blindingPointAssociatedToParent = _assetsService.GetBlindingPoint(bindingKey, rootAssetId);
            return attributes
                    .Select(kv =>
                        new KeyValuePair<string, IssueAttributesRequestDTO.AttributeValue>(
                            kv.Key,
                            new IssueAttributesRequestDTO.AttributeValue
                            {
                                Value = kv.Value,
                                BlindingPointValue = _assetsService.GetBlindingPoint(bindingKey, rootAssetId, AsyncUtil.RunSync(async () => await _assetsService.GenerateAssetId(kv.Key, kv.Value, issuer).ConfigureAwait(false))),
                                BlindingPointRoot = kv.Key == rootSchemeName ? blindingPointRootToRoot : blindingPointAssociatedToParent
                            }))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("issuer") && parameters.ContainsKey("rootAssetId"))
            {
                string issuer = parameters.GetValue<string>("issuer");
                string rootAssetId = parameters.GetValue<string>("rootAssetId");
                byte[] assetId = rootAssetId.HexStringToByteArray();
                _rootAttributeMaster = _dataAccessService.GetUserAttributes(_executionContext.AccountId).FirstOrDefault(a => a.Source == issuer && a.AssetId.Equals32(assetId));
            }

            InitializeInherenceVerifiers();

            InitializerMicroBlink();
            StartScanCommand.Execute();
        }

        private async Task ParseDrivingLicenseResults()
        {
            if (_blinkidRecognizer.Result.ResultState == RecognizerResultState.Valid)
            {
                DocumentType = "DrivingLicense";
                var blinkidResult = _blinkidRecognizer.Result;

                var fullDocumentFrontImageSource = blinkidResult.FullDocumentFrontImage;
                var fullDocumentBackImageSource = blinkidResult.FullDocumentBackImage;
                var faceImageSource = blinkidResult.FaceImage;

                var streamImageSource = (StreamImageSource)faceImageSource;
                var stream = await streamImageSource.Stream(CancellationToken.None).ConfigureAwait(false);
                _faceBytes = new byte[stream.Length];
                int pos = 0;
                do
                {
                    int countToRead = (int)Math.Min(4096L, stream.Length - pos);
                    int readCount = stream.Read(_faceBytes, pos, countToRead);
                    pos += readCount;
                } while (pos < _faceBytes.Length);

                FirstName = blinkidResult.FirstName;
                LastName = blinkidResult.LastName;
                DocumentNumber = blinkidResult.DocumentNumber;
                IdCardNumber = blinkidResult.PersonalIdNumber;
                DateOfBirth = new DateTime(blinkidResult.DateOfBirth.Year, blinkidResult.DateOfBirth.Month, blinkidResult.DateOfBirth.Day).ToShortDateString();
                IssuanceDate = new DateTime(blinkidResult.DateOfIssue.Year, blinkidResult.DateOfIssue.Month, blinkidResult.DateOfIssue.Day).ToShortDateString();
                ExpirationDate = new DateTime(blinkidResult.DateOfExpiry.Year, blinkidResult.DateOfExpiry.Month, blinkidResult.DateOfExpiry.Day).ToShortDateString();
                DocumentFront = fullDocumentFrontImageSource;
                DocumentBack = fullDocumentBackImageSource;
                Face = faceImageSource;
            }
        }

        private async Task ParsePassportResults()
        {
            if (_passportRecognizer.Result.ResultState == RecognizerResultState.Valid && _passportRecognizer.Result.MrzResult.DocumentType == MrtdDocumentType.Passport)
            {
                DocumentType = "Passport";
                var blinkidResult = _passportRecognizer.Result;

                var fullDocumentFrontImageSource = blinkidResult.FullDocumentImage;
                var faceImageSource = blinkidResult.FaceImage;

                var streamImageSource = (StreamImageSource)faceImageSource;
                var stream = await streamImageSource.Stream(CancellationToken.None).ConfigureAwait(false);
                _faceBytes = new byte[stream.Length];
                int pos = 0;
                do
                {
                    int countToRead = (int)Math.Min(4096L, stream.Length - pos);
                    int readCount = stream.Read(_faceBytes, pos, countToRead);
                    pos += readCount;
                } while (pos < _faceBytes.Length);

                FirstName = blinkidResult.MrzResult.SecondaryId;
                LastName = blinkidResult.MrzResult.PrimaryId;
                DocumentNumber = blinkidResult.MrzResult.SanitizedDocumentNumber;
                IdCardNumber = blinkidResult.MrzResult.SanitizedOpt1;
                IssuerState = blinkidResult.MrzResult.SanitizedIssuer;
                Nationality = blinkidResult.MrzResult.Nationality;
                DateOfBirth = new DateTime(blinkidResult.MrzResult.DateOfBirth.Year, blinkidResult.MrzResult.DateOfBirth.Month, blinkidResult.MrzResult.DateOfBirth.Day).ToShortDateString();
                //IssuanceDate = new DateTime(blinkidResult.DateOfIssue.Year, blinkidResult.DateOfIssue.Month, blinkidResult.DateOfIssue.Day).ToShortDateString();
                ExpirationDate = new DateTime(blinkidResult.MrzResult.DateOfExpiry.Year, blinkidResult.MrzResult.DateOfExpiry.Month, blinkidResult.MrzResult.DateOfExpiry.Day).ToShortDateString();
                DocumentFront = fullDocumentFrontImageSource;
                Face = faceImageSource;
            }
        }

        private void InitializerMicroBlink()
        {
            // before obtaining any of the recognizer's implementations from DependencyService, it is required
            // to obtain instance of IMicroblinkScanner and set the license key.
            // Failure to do so will crash your app.
            var microblinkFactory = DependencyService.Get<IMicroblinkScannerFactory>();

            string licenseKey = GetLicenseKey();

            // since DependencyService requires implementations to have default constructor, a factory is needed
            // to construct implementation of IMicroblinkScanner with given license key
            _blinkID = microblinkFactory.CreateMicroblinkScanner(licenseKey);

            // subscribe to scanning done message
            MessagingCenter.Subscribe<Messages.ScanningDoneMessage>(this, Messages.ScanningDoneMessageId, async (sender) =>
            {
                // if user canceled scanning, sender.ScanningCancelled will be true
                if (!sender.ScanningCancelled)
                {
                    // otherwise, one or more recognizers used in RecognizerCollection (see StartScan method below)
                    // will contain result

                    // if specific recognizer's result's state is Valid, then it contains data recognized from image
                    if (_scanCombined)
                    {
                        await ParseDrivingLicenseResults().ConfigureAwait(false);
                    }
                    if (_scanPassport)
                    {
                        await ParsePassportResults().ConfigureAwait(false);
                    }

                    ScanCompleted = true;

                    if (Face != null)
                    {
                        UseInherenceProtection = true;
                        if (_rootAttributeMaster == null)
                        {
                            // Add scanned face photo as biometric protection
                        }
                        else
                        {
                            // verify scanned face photo matches to existing biometric protection in case it exists or create new protection from the scanned photo
                        }
                    }

                    // similarly, we can check for results of other recognizers
                    //if (usdlRecognizer.Result.ResultState == RecognizerResultState.Valid)
                    //{
                    //    var result = usdlRecognizer.Result;
                    //    stringResult = 
                    //        "USDL version: " + result.GetField(UsdlKeys.StandardVersionNumber) + "\n" +
                    //        "Family name: " + result.GetField(UsdlKeys.CustomerFamilyName) + "\n" +
                    //        "First name: " + result.GetField(UsdlKeys.CustomerFirstName) + "\n" +
                    //        "Date of birth: " + result.GetField(UsdlKeys.DateOfBirth) + "\n" +
                    //        "Sex: " + result.GetField(UsdlKeys.Sex) + "\n" +
                    //        "Eye color: " + result.GetField(UsdlKeys.EyeColor) + "\n" +
                    //        "Height: " + result.GetField(UsdlKeys.Height) + "\n" +
                    //        "Street: " + result.GetField(UsdlKeys.AddressStreet) + "\n" +
                    //        "City: " + result.GetField(UsdlKeys.AddressCity) + "\n" +
                    //        "Jurisdiction: " + result.GetField(UsdlKeys.AddressJurisdictionCode) + "\n" +
                    //        "Postal code: " + result.GetField(UsdlKeys.AddressPostalCode) + "\n" +
                    //          // License information
                    //          "Issue date: " + result.GetField(UsdlKeys.DocumentIssueDate) + "\n" +
                    //          "Expiration date: " + result.GetField(UsdlKeys.DocumentExpirationDate) + "\n" +
                    //          "Issuer ID: " + result.GetField(UsdlKeys.IssuerIdentificationNumber) + "\n" +
                    //          "Jurisdiction version: " + result.GetField(UsdlKeys.JurisdictionVersionNumber) + "\n" +
                    //          "Vehicle class: " + result.GetField(UsdlKeys.JurisdictionVehicleClass) + "\n" +
                    //          "Restrictions: " + result.GetField(UsdlKeys.JurisdictionRestrictionCodes) + "\n" +
                    //          "Endorsments: " + result.GetField(UsdlKeys.JurisdictionEndorsementCodes) + "\n" +
                    //          "Customer ID: " + result.GetField(UsdlKeys.CustomerIdNumber);

                    //    successFrameImageSource = usdlSuccessFrameGrabberRecognizer.Result.SuccessFrame;
                    //}
                }
            });
        }

        private BlinkIdIdentityRequest CreateTranslateRequest()
        {
            return new BlinkIdIdentityRequest
            {
                DocumentNationality = "Israel",
                DocumentType = DocumentType,
                DocumentNumber = DocumentNumber,
                LocalIdNumber = IdCardNumber,
                FirstName = FirstName,
                LastName = LastName,
                DateOfBirth = string.IsNullOrEmpty(DateOfBirth) ? (DateTime?)null : DateTime.Parse(DateOfBirth),
                IssuanceDate = string.IsNullOrEmpty(IssuanceDate) ? (DateTime?)null : DateTime.Parse(IssuanceDate),
                ExpirationDate = string.IsNullOrEmpty(ExpirationDate) ? (DateTime?)null : DateTime.Parse(ExpirationDate),
                IssuerState = IssuerState,
                Nationality = Nationality
            };
        }

        private static string GetLicenseKey()
        {
            // license keys are different for iOS and Android and depend on iOS bundleID/Android application ID
            // in your app, you may obtain the correct license key for your platform via DependencyService from
            // your Droid/iOS projects
            string licenseKey;

            // both these license keys are demo license keys for bundleID/applicationID com.microblink.xamarin.blinkid
            if (Device.RuntimePlatform == Device.iOS)
            {
                licenseKey = "sRwAAAEWY29tLndpc3RuZXR3b3JrLndhbGxldBNXZH6MY95xnPs8lnPVcz6YYyy8BJnXXvyw+umaCTiB1X89pqAJtiaj8eH4j5sa24e/1fMPeYyV6eezPScDeGeDtQFi0AyxS0tWuTYyCyJefl4nX6v9u421cu8dzP3AwBGKtaShZ/IaSiwtWYw/yQH6CUgwKx52Tf8g8n59fB3b8Rt2udCc9ugyk1FWJRmU6hU0NE1oMVhoN7ewGQ1Mh39+hZe+vYNb1o4C866KDN73OFAwXICU6Brt1xGCo2uxvq+BG3nTO9jQxw5MeQqidA==";
            }
            else
            {
                licenseKey = "sRwAAAAWY29tLndpc3RuZXR3b3JrLndhbGxldGK+noiABaF+6t9qbP1kTE+5dqQEnoN2+zXkxhdYOO2v4lyliNZ6tv5kTjkoG/+manIonOm0sLfAKWFQ6iPZ7lmgPdzmg5fRK5hdiLTktrZ9fUNBAKo7ILlq7h5c2tF6swTDu5hbU/u7hsYVSvYLWqrmJGFLi5OQA1SSKR3pWTeB5qnBw0Bo9kh0j7Mg1yjfY7KotXZwRVnM4ocM6mQ63ysepb0AXVrdU2GEdkdgVz/c3s7oOe5XgSGcfKcLrbSTaZIArqphg9OGo2ytmMDQQQ==";
            }

            return licenseKey;
        }

        private void InitializeInherenceVerifiers()
        {
            try
            {
                IEnumerable<InherenceServiceInfo> inherenceServiceInfos = _verifierInteractionsManager.GetInherenceServices();

                InherenceVerifiers = inherenceServiceInfos
                    .Where(i => _verifierInteractionsManager.GetInstance(i.Name) != null)
                    .Select(i =>
                    {
                        return new InherenceVerifierViewModel(_verifierInteractionsManager, NavigationService)
                        {
                            Name = i.Name,
                            Alias = i.Alias,
                            Description = i.Description,
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize {nameof(InherenceProtectionPageViewModel)}", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_INHERENCE_PROTECTION_ALERT_TITLE, AppResources.CAP_INHERENCE_PROTECTION_ALERT_MSG, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
