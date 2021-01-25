using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Flurl;
using Flurl.Http;
using Plugin.Media;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Transactions.Core.DataModel;
using O10.Transactions.Core.DataModel.Stealth.Internal;
using O10.Client.Common.Configuration;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Translators;
using O10.Client.Mobile.Base.Dtos;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class O10SamlIdpPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IGatewayService _gatewayService;
        private readonly IRestClientService _restClientService;
        private readonly IAssetsService _assetsService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IBiometricService _biometricService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IRestApiConfiguration _walletSettings;
        private readonly ITranslator<UserRootAttribute, RootAttributeModel> _rootAttributeModelTranslator;
#pragma warning disable CS0169 // The field 'O10SamlIdpPageViewModel._isRegistered' is never used
        private readonly bool _isRegistered;
#pragma warning restore CS0169 // The field 'O10SamlIdpPageViewModel._isRegistered' is never used
        private string _targetPublicSpendKey;
        private string _targetPublicViewKey;
        private string _sessionKey;
#pragma warning disable CS0169 // The field 'O10SamlIdpPageViewModel._extraInfo' is never used
        private readonly string _extraInfo;
#pragma warning restore CS0169 // The field 'O10SamlIdpPageViewModel._extraInfo' is never used
        //private List<Tuple<AttributeType, ValidationType>> _requiredValidationTuples;
        private List<string> _requiredValidations;
        private List<RootAttributeModel> _rootAttributes;
        private RootAttributeModel _selectedAttribute;
        private string _password;
        private byte[] _photoBytes;
        private bool _isLoading;
        private bool _isBiometryRequired;
        private string _actionDescription;

        public O10SamlIdpPageViewModel(INavigationService navigationService,
                                        IExecutionContext executionContext,
                                        IDataAccessService dataAccessService,
                                        IGatewayService gatewayService,
                                        IRestClientService restClientService,
                                        IConfigurationService configurationService,
                                        IAssetsService assetsService,
                                        IIdentityAttributesService identityAttributesService,
                                        IBiometricService biometricService,
                                        ITranslatorsRepository translatorsRepository,
                                        IPageDialogService pageDialogService) : base(navigationService)
        {
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _gatewayService = gatewayService;
            _restClientService = restClientService;
            _assetsService = assetsService;
            _identityAttributesService = identityAttributesService;
            _biometricService = biometricService;
            _pageDialogService = pageDialogService;
            _rootAttributeModelTranslator = translatorsRepository.GetInstance<UserRootAttribute, RootAttributeModel>();
            _walletSettings = configurationService.Get<IRestApiConfiguration>();
        }

#pragma warning disable CS0108 // 'O10SamlIdpPageViewModel.IsLoading' hides inherited member 'ViewModelBase.IsLoading'. Use the new keyword if hiding was intended.
        public bool IsLoading
#pragma warning restore CS0108 // 'O10SamlIdpPageViewModel.IsLoading' hides inherited member 'ViewModelBase.IsLoading'. Use the new keyword if hiding was intended.
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        public string ActionDescription
        {
            get => _actionDescription;
            set
            {
                SetProperty(ref _actionDescription, value);
            }
        }

        public string SessionKey
        {
            get => _sessionKey;
            set
            {
                SetProperty(ref _sessionKey, value);
            }
        }

        public bool IsBiometryRequired
        {
            get => _isBiometryRequired;
            set
            {
                SetProperty(ref _isBiometryRequired, value);
            }
        }

        public List<string> RequiredValidations
        {
            get => _requiredValidations;
            set
            {
                SetProperty(ref _requiredValidations, value);
            }
        }

        public List<RootAttributeModel> RootAttributes
        {
            get => _rootAttributes;
            set
            {
                SetProperty(ref _rootAttributes, value);
            }
        }

        public RootAttributeModel SelectedAttribute
        {
            get => _selectedAttribute;
            set
            {
                SetProperty(ref _selectedAttribute, value);
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

        public DelegateCommand TakePhotoCommand => new DelegateCommand(async () =>
        {
            var photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front });

            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                photo.GetStream().CopyTo(ms);
                _photoBytes = ms.ToArray();
                //Photo = ImageSource.FromStream(() => { return photo.GetStream(); });

                ExecuteCommand();
            }
        });

        private void ExecuteCommand()
        {
            Device.BeginInvokeOnMainThread(() => IsLoading = true);

            Task.Run(async () =>
                {
                    TaskCompletionSource<byte[]> bindingKeySource = _executionContext.GetBindingKeySource(Password);

                    ActionDescription = AppResources.CAP_SP_ACTION_FACES_COMPARISON;
                    BiometricProof biometricProof = await _biometricService.CheckBiometrics(_photoBytes != null ? Convert.ToBase64String(_photoBytes) : null, SelectedAttribute, await bindingKeySource.Task.ConfigureAwait(false)).ConfigureAwait(false);
                    if (biometricProof != null)
                    {
                        ActionDescription = AppResources.CAP_SP_ACTION_SENDING_REQUEST;
                        //SendIdentityProofsRequest(biometricProof);
                    }
                    else
                    {
                        await _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, AppResources.CAP_SP_ALERT_FACES_COMPARING_FAILURE, AppResources.BTN_OK).ConfigureAwait(false);
                    }
                })
                .ContinueWith(t =>
                {
                    ActionDescription = string.Empty;
                    IsLoading = false;

                    if (t.IsFaulted)
                    {
                        _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, t.Exception?.Message, AppResources.BTN_OK);
                    }

                    Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());
                }, TaskScheduler.Default);

        }

        public DelegateCommand ConfirmCommand => new DelegateCommand(() => ExecuteCommand());

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            IsLoading = true;

            base.OnNavigatedTo(parameters);
            RootAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId)
                .Where(a => !a.IsOverriden && !a.LastCommitment.Equals32(new byte[Globals.DEFAULT_HASH_SIZE]))
                .Select(a => _rootAttributeModelTranslator.Translate(a)).ToList();

            if (RootAttributes.Count == 1)
            {
                SelectedAttribute = RootAttributes[0];
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            InitializeActionInfo(parameters["actionInfo"].ToString().DecodeUnescapedFromString64());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        //private List<string> GetRequiredValidations(List<Tuple<AttributeType, ValidationType>> validations)
        //{
        //    List<string> requiredValidations = new List<string>();
        //    IEnumerable<Tuple<AttributeType, string>> attributeDescriptions = _identityAttributesService.GetAssociatedAttributeTypes();
        //    IEnumerable<Tuple<ValidationType, string>> validationDescriptions = _identityAttributesService.GetAssociatedValidationTypes();

        //    foreach (var validation in validations)
        //    {
        //        if (validation.Item1 != AttributeType.DateOfBirth)
        //        {
        //            requiredValidations.Add(attributeDescriptions.FirstOrDefault(d => d.Item1 == validation.Item1)?.Item2 ?? validation.Item1.ToString());
        //        }
        //        else
        //        {
        //            requiredValidations.Add(validationDescriptions.FirstOrDefault(d => d.Item1 == validation.Item2)?.Item2 ?? validation.Item2.ToString());
        //        }
        //    }

        //    return requiredValidations;
        //}

        //private void SendIdentityProofsRequest(BiometricProof biometricProof)
        //{
        //    var rootAttribute = _dataAccessService.GetUserAttributes(_executionContext.AccountId).FirstOrDefault(u => u.UserAttributeId == _selectedAttribute.AttributeId);

        //    _assetsService.GetBlindingPoint(rootAttribute.Content, Password, out byte[] blindingPoint, out byte[] blindingFactor);

        //    byte[] rootOriginatingCommitment = _assetsService.GetCommitmentBlindedByPoint(rootAttribute.AssetId, blindingPoint);

        //    AssociatedProofPreparation[] associatedProofPreparations = null;

        //    if (_requiredValidationTuples != null && _requiredValidationTuples.Count > 0)
        //    {
        //        associatedProofPreparations = new AssociatedProofPreparation[_requiredValidationTuples.Count];

        //        var associatedAttributes = _dataAccessService.GetUserAssociatedAttributes(_executionContext.AccountId);

        //        int index = 0;
        //        foreach (var validation in _requiredValidationTuples)
        //        {
        //            string attrContent = associatedAttributes.FirstOrDefault(a => a.Item1 == validation.Item1)?.Item2 ?? string.Empty;
        //            byte[] groupId = _identityAttributesService.GetGroupId(validation.Item1);
        //            byte[] assetId = validation.Item1 != AttributeType.DateOfBirth ? _assetsService.GenerateAssetId(validation.Item1, attrContent) : rootAttribute.AssetId;
        //            byte[] associatedBlindingFactor = validation.Item1 != AttributeType.DateOfBirth ? ConfidentialAssetsHelper.GetRandomSeed() : null;
        //            byte[] associatedCommitment = validation.Item1 != AttributeType.DateOfBirth ? ConfidentialAssetsHelper.GetAssetCommitment(assetId, associatedBlindingFactor) : null;
        //            byte[] associatedOriginatingCommitment = _assetsService.GetCommitmentBlindedByPoint(assetId, blindingPoint);

        //            AssociatedProofPreparation associatedProofPreparation = new AssociatedProofPreparation { GroupId = groupId, Commitment = associatedCommitment, CommitmentBlindingFactor = associatedBlindingFactor, OriginatingAssociatedCommitment = associatedOriginatingCommitment, OriginatingBlindingFactor = blindingFactor, OriginatingRootCommitment = rootOriginatingCommitment };

        //            associatedProofPreparations[index++] = associatedProofPreparation;
        //        }
        //    }

        //    SendIdentityProofs(biometricProof, associatedProofPreparations);
        //}

        private async Task SendIdentityProofs(BiometricProof biometricProof, AssociatedProofPreparation[] associatedProofPreparations = null)
        {
            var rootAttribute = _dataAccessService.GetUserAttributes(_executionContext.AccountId).FirstOrDefault(u => u.UserAttributeId == _selectedAttribute.AttributeId);

            RequestInput requestInput = new RequestInput
            {
                AssetId = rootAttribute.AssetId,
                EligibilityBlindingFactor = rootAttribute.OriginalBlindingFactor,
                EligibilityCommitment = rootAttribute.OriginalCommitment,
                Issuer = rootAttribute.Source.HexStringToByteArray(),
                PrevAssetCommitment = rootAttribute.LastCommitment,
                PrevBlindingFactor = rootAttribute.LastBlindingFactor,
                PrevDestinationKey = rootAttribute.LastDestinationKey,
                PrevTransactionKey = rootAttribute.LastTransactionKey,
                PublicSpendKey = _targetPublicSpendKey.HexStringToByteArray(),
                PublicViewKey = _targetPublicViewKey?.HexStringToByteArray(),
                Payload = _sessionKey.HexStringToByteArray(),
                BiometricProof = biometricProof
            };

            OutputModel[] outputModels = await _gatewayService.GetOutputs(_walletSettings.RingSize + 1).ConfigureAwait(false);
            RequestResult requestResult = await _executionContext.TransactionsService.SendIdentityProofs(requestInput, associatedProofPreparations, outputModels, rootAttribute.Source.HexStringToByteArray()).ConfigureAwait(false);
        }

        private async Task InitializeActionInfo(string action)
        {
            ActionDescription = AppResources.CAP_SP_ACTION_SYNC_WITH_SP;
            try
            {
                UriBuilder uriBuilder = new UriBuilder(_walletSettings.SamlIdpUri);
                NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Uri.Query);
                query["sessionInfo"] = action;
                uriBuilder.Query = query.ToString();

                Url url = uriBuilder.Uri.ToString().AppendPathSegments("SamlIdp", "GetSessionInfo");
                SamlIdpSessionInfo samlIdpSessionInfo = await _restClientService.Request(url).GetJsonAsync<SamlIdpSessionInfo>().ConfigureAwait(false);
                byte[] sessionKeyBytes = new Guid(samlIdpSessionInfo.SessionKey).ToByteArray();
                byte[] sessionKeyComplemented = sessionKeyBytes.ComplementTo32();

                _targetPublicSpendKey = samlIdpSessionInfo.TargetPublicSpendKey;
                _targetPublicViewKey = samlIdpSessionInfo.TargetPublicViewKey;
                SessionKey = sessionKeyComplemented.ToHexString();
                IsBiometryRequired = false;
                //if ((serviceProviderActionAndValidations.Validations?.Count ?? 0) > 0)
                //{
                //    _requiredValidationTuples = serviceProviderActionAndValidations.Validations.Select(v =>
                //        new Tuple<AttributeType, ValidationType>(
                //            (AttributeType)Enum.Parse(typeof(AttributeType), v.Split(':')[0]),
                //            (ValidationType)Enum.Parse(typeof(ValidationType), v.Split(':')[1])))
                //        .ToList();
                //    RequiredValidations = GetRequiredValidations(_requiredValidationTuples);
                //}

            }
            catch (Exception ex)
            {
                await _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, ex.Message, AppResources.BTN_OK).ConfigureAwait(false);
                Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());
            }
            finally
            {
                ActionDescription = string.Empty;
                IsLoading = false;
            }
        }
    }
}
