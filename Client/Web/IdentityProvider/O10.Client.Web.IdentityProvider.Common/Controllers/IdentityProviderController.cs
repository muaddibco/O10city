using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using O10.Client.Common.Interfaces;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.IdentityProvider.DataLayer.Services;
using O10.Core.ExtensionMethods;
using System.Threading.Tasks;
using Mailjet.Client;
using O10.Client.Web.Common;
using O10.Client.Web.Common.Configuration;
using O10.Core.Configuration;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using O10.Server.IdentityProvider.Common.Models;
using O10.IdentityProvider.DataLayer.Model;
using O10.Server.IdentityProvider.Common.Services;
using O10.Client.Common.Entities;
using O10.Core;
using Flurl.Http;
using O10.Server.IdentityProvider.Common.Configuration;
using O10.Core.Cryptography;
using Microsoft.AspNetCore.SignalR;
using O10.Server.IdentityProvider.Common.Hubs;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.Common.Configuration;
using ICoreDataAccessService = O10.Client.DataLayer.Services.IDataAccessService;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Server.IdentityProvider.Common.Controllers
{
    [ApiController]
	[Route("[controller]")]
	public class IdentityProviderController : ControllerBase
	{
		private const string APIPUBLICKEY = "mailjet-apipublickey";
		private const string APISECRETKEY = "mailjet-apisecretkey";
		private readonly IAzureConfiguration _azureConfiguration;
        private readonly IO10IdpConfiguration _o10IdpConfiguration;
        private readonly IRestApiConfiguration _restApiConfiguration;
		private readonly IExecutionContext _executionContext;
		private readonly IAssetsService _assetsService;
		private readonly IDataAccessService _dataAccessService;
		private readonly ICoreDataAccessService _coreDataAccessService;
		private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IAccountsService _accountsService;
		private readonly IHubContext<NotificationsHub> _hubContext;
		private readonly ILogger _logger;

		public IdentityProviderController(IExecutionContext executionContext, IAssetsService assetsService, 
            IDataAccessService dataAccessService, ICoreDataAccessService coreDataAccessService, IIdentityAttributesService identityAttributesService, 
            IAccountsService accountsService, IHubContext<NotificationsHub> hubContext,
			IConfigurationService configurationService, ILoggerService loggerService)
		{
			_executionContext = executionContext;
			_assetsService = assetsService;
			_dataAccessService = dataAccessService;
			_coreDataAccessService = coreDataAccessService;
            _identityAttributesService = identityAttributesService;
            _accountsService = accountsService;
			_hubContext = hubContext;
			_azureConfiguration = configurationService.Get<IAzureConfiguration>();
            _o10IdpConfiguration = configurationService.Get<IO10IdpConfiguration>();
			_restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
			_logger = loggerService.GetLogger(nameof(IdentityProviderController));
		}

		[HttpPost("RegisterWithEmail")]
		public async Task<IActionResult> RegisterWithEmail([FromBody] ActivationEmail activationEmail)
		{
			AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
			string email = Uri.UnescapeDataString(activationEmail.Email);
			byte[] assetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, email, account.PublicSpendKey.ToHexString()).ConfigureAwait(false);

			string assetIdExpr = assetId.ToHexString();

			if(!_dataAccessService.DoesAssetExist(assetIdExpr))
			{
				string sessionKey = Guid.NewGuid().ToString();
				byte[] sessionBlindingFactor = CryptoHelper.ReduceScalar32(CryptoHelper.FastHash256(Encoding.ASCII.GetBytes(activationEmail.Passphrase)));
				byte[] sessionCommitment = CryptoHelper.BlindAssetCommitment(CryptoHelper.GetNonblindedAssetCommitment(assetId), sessionBlindingFactor);
				long registrationSessionId = _dataAccessService.AddAssetRegistrationSession(sessionKey, sessionCommitment.ToHexString());

				if(await SendMail(email, sessionKey, Uri.UnescapeDataString(activationEmail.BaseUri)).ConfigureAwait(false))
				{
					return Ok(new { registrationSessionId });
				}
				else
				{
					throw new Exception("Failed to send email");
				}

			}

			return BadRequest("User with specified email already registered");
		}

		[HttpPost("ProceedWithRegistration/{sessionKey}")]
		public async Task<IActionResult> ProceedWithRegistration(string sessionKey, [FromBody] IssueAttributesRequestDTO requestBody)
		{
			RegistrationSession registrationSession = _dataAccessService.GetAssetRegistrationSession(sessionKey, requestBody.Protection.SessionCommitment);

            if (registrationSession != null && (DateTime.Now - registrationSession.CreationTime).TotalMinutes < _o10IdpConfiguration.SessionTimeout)
            {
                try
                {
					AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
                    bool proceed = true;

					byte[] assetIdEmail = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, requestBody.Content, account.PublicSpendKey.ToHexString()).ConfigureAwait(false);
                    byte[] biometricBlindingfactor = new byte[Globals.DEFAULT_HASH_SIZE];
                    byte[] originatingBiometricCommitment = new byte[Globals.DEFAULT_HASH_SIZE];

       //             if (!string.IsNullOrEmpty(requestBody.ImageContent))
       //             {
       //                 try
       //                 {
       //                     //HttpResponseMessage httpResponse = await _o10IdpConfiguration.BiometricUri.AppendPathSegment("RegisterPerson").PostJsonAsync(new BiometricPersonDataDto { Requester = account.PublicSpendKey.ToHexString(), PersonData = registrationSessionData.Content, ImageString = registrationSessionData.ImageContent }).ConfigureAwait(false);

       //                     //if (httpResponse.IsSuccessStatusCode)
       //                     //{
       //                     //    proceed = true;
       //                     //}

       //                     biometricBlindingfactor = ConfidentialAssetsHelper.GetRandomSeed();

							//originatingBiometricCommitment = await ProcessIssuingAssociatedAttribute(account.PublicSpendKey.ToHexString(), AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, requestBody.ImageContent, assetIdEmail, blindingPoint, blindingPoint, statePersistency.TransactionsService).ConfigureAwait(false);
       //                     proceed = true;
       //                 }
       //                 catch (Exception ex)
       //                 {
					  //      await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeRegistrationFailed", ex.Message).ConfigureAwait(false);

       //                     proceed = false;
       //                 }
       //             }
       //             else
       //             {
       //                 proceed = true;
       //             }

                    if (proceed)
                    {
						var persistency = _executionContext.GetContext();
						var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
						byte[] issuanceBlindingFactor = CryptoHelper.GetRandomSeed();
						var packet = await transactionsService.IssueBlindedAsset2(assetIdEmail, issuanceBlindingFactor).ConfigureAwait(false);

						byte[] protectionCommitment = null;
						if (requestBody.Attributes.ContainsKey(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD))
						{
							var attributeValueProtection = requestBody.Attributes[AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD];
							protectionCommitment = await ProcessIssuingAssociatedAttribute(
																				issuer: account.PublicSpendKey.ToHexString(),
                                                                                attributeSchemeName: AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                                                                                content: assetIdEmail.ToHexString(),
                                                                                rootAssetId: assetIdEmail,
                                                                                blindingPointValue: attributeValueProtection.BlindingPointValue,
                                                                                blindingPointRoot: attributeValueProtection.BlindingPointRoot,
                                                                                transactionsService: transactionsService).ConfigureAwait(false);
						}

                        long blindingRecordId = _dataAccessService.AddIssuanceBlindingFactors(issuanceBlindingFactor.ToHexString(), biometricBlindingfactor.ToHexString());

                        long userRecordId = _dataAccessService.AddAssetRegistration(assetIdEmail.ToHexString(), packet.AssetCommitment.ToHexString(), originatingBiometricCommitment.ToHexString(), protectionCommitment.ToHexString(), blindingRecordId);

                        //if (!string.IsNullOrEmpty(requestBody.ImageContent))
                        //{
                        //    byte[] imageContent = Convert.FromBase64String(requestBody.ImageContent);
                        //    _dataAccessService.AddBiometricRecord(userRecordId, BiometricRecordType.Photo, imageContent);
                        //}

                        await transactionsService.TransferAssetToStealth2(assetIdEmail, packet.AssetCommitment,
                            new ConfidentialAccount
                            {
                                PublicSpendKey = requestBody.PublicSpendKey.HexStringToByteArray(),
                                PublicViewKey = requestBody.PublicViewKey.HexStringToByteArray()
                            }).ConfigureAwait(false);

                        _dataAccessService.RemoveAssetRegistrationSession(registrationSession.RegistrationSessionId);

                        await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeRegistered").ConfigureAwait(false);
                    }

                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeRegistrationFailed", ex.Message).ConfigureAwait(false);
                    return BadRequest(ex.Message);
                }

                return Ok();
            }
            else
            {
                if (registrationSession == null)
                {
                    await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeRegistrationFailed", "Session of attribute registration not found").ConfigureAwait(false);
                }
                else
                {
                    await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeRegistrationFailed", "Session expired, need to request registration once again").ConfigureAwait(false);
                }
            }

            return BadRequest(registrationSession == null ? "Session not found" : "Session expired");
        }

		[HttpDelete("Attribute")]
		public async Task<IActionResult> DeleteAttribute(string mail)
		{
			AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
			byte[] assetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, mail, account.PublicSpendKey.ToHexString()).ConfigureAwait(false);
			UserRecord userRecord = _dataAccessService.RemoveAssetRegistration(assetId.ToHexString());

			return Ok(userRecord);
		}

		[HttpPost("IssueAttribute/{sessionKey}")]
        public async Task<IActionResult> IssueAttribute(string sessionKey, [FromBody] IdentityBaseData sessionData)
        {
			AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
			byte[] assetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, sessionData.Content, account.PublicSpendKey.ToHexString()).ConfigureAwait(false);
			UserRecord userRecord = _dataAccessService.GetAssetRegistration(assetId.ToHexString());
			BlindingFactorsRecord blindingFactors = _dataAccessService.GetBlindingFactors(userRecord.IssuanceBlindingRecordId);

			byte[] issuanceCommitment = userRecord.IssuanceCommitment.HexStringToByteArray();
			byte[] protectionCommitment = userRecord.ProtectionCommitment.HexStringToByteArray();

			SurjectionProof surjectionProof = new SurjectionProof
			{
				AssetCommitments = new byte[][] { protectionCommitment },
				Rs = new BorromeanRingSignature
				{
					E = sessionData.Protection.SignatureE.HexStringToByteArray(),
					S = new byte[][] { sessionData.Protection.SignatureS.HexStringToByteArray() }
				}
			};

			if(CryptoHelper.VerifySurjectionProof(surjectionProof, sessionData.Protection.SessionCommitment.HexStringToByteArray()))
			{
				bool faceComparisonSucceeded = true; // false;

				//BiometricRecord biometricRecord = _dataAccessService.GetBiometricRecord(userRecord.UserRecordId, BiometricRecordType.Photo);
				//await _restApiConfiguration.BiometricUri.AppendPathSegment("CompareFaces")
				//	.PostJsonAsync(new CompareFacesDto { Face1 = Convert.ToBase64String(biometricRecord.Content), Face2 = sessionData.ImageContent })
				//	.ContinueWith(t =>
				//	{
				//		if(t.IsCompleted && !t.IsFaulted && t.Result.IsSuccessStatusCode)
				//		{
				//			string responseContent = t.Result.Content.ReadAsStringAsync().Result;

				//			bool.TryParse(responseContent, out faceComparisonSucceeded);
				//		}
				//	}, TaskScheduler.Default).ConfigureAwait(false);

				if (faceComparisonSucceeded)
				{
					var persistency = _executionContext.GetContext();
					var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
					transactionsService.TransferAssetToStealth2(assetId, issuanceCommitment,
						new ConfidentialAccount
                        {
							PublicSpendKey = sessionData.PublicSpendKey.HexStringToByteArray(),
							PublicViewKey = sessionData.PublicViewKey.HexStringToByteArray()
						});

					await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeIssued").ConfigureAwait(false);
                    IEnumerable<AttributeDefinition> attributeDefinitions = _coreDataAccessService
                        .GetAttributesSchemeByIssuer(account.PublicSpendKey.ToHexString(), true)
						.Select(a => new AttributeDefinition
                        {
								SchemeId = a.IdentitiesSchemeId,
								AttributeName = a.AttributeName,
								SchemeName = a.AttributeSchemeName,
								Alias = a.Alias,
								Description = a.Description,
								IsActive = a.IsActive,
								IsRoot = a.CanBeRoot
							});
                    AttributeValue attributeValue = new AttributeValue
                    {
						Value = sessionData.Content,
						Definition = attributeDefinitions.FirstOrDefault(d => d.AttributeName == AttributesSchemes.ATTR_SCHEME_NAME_EMAIL)
					};

					return base.Ok(new List<AttributeValue> { attributeValue });
				}
                else
                {
                    await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeIssueFailed", "Faces comparison failed").ConfigureAwait(false);
					return base.BadRequest("Faces comparison failed");
				}
			}
            else
            {
                await _hubContext.Clients.Group(sessionKey).SendAsync("AttributeIssueFailed", "Password is incorrect").ConfigureAwait(false);
				return base.BadRequest("Password is incorrect");
			}
		}

		[HttpGet("IssueSessionData")]
		public IActionResult GetIssueSessionData()
		{
			string guid = Guid.NewGuid().ToString();
			
			return Ok(new { SessionKey = guid, Uri = $"iss://{Request.Scheme}://{Request.Host.ToUriComponent()}/IdentityProvider/ReissuanceDetails/{guid}".EncodeToString64() });
		}

		[HttpGet("IsAccountExist")]
		public async Task<IActionResult> GetIsAccountExist([FromQuery] string email)
		{
			AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
			byte[] assetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, email, account.PublicSpendKey.ToHexString()).ConfigureAwait(false);
			UserRecord userRecord = _dataAccessService.GetAssetRegistration(assetId.ToHexString());

			return Ok(new { Exist = userRecord != null });
		}

		[HttpGet("RegistrationDetails/{sessionKey}")]
		public ActionResult<IssuerActionDetails> GetRegistrationDetails(string sessionKey)
		{
			AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
			IssuerActionDetails registrationDetails = new IssuerActionDetails
			{
				Issuer = account.PublicSpendKey.ToHexString(),
				IssuerAlias = account.AccountInfo,
				ActionUri = $"{Request.Scheme}://{Request.Host.ToUriComponent()}/IdentityProvider/ProceedWithRegistration/{sessionKey}".EncodeToString64()
			};

			return registrationDetails;
		}

		[HttpGet("ReissuanceDetails/{sessionKey}")]
		public ActionResult<IssuerActionDetails> GetReissuanceDetails(string sessionKey)
		{
			AccountDescriptor account = _accountsService.GetById(_executionContext.GetContext().AccountId);
			IssuerActionDetails registrationDetails = new IssuerActionDetails
			{
				Issuer = account.PublicSpendKey.ToHexString(),
				IssuerAlias = account.AccountInfo,
				ActionUri = $"{Request.Scheme}://{Request.Host.ToUriComponent()}/IdentityProvider/IssueAttribute/{sessionKey}".EncodeToString64()
			};

			return registrationDetails;
		}

		/// <summary>
		/// commitment = blindingPointValue + GenerateAssetId(attributeSchemeName, content, issuer) * G
		/// </summary>
		/// <param name="issuer"></param>
		/// <param name="attributeSchemeName"></param>
		/// <param name="content"></param>
		/// <param name="rootAssetId"></param>
		/// <param name="blindingPointValue"></param>
		/// <param name="blindingPointRoot"></param>
		/// <param name="transactionsService"></param>
		/// <returns></returns>
		private async Task<byte[]> ProcessIssuingAssociatedAttribute(string issuer,
															   string attributeSchemeName,
															   string content,
															   byte[] rootAssetId,
															   byte[] blindingPointValue,
															   byte[] blindingPointRoot,
															   IStateTransactionsService transactionsService)
        {
            byte[] assetId = await _assetsService.GenerateAssetId(attributeSchemeName, content, issuer).ConfigureAwait(false);
			byte[] rootCommitment = _assetsService.GetCommitmentBlindedByPoint(rootAssetId, blindingPointRoot);
            var packet = await transactionsService.IssueAssociatedAsset(assetId, blindingPointValue, rootCommitment).ConfigureAwait(false);

			return packet.AssetCommitment;
        }

        private async Task<bool> SendMail(string email, string sessionKey, string baseUri)
		{
			MailjetClient mailjetClient = new MailjetClient(
					AzureHelper.GetSecretValue(APIPUBLICKEY, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName),
					AzureHelper.GetSecretValue(APISECRETKEY, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName))
			{
				Version = ApiVersion.V3_1
			};

			MailjetRequest request = new MailjetRequest
			{
				Resource = Send.Resource
			}
			.Property(Send.Messages, new JArray {
				new JObject {
				 {"From", new JObject {
				  {"Email", "info@o10.city"},
				  {"Name", "O10 Network"}
				  }},
				 {"To", new JArray {
				  new JObject {
				   {"Email", email}
				   }
				  }},
				 {"TemplateID", 1003985},
				 {"TemplateLanguage", true},
				 {"Subject", "O10 Network Identity Provider Registration"},
				 {"Variables", new JObject {
				  {"registrationuri", $"{baseUri}/idpregconfirm?sk={sessionKey}"}
				  }}
				 }
				});

			MailjetResponse response = await mailjetClient.PostAsync(request).ConfigureAwait(false);

			if (response.IsSuccessStatusCode)
			{
				_logger.Debug(string.Format("Total: {0}, Count: {1}	", response.GetTotal(), response.GetCount()));

				_logger.Debug(response.GetData().ToString());

				return true;
			}
			else
			{
				_logger.Error(string.Format("StatusCode: {0}	", response.StatusCode));

				_logger.Error(string.Format("ErrorInfo: {0}	", response.GetErrorInfo()));

				_logger.Error(response.GetData().ToString());
				_logger.Error(string.Format("ErrorMessage: {0}	", response.GetErrorMessage()));
				return false;
			}
		}
	}
}
