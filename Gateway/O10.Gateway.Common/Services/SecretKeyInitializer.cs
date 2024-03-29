﻿using System;
using System.Threading;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.ExtensionMethods;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Gateway.Common.Configuration;
using O10.Crypto.Services;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
	public class SecretKeyInitializer : InitializerBase
	{
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ISigningServicesRepository _signingServicesRepository;
        private readonly IAppConfig _appConfig;
        private readonly IGatewayContext _gatewayContext;
        private readonly ISecretConfiguration _secretConfiguration;
        private readonly ILogger _logger;

        public SecretKeyInitializer(IGatewayContext gatewayContext,
                              ISigningServicesRepository signingServicesRepository,
                              IConfigurationService configurationService,
                              IAppConfig appConfig,
                              IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                              ILoggerService loggerService)
		{
            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _signingServicesRepository = signingServicesRepository ?? throw new ArgumentNullException(nameof(signingServicesRepository));
            _appConfig = appConfig;
            _gatewayContext = gatewayContext;
			_secretConfiguration = configurationService.Get<ISecretConfiguration>();
            _logger = loggerService.GetLogger(nameof(SecretKeyInitializer));
		}

		public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest8;

		protected override async Task InitializeInner(CancellationToken cancellationToken)
		{
            if (string.IsNullOrEmpty(_secretConfiguration.SecretName))
            {
                throw new InvalidOperationException($"There was no value for {_secretConfiguration.SectionName}:{nameof(_secretConfiguration.SecretName)} was provided");
            }

            try
            {
                byte[] secretKey = GetSecret(_secretConfiguration.SecretName).HexStringToByteArray();
                var signingService = _signingServicesRepository.GetInstance("Ed25519SigningService");
                signingService.Initialize(secretKey);
                _gatewayContext.Initialize(signingService);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to set secret key", ex);
                throw;
            }
		}

        private string GetSecret(string key)
        {
            return _appConfig.GetString(key, true);
        }
    }
}
