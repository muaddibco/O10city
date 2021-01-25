using Flurl;
using Flurl.Http;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Dtos;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    [RegisterExtension(typeof(IVerifierInteractionService), Lifetime = LifetimeManagement.Singleton)]
    public class O10InherenceInteractionService : IVerifierInteractionService
    {
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IO10InherenceConfiguration _o10InherenceConfiguration;
        private readonly ILogger _logger;

        public O10InherenceInteractionService(IExecutionContext executionContext,
                                               IDataAccessService dataAccessService,
                                               IConfigurationService configurationService,
                                               ILoggerService loggerService)
        {
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _o10InherenceConfiguration = configurationService.Get<IO10InherenceConfiguration>();
            _logger = loggerService.GetLogger(nameof(O10InherenceInteractionService));
        }

        public string Name => "O10Inherence";
        public string Buffer { get; set; }
        public InherenceServiceInfo ServiceInfo { get; set; }

        public async Task InvokeRegistration(INavigationService navigationService, string args = null)
        {
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("O10InherenceRegistration" + (!string.IsNullOrEmpty(args) ? $"?{args}" : "")));
        }

        public async Task InvokeUnregistration(INavigationService navigationService, string args = null) =>
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("O10InherenceRemoval" + (!string.IsNullOrEmpty(args) ? $"?{args}" : "")));

        public async Task InvokeVerification(INavigationService navigationService, string args = null) =>
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("O10InherenceVerification" + (!string.IsNullOrEmpty(args) ? $"?{args}" : "")));

        public async Task<VerificationResult> Verify(long rootAttributeId, byte[] photoBytes)
        {
            VerificationResult verificationResult = new VerificationResult();
            var rootAttribute = _dataAccessService.GetUserRootAttribute(rootAttributeId);
            _executionContext.RelationsBindingService.GetBoundedCommitment(rootAttribute.AssetId, ServiceInfo.Target.HexStringToByteArray(), out byte[] blindingFactor, out byte[] registrationCommitment);
            byte[] keyImage = _executionContext.ClientCryptoService.GetKeyImage(rootAttribute.LastTransactionKey);
            BiometricVerificationDataDto verificationDataDto = new BiometricVerificationDataDto
            {
                Issuer = rootAttribute.Source,
                KeyImage = keyImage.ToHexString(),
                RegistrationKey = registrationCommitment.ToHexString(),
                ImageString = Convert.ToBase64String(photoBytes)
            };

            await _o10InherenceConfiguration.Uri.AppendPathSegment("VerifyPersonFace")
                .PostJsonAsync(verificationDataDto)
                .ReceiveJson<BiometricSignedVerificationDto>()
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        verificationResult.SignedVerification = t.Result;
                        _logger.Info("Biometric verification succeeded");
                    }
                    else
                    {
                        _logger.Error("Biometric verification failed", t.Exception.InnerException);
                        verificationResult.ErrorMessage = t.Exception.InnerException.Message;
                    }
                }, TaskScheduler.Current).ConfigureAwait(false);

            return verificationResult;
        }
    }
}
