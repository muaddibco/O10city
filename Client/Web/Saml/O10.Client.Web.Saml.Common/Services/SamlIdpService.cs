using dk.nita.saml20;
using dk.nita.saml20.Schema.Core;
using dk.nita.saml20.Schema.Protocol;
using dk.nita.saml20.Utils;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Client.Common.Communication;
using O10.Client.Web.Common.Dtos.SamlIdp;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Web.Saml.Common.Dtos;
using O10.Client.Web.Saml.Common.Hubs;
using Flurl.Http;
using System.Collections.Specialized;
using System.Net.Http;
using Saml2.Authentication.Core.Extensions;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Saml2.Authentication.Core.Configuration;
using CONST = dk.nita.saml20.Bindings.HttpRedirectBindingConstants;
using System.Security.Cryptography;
using System.Web;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers;

namespace O10.Client.Web.Saml.Common.Services
{
    public class SamlIdpService
	{
		/*private readonly ILogger _logger;
		private readonly StealthPacketsExtractor _utxoPacketsExtractor;
		private readonly IHubContext<SamlIdpHub> _samlIdpHubContext;
		private readonly ActionBlock<TaskCompletionWrapper<PacketBase>> _processPacketBlock;
		private byte[] _secretViewKey;
		private byte[] _publicViewKey;
		private byte[] _publicSpendKey;
		private string _publicViewKeyString;
		private string _publicSpendKeyString;
		private IDisposable _unsubscriber;
        private X509Certificate2 _certificate;

        // TODO: need to design lifetime of persistence record - when entry can be removed?
        private readonly ConcurrentDictionary<string, SamlIdpSessionPersistence> _persistences = new ConcurrentDictionary<string, SamlIdpSessionPersistence>();
		private readonly ConcurrentDictionary<string, SamlLoginPersistence> _logins = new ConcurrentDictionary<string, SamlLoginPersistence>();

		public SamlIdpService(StealthPacketsExtractor utxoPacketsExtractor, IHubContext<SamlIdpHub> samlIdpHubContext, ILoggerService loggerService)
		{
			_logger = loggerService.GetLogger(nameof(SamlIdpService));
			_utxoPacketsExtractor = utxoPacketsExtractor;
			_samlIdpHubContext = samlIdpHubContext;

			//TransformBlock<WitnessPackageWrapper, WitnessPackageWrapper> transformBlockIn = new TransformBlock<WitnessPackage, WitnessPackage>(p => p);
			PipeIn = _utxoPacketsExtractor.GetTargetPipe<WitnessPackageWrapper>(); // transformBlockIn;
			//transformBlockIn.LinkTo(_utxoPacketsExtractor.PipeIn);

			_processPacketBlock = new ActionBlock<TaskCompletionWrapper<PacketBase>>(p =>
            {
                try
                {
                    if (p.State is IdentityProofs identityProofs)
                    {
                        try
                        {
                            byte[] payload = identityProofs.EncodedPayload.Payload;
                            string registrationCommitment = identityProofs.AuthenticationProof.AssetCommitments[0].ToHexString();

                            string sessionKey = new Guid(new ReadOnlySpan<byte>(payload, 0, 16)).ToString();
                            string keyImage = identityProofs.KeyImage.ToString();
                            PropagateResponse(registrationCommitment, sessionKey, keyImage);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Failed to process IdentityProofs packet", ex);
                        }
                    }
                    else if (p.State is TransitionCompromisedProofs compromisedProofs)
                    {
                        string keyImage = compromisedProofs.CompromisedKeyImage.ToHexString();
                        SamlForceLogout(keyImage);
                    }

                    p.TaskCompletion.SetResult(new SucceededNotification());
                }
                catch (Exception ex)
                {
                    p.TaskCompletion.SetException(ex);
                }
			});

			_utxoPacketsExtractor.GetSourcePipe<TaskCompletionWrapper<PacketBase>>().LinkTo(_processPacketBlock);
		}

		public async void SamlForceLogout(string keyImage)
		{
			if (_logins.Remove(keyImage, out SamlLoginPersistence samlLoginPersistence))
			{
				try
				{
					NameID nameID = new NameID
					{
						Format = Saml2Constants.NameIdentifierFormats.Unspecified,
						Value = samlLoginPersistence.RegistrationCommitment
					};

					LogoutRequest logoutRequest = new LogoutRequest
					{
						ID = Guid.NewGuid().ToByteArray().ToHexString(),
						IssueInstant = DateTime.Now,
						Issuer = new NameID { Format = Saml2Constants.NameIdentifierFormats.Unspecified, Value = "o10idp" },
						Item = nameID
					};

					string logoutString = Serialization.SerializeToXmlString(logoutRequest);
					string logoutStringDeflated = Convert.ToBase64String(Encoding.UTF8.GetBytes(logoutString));

					UriBuilder uriBuilder = new UriBuilder(samlLoginPersistence.SingleLogoutUri);
					NameValueCollection queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
					queryParams[CONST.SamlRequest] = logoutString.DeflateEncode();
                    queryParams[CONST.SigAlg] = Saml2Constants.XmlDsigRSASHA256Url;

                    StringBuilder signedQuery = new StringBuilder();
                    signedQuery.AppendFormat("{0}=", CONST.SamlRequest);
                    signedQuery.Append(queryParams[CONST.SamlRequest].UrlEncode());
                    signedQuery.AppendFormat("&{0}=", CONST.SigAlg);
                    signedQuery.Append(queryParams[CONST.SigAlg].UrlEncode());

                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(signedQuery.ToString());
                        byte[] hash = sha256.ComputeHash(data);
                        byte[] signature = ((RSACng)_certificate.PrivateKey).SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        queryParams[CONST.Signature] = Convert.ToBase64String(signature);

                        uriBuilder.Query = queryParams.ToString();
                        string uri = uriBuilder.Uri.ToString();

                        HttpResponseMessage httpResponse = await uri.GetAsync().ConfigureAwait(false);
                    }
				}
				catch (Exception ex)
				{
					_logger.Error($"Failed to process SamlLogout for KeyImage {keyImage}", ex);
				}
			}
		}

		public void ProduceSignedResponse(string response, out string signatureString, out string signatureAlgorithm)
		{
			signatureAlgorithm = Saml2Constants.XmlDsigRSASHA256Url;
			StringBuilder signedQuery = new StringBuilder();
			signedQuery.AppendFormat("{0}=", CONST.SamlResponse);
			signedQuery.Append(response.UrlEncode());
			signedQuery.AppendFormat("&{0}=", CONST.SigAlg);
			signedQuery.Append(signatureAlgorithm.UrlEncode());

			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] data = Encoding.UTF8.GetBytes(signedQuery.ToString());
				byte[] hash = sha256.ComputeHash(data);
				byte[] signature = ((RSACng)_certificate.PrivateKey).SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				signatureString = Convert.ToBase64String(signature);
			}
		}

		internal void PropagateResponse(string registrationCommitment, string sessionKey, string keyImage)
        {
			if (!_persistences.Remove(sessionKey, out SamlIdpSessionPersistence persistence))
			{
				return;
			}

            SamlLoginPersistence samlLoginPersistence = new SamlLoginPersistence
            {
                CreationTime = DateTime.UtcNow,
                RegistrationCommitment = registrationCommitment,
                SingleLogoutUri = persistence.SingleLogoutUri
            };

            _logins.AddOrUpdate(keyImage, samlLoginPersistence, (k, v) => v);

			Assertion assertion = GetAssertion(registrationCommitment);

			Response response = new Response
			{
				InResponseTo = persistence.InResponseTo,
				Status = new Status
				{
					StatusCode = new StatusCode
					{
						Value = Saml2Constants.StatusCodes.Success
					}
				},
				Items = new object[] { assertion }
			};

            string assertionString = Serialization.SerializeToXmlString(response);
            string assertionStringDeflated = Convert.ToBase64String(Encoding.UTF8.GetBytes(assertionString));

            PropagateSamlIdpResponse(new SamlIdpSessionResponse
            {
                SessionId = sessionKey,
                RedirectUri = persistence.RedirectUri,
                Saml2Response = new Saml2.Authentication.Core.Bindings.Saml2Response
                {
                    Response = assertionStringDeflated,
                    RelayState = persistence.RelayState
                }
            });
        }

		private static Assertion GetAssertion(string registrationCommitment)
		{
			SamlAttribute samlAttribute = new SamlAttribute
			{
				NameFormat = SamlAttribute.NAMEFORMAT_BASIC,
				Name = "user_id",
				AttributeValue = new[] { registrationCommitment }
			};

			AttributeStatement attributeStatement = new AttributeStatement
			{
				Items = new[] { samlAttribute }
			};

			NameID nameID = new NameID
			{
				Format = Saml2Constants.NameIdentifierFormats.Unspecified,
				Value = registrationCommitment
			};

			List<AttributeStatement> attributeStatements = new List<AttributeStatement>
			{
				attributeStatement
			};

			Assertion assertion = new Assertion
			{
				ID = Guid.NewGuid().ToByteArray().ToHexString(),
				IssueInstant = DateTime.Now,
				Issuer = new NameID { Format = Saml2Constants.NameIdentifierFormats.Unspecified, Value = "o10idp" },
				Subject = new Subject
				{
					Items = new[] { nameID }
				},
				Items = attributeStatements.ToArray()
			};

			return assertion;
		}

		public ITargetBlock<WitnessPackageWrapper> PipeIn { get; }

		public string Name { get; private set; }

		public void Initialize(string name, byte[] secretViewKey, byte[] publicSpendKey)
		{
			Name = name;
			_secretViewKey = secretViewKey;
			_publicViewKey = CryptoHelper.GetPublicKey(_secretViewKey);
			_publicSpendKey = publicSpendKey;

			_publicSpendKeyString = _publicSpendKey.ToHexString();
			_publicViewKeyString = _publicViewKey.ToHexString();

            X509Certificate2 cert = LoadCertificate(new Certificate { FileName = @"Certificates\o10idp.pfx", Password = "o10idp" });
            CheckPrivateKey(cert);
            _certificate = cert;
        }

        public SamlIdpSession InstantiateSamlIdpSession(string id, string redirectUri, string relayState, string singleLogoutUri)
		{
			string sessionId = Guid.NewGuid().ToString();

			SamlIdpSessionPersistence persistence = new SamlIdpSessionPersistence
			{
                CreationTime = DateTime.UtcNow,
				SessionKey = sessionId,
				InResponseTo = id,
				RedirectUri = redirectUri,
                SingleLogoutUri = singleLogoutUri,
				RelayState = relayState,
				SessionInfo = new SamlIdpSessionInfo
				{
					SessionKey = sessionId,
					TargetPublicSpendKey = _publicSpendKeyString,
					TargetPublicViewKey = _publicViewKeyString
				}
			};

			_persistences.AddOrUpdate(sessionId, persistence, (k, v) => v);

			SamlIdpSession samlIdpSession = new SamlIdpSession
			{
				SessionKey = sessionId,
				ServiceName = Name
			};

			return samlIdpSession;
		}

        public SamlIdpSessionPersistence GetSamlIdpSessionPersistence(string sessionKey)
        {
            return _persistences.GetOrAdd(sessionKey, key => null);
        }

		private void PropagateSamlIdpResponse(SamlIdpSessionResponse samlIdpSessionResponse)
		{
			_samlIdpHubContext.Clients.Groups(samlIdpSessionResponse.SessionId).SendAsync("SamlIdpSessionResponse", samlIdpSessionResponse);
		}

		public void SetUnsubscriber(IDisposable unsubscriber)
		{
			_unsubscriber = unsubscriber;
		}

		public void Unsubscribe()
		{
			if(_unsubscriber != null)
			{
				_unsubscriber.Dispose();
				_unsubscriber = null;
			}
		}

        private static X509Certificate2 FindCertificate(string findValue, X509FindType findType, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine, bool validOnly = false)
        {
            var store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var found = store.Certificates.Find(findType, findValue, validOnly);
                if (found.Count == 0)
                {
                    var searchDescriptor = SearchDescriptor(findValue, findType, storeName, storeLocation, validOnly);
                    var msg =
                        $"A configured certificate could not be found in the certificate store. {searchDescriptor}";
                    throw new Exception(msg);
                }

                if (found.Count > 1)
                {
                    var searchDescriptor = SearchDescriptor(findValue, findType, storeName, storeLocation, validOnly);
                    var msg =
                        $"Found more than one certificate in the certificate store. Make sure you don't have duplicate certificates installed. {searchDescriptor}";
                    throw new Exception(msg);
                }

                return found[0];
            }
            finally
            {
                store.Close();
            }
        }

        private X509Certificate2 LoadCertificate(Certificate certificateDetails) =>
            certificateDetails.Thumbprint.IsNotNullOrEmpty()
                ? FindCertificate(
                    certificateDetails.Thumbprint,
                    X509FindType.FindByThumbprint,
                    certificateDetails.GetStoreName(),
                    certificateDetails.GetStoreLocation())
                : LoadCertificateFromFile(
                    certificateDetails.FileName,
                    certificateDetails.Password,
                    certificateDetails.GetKeyStorageFlags());

        private static X509Certificate2 LoadCertificateFromFile(string filename, string password, X509KeyStorageFlags flags = X509KeyStorageFlags.PersistKeySet)
        {
            var fullFileName = !Path.IsPathRooted(filename)
                ? Path.Combine(Directory.GetCurrentDirectory(), filename)
                : filename;

            return new X509Certificate2(
                fullFileName,
                password,
                flags);
        }

        private static string SearchDescriptor(string findValue, X509FindType findType, StoreName storeName, StoreLocation storeLocation, bool validOnly)
        {
            var message =
                $"The certificate was searched for in {storeLocation}/{storeName}, {findType}='{findValue}', validOnly={validOnly}.";
            if (findType == X509FindType.FindByThumbprint && findValue?.Length > 0 && findValue[0] == 0x200E)
            {
                message =
                    "\nThe configuration for the certificate searches by thumbprint but has an invalid character in the thumbprint string. Make sure you remove the first hidden character in the thumbprint value in the configuration. See https://support.microsoft.com/en-us/help/2023835/certificate-thumbprint-displayed-in-mmc-certificate-snap-in-has-extra-invisible-unicode-character. \n" +
                    message;
            }

            return message;
        }

        private static void CheckPrivateKey(X509Certificate2 x509Certificate)
        {
            if (!x509Certificate.HasPrivateKey)
            {
                throw new InvalidOperationException($"Certificate with thumbprint {x509Certificate.Thumbprint} does not have a private key.");
            }
        }*/
    }
}
