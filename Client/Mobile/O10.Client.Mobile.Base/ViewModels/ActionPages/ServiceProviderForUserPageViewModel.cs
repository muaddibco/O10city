using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Client.Common.Communication;
using O10.Client.Common.Configuration;
using O10.Client.Common.Entities;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Configuration;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.ViewModels.Inherence;
using Xamarin.Forms;
using O10.Core.Translators;
using O10.Client.Common.Dtos.UniversalProofs;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class ServiceProviderForUserPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IGatewayService _gatewayService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IDetailsService _detailsService;
        private readonly IAssetsService _assetsService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IBiometricService _biometricService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IO10LogicService _o10LogicService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ITranslator<UserRootAttribute, RootAttributeModel> _rootAttributeModelTranslator;
        private readonly ILogger _logger;
        private bool _isRegistered;
        private string _target;
        private string _sessionKey;
        private string _extraInfo;
        private string _actionInfo;
        //private List<Tuple<AttributeType, ValidationType>> _requiredValidationTuples;
        private List<string> _requiredValidations;
        private List<RootAttributeModel> _rootAttributes;
        private RootAttributeModel _selectedAttribute;
        private string _password;
        private bool _isBiometryRequired;
        private byte[] _inherencePublicKey;
        private byte[] _inherenceSignature;
        private List<InherenceVerifierViewModel> _inherenceVerifiers;
        private string _spInfo;
        private bool _authenticationRequired;

        private TaskCompletionSource<byte[]> _bindingKeySource;

        public ServiceProviderForUserPageViewModel(INavigationService navigationService,
                                                   IExecutionContext executionContext,
                                                   IDataAccessService dataAccessService,
                                                   IGatewayService gatewayService,
                                                   ISchemeResolverService schemeResolverService,
                                                   IConfigurationService configurationService,
                                                   IDetailsService detailsService,
                                                   IAssetsService assetsService,
                                                   IVerifierInteractionsManager verifierInteractionsManager,
                                                   IIdentityAttributesService identityAttributesService,
                                                   IBiometricService biometricService,
                                                   IIdentityKeyProvider identityKeyProvider,
                                                   IO10LogicService o10LogicService,
                                                   ITranslatorsRepository translatorsRepository,
                                                   ILoggerService loggerService,
                                                   IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _gatewayService = gatewayService;
            _schemeResolverService = schemeResolverService;
            _detailsService = detailsService;
            _assetsService = assetsService;
            _verifierInteractionsManager = verifierInteractionsManager;
            _identityAttributesService = identityAttributesService;
            _biometricService = biometricService;
            _identityKeyProvider = identityKeyProvider;
            _o10LogicService = o10LogicService;
            _rootAttributeModelTranslator = translatorsRepository.GetInstance<UserRootAttribute, RootAttributeModel>();
            _pageDialogService = pageDialogService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _logger = loggerService.GetLogger(nameof(ServiceProviderForUserPageViewModel));
        }

        #region Properties

        public bool IsRegistered
        {
            get => _isRegistered;
            set
            {
                SetProperty(ref _isRegistered, value);
            }
        }

        public string Target
        {
            get => _target;
            set
            {
                SetProperty(ref _target, value);
            }
        }

        public string SpInfo
        {
            get => _spInfo;
            set
            {
                SetProperty(ref _spInfo, value);
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

        public string ExtraInfo
        {
            get => _extraInfo;
            set
            {
                SetProperty(ref _extraInfo, value);
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

                VerifyBindingKeyValid()
                    .ContinueWith(t =>
                    {
                        if (t.Result)
                        {
                            ConfirmCommand.Execute();
                        }
                    }, TaskScheduler.Default);
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

        public bool AuthenticationRequired
        {
            get => _authenticationRequired;
            set
            {
                SetProperty(ref _authenticationRequired, value);
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

        #endregion Properties

        private async Task<bool> VerifyBindingKeyValid()
        {
            if (_selectedAttribute == null)
            {
                return false;
            }

            var bindingKeySource = await _executionContext.GetBindingKeySourceWithBio(_selectedAttribute.Key).ConfigureAwait(false);
            AuthenticationRequired = bindingKeySource == null;
            return bindingKeySource != null;
        }

        private void ExecuteCommand()
        {
            IsLoading = true;

            Task.Run(async () =>
                {
                    ActionDescription = AppResources.CAP_SP_ACTION_SENDING_REQUEST;

                    BiometricProof biometricProof = null;

                    if (_inherencePublicKey != null)
                    {
                        biometricProof = PrepareBiometricProof();
                    }

                    await SendProofs(biometricProof).ConfigureAwait(false);
                })
                .ContinueWith(t =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (t.IsFaulted)
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, t.Exception?.Message, AppResources.BTN_OK);
                        }

                        NavigationService.GoBackAsync();
                        ActionDescription = string.Empty;
                        IsLoading = false;
                    });
                }, TaskScheduler.Default);

        }

        private BiometricProof PrepareBiometricProof()
        {
            BiometricProof biometricProof;
            UserRootAttribute rootAttribute = _dataAccessService.GetUserRootAttribute(SelectedAttribute.AttributeId);
            _executionContext.RelationsBindingService.GetBoundedCommitment(rootAttribute.AssetId, _inherencePublicKey, out byte[] registrationBlindingFactor, out byte[] registrationCommitment);
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, rootAttribute.AssetId);
            byte[] blindingFactorToRegistration = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, registrationBlindingFactor);
            SurjectionProof authenticationProof = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { registrationCommitment }, 0, blindingFactorToRegistration);

            return new BiometricProof
            {
                BiometricCommitment = assetCommitment,
                BiometricSurjectionProof = authenticationProof,
                VerifierPublicKey = _inherencePublicKey,
                VerifierSignature = _inherenceSignature
            };
        }

        public DelegateCommand ConfirmCommand => new DelegateCommand(() =>
        {
            InitializeActionInfo()
                .ContinueWith(t =>
                {
                    if (!t.Result)
                    {
                        return;
                    }

                    if (IsBiometryRequired)
                    {
                        AdjustInherenceSection();
                    }
                    else
                    {
                        ExecuteCommand();
                    }
                }, TaskScheduler.Current);
        });

        public DelegateCommand GoToInherenceVerifiersCommand => new DelegateCommand(() =>
        {
            UserRootAttribute rootAttribute = _dataAccessService.GetUserRootAttribute(SelectedAttribute.AttributeId);
            NavigationService.NavigateAsync($"InherenceProtection?rootAttributeId={rootAttribute.UserAttributeId}");
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            IsLoading = true;

            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("actionInfo"))
            {
                _actionInfo = parameters["actionInfo"].ToString().DecodeUnescapedFromString64();

                RootAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId)
                    .Where(a => !a.IsOverriden && !a.LastCommitment.Equals32(new byte[Globals.DEFAULT_HASH_SIZE]))
                    .Select(a => _rootAttributeModelTranslator.Translate(a))
                    .ToList();

                if (RootAttributes.Count == 1)
                {
                    SelectedAttribute = RootAttributes[0];
                }
            }
            else if (parameters.ContainsKey("rootAttributeId"))
            {
                long rootAttributeId = long.Parse(parameters.GetValue<string>("rootAttributeId"));
                _selectedAttribute = RootAttributes.Find(r => r.AttributeId == rootAttributeId);
                _inherencePublicKey = parameters.GetValue<string>("publicKey").HexStringToByteArray();
                _inherenceSignature = parameters.GetValue<string>("signature").HexStringToByteArray();

                ExecuteCommand();
            }
            else
            {
                ConfirmCommand.Execute();
            }

            IsLoading = false;

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


        private async Task<bool> InitializeActionInfo()
        {
            if (_selectedAttribute == null)
            {
                return false;
            }

            _bindingKeySource = _executionContext.GetIssuerBindingKeySource(_selectedAttribute.Key) ?? _executionContext.GenerateBindingKey(_selectedAttribute.Key, Password);

            IsLoading = true;

            ActionDescription = AppResources.CAP_SP_CALCULATE_BINDING;

            _executionContext.RelationsBindingService.Initialize(_bindingKeySource);

            ActionDescription = AppResources.CAP_SP_ACTION_SYNC_WITH_SP;
            try
            {
                ServiceProviderActionAndValidations serviceProviderActionAndValidations = await GetServiceProviderActionAndValidations().ConfigureAwait(false);

                SpInfo = serviceProviderActionAndValidations.SpInfo;
                IsRegistered = serviceProviderActionAndValidations.IsRegistered;
                Target = serviceProviderActionAndValidations.PublicKey;
                SessionKey = serviceProviderActionAndValidations.SessionKey;
                IsBiometryRequired = serviceProviderActionAndValidations.IsBiometryRequired;
                ExtraInfo = serviceProviderActionAndValidations.ExtraInfo;
                if ((serviceProviderActionAndValidations.Validations?.Count ?? 0) > 0)
                {
                    //_requiredValidationTuples = serviceProviderActionAndValidations.Validations.Select(v =>
                    //    new Tuple<AttributeType, ValidationType>(
                    //        (AttributeType)Enum.Parse(typeof(AttributeType), v.Split(':')[0]),
                    //        (ValidationType)Enum.Parse(typeof(ValidationType), v.Split(':')[1])))
                    //    .ToList();
                    //RequiredValidations = GetRequiredValidations(_requiredValidationTuples);
                }

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

            return true;
        }

        private async Task<ServiceProviderActionAndValidations> GetServiceProviderActionAndValidations()
        {
            UriBuilder uriBuilder = new UriBuilder(_actionInfo);
            NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Uri.Query);
            byte[] targetBytes = query["pk"]?.HexStringToByteArray();

            query["rk"] = GetRegistrationKey(targetBytes);
            uriBuilder.Query = query.ToString();

            ServiceProviderActionAndValidations serviceProviderActionAndValidations = await _detailsService.GetServiceProviderActionAndValidations(uriBuilder.Uri.ToString()).ConfigureAwait(false);
            return serviceProviderActionAndValidations;
        }

        private string GetRegistrationKey(byte[] targetBytes)
        {
            var rootAttribute = _dataAccessService.GetUserRootAttribute(_selectedAttribute.AttributeId);
            _executionContext.RelationsBindingService.GetBoundedCommitment(rootAttribute.AssetId, targetBytes, out byte[] blindingFactor, out byte[] assetCommitment);
            string registrationKey = assetCommitment.ToHexString();
            return registrationKey;
        }

        private async Task SendProofs(BiometricProof biometricProof, AssociatedProofPreparation[] associatedProofPreparations = null)
        {
            var rootAttribute = _dataAccessService.GetUserRootAttribute(_selectedAttribute.AttributeId);
            byte[] commitment = CryptoHelper.GetNonblindedAssetCommitment(rootAttribute.AssetId);
            byte[] bf = CryptoHelper.GetRandomSeed();
            byte[] commitmentToRoot = CryptoHelper.BlindAssetCommitment(commitment, bf);
            byte[] issuer = rootAttribute.Source.HexStringToByteArray();
            Random random = new Random(BitConverter.ToInt32(bf, 0));
            byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            SurjectionProof eligibilityProof = StealthTransactionsService.CreateEligibilityProof(rootAttribute.IssuanceCommitment, rootAttribute.OriginalBlindingFactor, issuanceCommitments, bf, commitmentToRoot, random);

            RequestInput requestInput = new RequestInput
            {
                AssetId = rootAttribute.AssetId,
                Issuer = rootAttribute.Source.HexStringToByteArray(),
                PrevAssetCommitment = rootAttribute.LastCommitment,
                PrevBlindingFactor = rootAttribute.LastBlindingFactor,
                PrevDestinationKey = rootAttribute.LastDestinationKey,
                PrevTransactionKey = rootAttribute.LastTransactionKey,
                PublicSpendKey = Target.HexStringToByteArray(),
                AssetCommitment = commitmentToRoot,
                BlindingFactor = bf
            };

            IKey issuerKey = _identityKeyProvider.GetKey(rootAttribute.Source.HexStringToByteArray());
            SurjectionProof proofToRegistration = await _executionContext.RelationsBindingService.CreateProofToRegistration(requestInput.PublicSpendKey, bf, commitmentToRoot, requestInput.AssetId).ConfigureAwait(false);

            // ================================================================================
            // Prepare proof of Password
            // ================================================================================
            var associatedAttribute = await _assetsService.GetProtectionAttributeProofs(bf,
                                                                              rootAttribute.AssetId,
                                                                              _bindingKeySource,
                                                                              rootAttribute.Source).ConfigureAwait(false);
            // ================================================================================

            UniversalProofs universalProofs = new UniversalProofs
            {
                SessionKey = SessionKey,
                Mission = UniversalProofsMission.Authentication,
                Issuer = issuerKey,
                IssuersAttributes = new List<AttributesByIssuer>
                {
                    new AttributesByIssuer()
                    {
                        Issuer = issuerKey,
                        RootAttribute = new AttributeProofs
                        {
                            Commitment = _identityKeyProvider.GetKey(commitmentToRoot),
                            BindingProof = eligibilityProof,
                            CommitmentProof = new CommitmentProof
                            {
                                SurjectionProof = proofToRegistration
                            }
                        },
                        Attributes = new List<AttributeProofs> { associatedAttribute }
                    }
                }
            };

            try
            {
                await _o10LogicService.SendUniversalTransport(requestInput, universalProofs, SpInfo, true).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, ex.InnerException.Message, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, ex.Message, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
        }

        private void AdjustInherenceSection()
        {
            IsLoading = true;

            try
            {
                var inherenceServiceInfos = GetRegisteredInherenceServices();

                if (inherenceServiceInfos != null)
                {
                    InherenceVerifiers = inherenceServiceInfos
                        .Select(i => new InherenceVerifierViewModel(_verifierInteractionsManager, NavigationService)
                        {
                            Alias = i.Alias,
                            Description = i.Description,
                            IsRegistered = true,
                            Name = i.Name,
                            RootAttributeId = _selectedAttribute.AttributeId,
                        }).ToList();
                }
                else
                {
                    InherenceVerifiers = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(AdjustInherenceSection)}", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_SP_ALERT_TITLE, ex.Message, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private IEnumerable<InherenceServiceInfo> GetRegisteredInherenceServices()
        {
            var userRegistrations = _dataAccessService.GetUserRegistrations(_executionContext.AccountId);
            var rootAttribute = _dataAccessService.GetUserRootAttribute(_selectedAttribute.AttributeId);

            IEnumerable<InherenceServiceInfo> inherenceServiceInfos = _verifierInteractionsManager.GetInherenceServices();

            return inherenceServiceInfos.Where(i =>
            {
                _executionContext.RelationsBindingService.GetBoundedCommitment(rootAttribute.AssetId, i.Target.HexStringToByteArray(), out byte[] blindingFactor, out byte[] commitment);
                return userRegistrations.Any(r => r.Commitment == commitment.ToHexString());
            });
        }

        //private (byte[] issuer, T requestInput) GetRequestInput<T>(UserAttributeTransferDto userAttributeTransfer, long accountId, BiometricProof biometricProof) where T : RequestInput, new()
        //{
        //    UserRootAttribute userRootAttribute = _dataAccessService.GetRootAttributeByOriginalCommitment(accountId, userAttributeTransfer.OriginalCommitment.HexStringToByteArray());
        //    byte[] target = userAttributeTransfer.Target.HexStringToByteArray();
        //    byte[] target2 = userAttributeTransfer.Target2?.HexStringToByteArray();
        //    byte[] payload = userAttributeTransfer.Payload?.HexStringToByteArray();
        //    byte[] issuer = userRootAttribute.Source.HexStringToByteArray();
        //    byte[] assetId = userRootAttribute.AssetId;
        //    byte[] originalBlindingFactor = userRootAttribute.OriginalBlindingFactor;
        //    byte[] originalCommitment = userRootAttribute.OriginalCommitment;
        //    byte[] lastTransactionKey = userRootAttribute.LastTransactionKey;
        //    byte[] lastBlindingFactor = userRootAttribute.LastBlindingFactor;
        //    byte[] lastCommitment = userRootAttribute.LastCommitment;
        //    byte[] lastDestinationKey = userRootAttribute.LastDestinationKey;

        //    var rootAttribute = _dataAccessService.GetUserAttributes(_executionContext.AccountId).FirstOrDefault(u => u.UserAttributeId == _selectedAttribute.AttributeId);

        //    T requestInput = new T
        //    {
        //        AssetId = rootAttribute.AssetId,
        //        EligibilityBlindingFactor = rootAttribute.OriginalBlindingFactor,
        //        EligibilityCommitment = rootAttribute.OriginalCommitment,
        //        Issuer = rootAttribute.Source.HexStringToByteArray(),
        //        PrevAssetCommitment = rootAttribute.LastCommitment,
        //        PrevBlindingFactor = rootAttribute.LastBlindingFactor,
        //        PrevDestinationKey = rootAttribute.LastDestinationKey,
        //        PrevTransactionKey = rootAttribute.LastTransactionKey,
        //        PublicSpendKey = target,
        //        PublicViewKey = target2,
        //        Payload = payload,
        //        BiometricProof = biometricProof
        //    };

        //    return (issuer, requestInput);
        //}
    }
}
