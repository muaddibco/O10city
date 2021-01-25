using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using O10.Client.Web.Common.Exceptions;
using O10.Core.Logging;

namespace O10.Client.Web.Common
{
    public static class AzureHelper
    {
		public static X509Certificate2 GetCertificate(string thumbprint, ILogger logger)
		{
			using X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			certStore.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
			logger.Info($"Count of certificates: {certCollection.Count}");
			X509Certificate2 certificate = certCollection.OfType<X509Certificate2>().FirstOrDefault();

			return certificate;
		}

		public static string GetSecretValue(string secretName, string certThumbprint, string applicationId, string keyVaultName)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = null;

            if (!string.IsNullOrEmpty(certThumbprint))
            {
                using var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);

                certStore.Open(OpenFlags.ReadOnly);
                var certs = certStore.Certificates
                            .Find(X509FindType.FindByThumbprint,
                                certThumbprint, false);

                X509Certificate2 certificate = certs.OfType<X509Certificate2>().FirstOrDefault();

                if (certificate == null)
                {
                    throw new CertificateNotFoundException(certThumbprint, $"{certStore.Location}/{certStore.Name}");
                }

                ClientAssertionCertificate clientAssertionCertificate = new ClientAssertionCertificate(applicationId, certificate);
                keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback((a, r, s) => GetAccessToken(a, r, s, clientAssertionCertificate)));
            }
			else
            {
                keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            }

            var secret = keyVaultClient.GetSecretAsync($"https://{keyVaultName}.vault.azure.net/secrets/{secretName}").Result;

            return secret.Value;
        }

        private static async Task<string> GetAccessToken(string authority, string resource, string scope, ClientAssertionCertificate cert)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, cert).ConfigureAwait(false);
            return result.AccessToken;
        }
    }
}
