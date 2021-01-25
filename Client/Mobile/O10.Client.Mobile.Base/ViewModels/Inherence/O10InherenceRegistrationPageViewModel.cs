using Flurl.Http;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Communication;
using O10.Client.Common.Configuration;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.Configuration;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.ExtensionMethods;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.Services.Inherence;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class O10InherenceRegistrationPageViewModel : ViewModelBase
    {
        private const string NAME = "O10Inherence";

        private readonly IO10InherenceConfiguration _o10InherenceConfiguration;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly IRestClientService _restClientService;
        private readonly IO10InherenceService _o10InherenceService;
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IGatewayService _gatewayService;
        private readonly IO10LogicService _communicationService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IAssetsService _assetsService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IVerifierInteractionService _verifierInteractionService;
        private readonly IO10LogicService _o10LogicService;
        private readonly IPageDialogService _pageDialogService;
        private readonly ILogger _logger;
        private ImageSource _photo;
        private byte[] _photoBytes;
        private byte[] _photoBytes2;
        private bool _isRegisterEnabled;
        private UserRootAttribute _rootAttribute;
        private UserAssociatedAttribute _associatedAttribute;
        private byte[] _target;
        private string _password;
        private bool _isLocked;

        public O10InherenceRegistrationPageViewModel(INavigationService navigationService,
                                                IConfigurationService configurationService,
                                                IRestClientService restClientService,
                                                IO10InherenceService o10InherenceService,
                                                IExecutionContext executionContext,
                                                IDataAccessService dataAccessService,
                                                IGatewayService gatewayService,
                                                IO10LogicService communicationService,
                                                ISchemeResolverService schemeResolverService,
                                                IAssetsService assetsService,
                                                IVerifierInteractionsManager verifierInteractionsManager,
                                                IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                                IO10LogicService o10LogicService,
                                                ILoggerService loggerService,
                                                IPageDialogService pageDialogService)
            : base(navigationService)
        {
            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _o10InherenceConfiguration = configurationService.Get<IO10InherenceConfiguration>();
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _restClientService = restClientService;
            _o10InherenceService = o10InherenceService;
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _gatewayService = gatewayService;
            _communicationService = communicationService;
            _schemeResolverService = schemeResolverService;
            _assetsService = assetsService;
            _verifierInteractionService = verifierInteractionsManager.GetInstance(NAME);
            _o10LogicService = o10LogicService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _pageDialogService = pageDialogService;
            _logger = loggerService.GetLogger(nameof(O10InherenceRegistrationPageViewModel));
        }

        #region Properties

        public ImageSource Photo
        {
            get => _photo;
            set
            {
                SetProperty(ref _photo, value);
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

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                SetProperty(ref _isLocked, value);
            }
        }

        public bool IsRegisterEnabled
        {
            get => _isRegisterEnabled;
            set
            {
                SetProperty(ref _isRegisterEnabled, value);
            }
        }

        #endregion Properties

        #region Commands

        public DelegateCommand TakePhotoCommand => new DelegateCommand(async () =>
        {
            bool available = await CrossMedia.Current.Initialize().ConfigureAwait(false);

            var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions() { DefaultCamera = CameraDevice.Front, SaveToAlbum = false, SaveMetaData = false });

            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                photo.GetStream().CopyTo(ms);
                _photoBytes = ms.ToArray();
                Photo = ImageSource.FromStream(() => photo.GetStream());
            }

            IsRegisterEnabled = _photo != null;
        });

        public DelegateCommand RegisterCommand => new DelegateCommand(async () =>
        {
            IsLoading = true;

            if (!string.IsNullOrEmpty(Password))
            {
                await _executionContext.RelationsBindingService.Initialize(Password).ConfigureAwait(false);
            }

            try
            {
                byte[] sessionKey = ConfidentialAssetsHelper.GetRandomSeed();

                (byte[] bf, byte[] commitmentToRoot, SurjectionProof proofToRegistration) = await GenerateRegistrationCommitmentAndProof(_target, _rootAttribute.AssetId).ConfigureAwait(false);

                byte[] issuer = _rootAttribute.Source.HexStringToByteArray();
                Random random = new Random(BitConverter.ToInt32(bf, 0));
                byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);
                SurjectionProof eligibilityProof = StealthTransactionsService.CreateEligibilityProof(_rootAttribute.OriginalCommitment, _rootAttribute.OriginalBlindingFactor, issuanceCommitments, bf, commitmentToRoot, random);

                RequestInput requestInput = new RequestInput
                {
                    AssetId = _rootAttribute.AssetId,
                    Issuer = _rootAttribute.Source.HexStringToByteArray(),
                    PrevAssetCommitment = _rootAttribute.LastCommitment,
                    PrevBlindingFactor = _rootAttribute.LastBlindingFactor,
                    PrevDestinationKey = _rootAttribute.LastDestinationKey,
                    PrevTransactionKey = _rootAttribute.LastTransactionKey,
                    PublicSpendKey = _target,
                    AssetCommitment = commitmentToRoot,
                    BlindingFactor = bf
                };

                UniversalProofs universalProofs = GenerateMainUniversalProofs(sessionKey, commitmentToRoot, proofToRegistration, eligibilityProof);

                byte[] commitmentToAssociated = null;
                byte[] associatedAssetId = null;
                if (_associatedAttribute != null)
                {
                    (associatedAssetId, commitmentToAssociated) = await EnrichWithAssociatedProofs(universalProofs, bf, _rootAttribute.AssetId).ConfigureAwait(false);
                }

                await _o10LogicService.SendUniversalTransport(requestInput, universalProofs, "O10 Inherence").ConfigureAwait(false);

                await RequestO10InherenceServer(sessionKey, commitmentToRoot, associatedAssetId, commitmentToAssociated).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                string response = await ex.Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.Error($"Failure while {nameof(RequestO10InherenceServer)}, response: {response}", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, string.Format(AppResources.CAP_O10INHERENCE_ALERT_GENERALFAILURE, response), AppResources.BTN_OK);
                    NavigationService.NavigateToRoot(_logger);
                });
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is FlurlHttpException flurlEx)
                {
                    string response = await flurlEx.Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger.Error($"Failure while {nameof(RequestO10InherenceServer)}, response: {response}", flurlEx);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, string.Format(AppResources.CAP_O10INHERENCE_ALERT_GENERALFAILURE, response), AppResources.BTN_OK);
                        NavigationService.NavigateToRoot(_logger);
                    });
                }
                else
                {
                    _logger.Error("Failed to register biometric", ex.InnerException);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, string.Format(AppResources.CAP_O10INHERENCE_ALERT_GENERALFAILURE, ex.InnerException.Message), AppResources.BTN_OK);
                        NavigationService.NavigateToRoot(_logger);
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to register biometric", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, string.Format(AppResources.CAP_O10INHERENCE_ALERT_GENERALFAILURE, ex.Message), AppResources.BTN_OK);
                    NavigationService.NavigateToRoot(_logger);
                });
            }
            finally
            {
                IsLoading = false;
            }
        });

        private UniversalProofs GenerateMainUniversalProofs(byte[] sessionKey, byte[] commitmentToRoot, SurjectionProof proofToRegistration, SurjectionProof eligibilityProof)
        {
            IKey issuerKey = _identityKeyProvider.GetKey(_rootAttribute.Source.HexStringToByteArray());
            UniversalProofs universalProofs = new UniversalProofs
            {
                SessionKey = sessionKey.ToHexString(),
                Mission = UniversalProofsMission.Authentication,
                Issuer = issuerKey,
                IssuersAttributes = new System.Collections.Generic.List<AttributesByIssuer>
                    {
                        new AttributesByIssuer
                        {
                            Issuer = issuerKey,
                            RootAttribute = new AttributeProofs
                            {
                                SchemeName = _rootAttribute.SchemeName,
                                Commitment = _identityKeyProvider.GetKey(commitmentToRoot),
                                BindingProof = eligibilityProof,
                                CommitmentProof = new CommitmentProof
                                {
                                    SurjectionProof = proofToRegistration
                                }
                            }
                        }
                    }
            };

            return universalProofs;
        }

        private async Task<(byte[] associatedAssetId, byte[] commitmentToAssociated)> EnrichWithAssociatedProofs(UniversalProofs universalProofs, byte[] bf, byte[] rootAssetId)
        {
            byte[] assetId = await _assetsService.GenerateAssetId(_associatedAttribute.AttributeSchemeName, _associatedAttribute.Content, _associatedAttribute.Source).ConfigureAwait(false);
            (_, byte[] commitmentToAssociated, SurjectionProof proofToRegistrationAssociated) = await GenerateRegistrationCommitmentAndProof(_target, assetId, rootAssetId, bf).ConfigureAwait(false);
            var bindingToRootProof = await _assetsService.GenerateBindingProofToRoot(bf, _rootAttribute.AssetId, _executionContext.GetIssuerBindingKeySource($"{_rootAttribute.Source}-{_rootAttribute.AssetId.ToHexString()}"), assetId).ConfigureAwait(false);

            universalProofs.IssuersAttributes.Add(new AttributesByIssuer
            {
                Issuer = _identityKeyProvider.GetKey(_associatedAttribute.Source.HexStringToByteArray()),
                RootAttribute = new AttributeProofs
                {
                    SchemeName = _associatedAttribute.AttributeSchemeName,
                    Commitment = _identityKeyProvider.GetKey(commitmentToAssociated),
                    BindingProof = bindingToRootProof,
                    CommitmentProof = new CommitmentProof
                    {
                        SurjectionProof = proofToRegistrationAssociated
                    }
                }
            });
            return (assetId, commitmentToAssociated);
        }

        private async Task<(byte[] bf, byte[] commitmentToRoot, SurjectionProof proofToRegistration)> GenerateRegistrationCommitmentAndProof(byte[] target, byte[] assetId, byte[] parentAssetId = null, byte[] parentBf = null)
        {
            byte[] commitmentNonblinded = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetId);
            byte[] bf = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] commitmentToRoot = ConfidentialAssetsHelper.BlindAssetCommitment(commitmentNonblinded, bf);
            byte[] bfSum;
            byte[] commitmentBlinded;
            if (parentAssetId != null)
            {
                byte[] commitmentToParent = ConfidentialAssetsHelper.GetAssetCommitment(parentBf, parentAssetId);
                commitmentBlinded = ConfidentialAssetsHelper.SumCommitments(commitmentToRoot, commitmentToParent);
                bfSum = ConfidentialAssetsHelper.SumScalars(bf, parentBf);
            }
            else
            {
                commitmentBlinded = commitmentToRoot;
                bfSum = bf;
            }

            SurjectionProof proofToRegistration = await _executionContext.RelationsBindingService.CreateProofToRegistration(target, bfSum, commitmentBlinded, assetId, parentAssetId).ConfigureAwait(false);

            return (bf, commitmentToRoot, proofToRegistration);
        }

        #endregion Commands

        #region Private Functions

        private async Task RequestO10InherenceServer(byte[] sessionKey, byte[] commitmentToRoot, byte[] associatedAssetId, byte[] commitmentToAssociated)
        {
            (byte[] commitment, byte[] image)[] commitmentImages = new (byte[] commitment, byte[] image)[(_photoBytes != null && _photoBytes2 != null) ? 2 : 1];
            int index = 0;
            if (_photoBytes != null)
            {
                commitmentImages[index].commitment = commitmentToRoot;
                commitmentImages[index].image = _photoBytes;
                index++;
            }

            if (_photoBytes2 != null)
            {
                commitmentImages[index].commitment = commitmentToAssociated;
                commitmentImages[index].image = _photoBytes2;
            }

            await (await _o10InherenceService.RequestO10InherenceServer(sessionKey, commitmentImages)
                .ContinueWith(async t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        bool res1 = await _communicationService.StoreRegistration(_target, "O10 Inherehnce", _rootAttribute.Source.HexStringToByteArray(), _rootAttribute.AssetId).ConfigureAwait(false);

                        bool res2 = true;
                        if (associatedAssetId != null)
                        {
                            res2 = await _communicationService.StoreRegistration(_target, "O10 Inherehnce", _associatedAttribute.Source.HexStringToByteArray(), associatedAssetId, _rootAttribute.AssetId).ConfigureAwait(false);
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (res1 && res2)
                            {
                                _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, AppResources.CAP_O10INHERENCE_ALERT_SUCCESS, AppResources.BTN_OK);
                            }
                            else
                            {
                                _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, AppResources.CAP_O10INHERENCE_ALERT_REGSTOREFAILED, AppResources.BTN_OK);
                            }
                            NavigationService.NavigateToRoot(_logger, $"expand={_rootAttribute.Source}");
                        });

                        IsLoading = false;
                    }
                    else
                    {
                        string response = await t.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        _logger.Error($"Failure while {nameof(RequestO10InherenceServer)}, response: {response}", t.Exception.InnerException);
                        IsLoading = false;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, string.Format(AppResources.CAP_O10INHERENCE_ALERT_GENERALFAILURE, response), AppResources.BTN_OK);
                            NavigationService.NavigateToRoot(_logger);
                        });
                    }
                }, TaskScheduler.Current)
                .ConfigureAwait(false)).ConfigureAwait(false);
        }

        #endregion Private Functions

        #region Overrides

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            long rootAttributeId = long.Parse(parameters["rootAttributeId"].ToString());
            _rootAttribute = _dataAccessService.GetUserRootAttribute(rootAttributeId);
            _target = _verifierInteractionService.ServiceInfo.Target.HexStringToByteArray();

            if (parameters.ContainsKey("associatedAttributeId"))
            {
                long associatedAttributeId = parameters.GetValue<long>("associatedAttributeId");
                _associatedAttribute = _dataAccessService.GetUserAssociatedAttributes(_executionContext.AccountId).FirstOrDefault(a => a.UserAssociatedAttributeId == associatedAttributeId);
            }

            string photo = _verifierInteractionService.Buffer;
            if (!string.IsNullOrEmpty(photo))
            {
                _verifierInteractionService.Buffer = null;

                //=============================================================================
                // If photo was provided it is needed to check whether it is photo of the Root 
                // Attribute or of the Associated one and if it is latter then it is needed to
                // check whether photo of Root Attribute already registered or it is needed to 
                // take one.
                //=============================================================================
                if (_associatedAttribute != null)
                {
                    var userRegistrations = _dataAccessService.GetUserRegistrations(_executionContext.AccountId);
                    _executionContext.RelationsBindingService.GetBoundedCommitment(_rootAttribute.AssetId, _verifierInteractionService.ServiceInfo.Target.HexStringToByteArray(), out byte[] blindingFactor, out byte[] commitment);
                    IsLocked = userRegistrations.Any(r => r.Commitment == commitment.ToHexString());
                    _photoBytes2 = Convert.FromBase64String(photo);
                    Photo = ImageSource.FromStream(() => new MemoryStream(_photoBytes2));
                }
                else
                {
                    IsLocked = true;
                    _photoBytes = Convert.FromBase64String(photo);
                    Photo = ImageSource.FromStream(() => new MemoryStream(_photoBytes));
                }

                if (IsLocked)
                {
                    IsRegisterEnabled = true;

                    RegisterCommand.Execute();
                }
            }
            else
            {
                TakePhotoCommand.Execute();
            }
        }

        #endregion Overrides
    }
}
