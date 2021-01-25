using dk.nita.saml20.Schema.Protocol;
using dk.nita.saml20.Utils;
using Microsoft.AspNetCore.Mvc;
using Saml2.Authentication.Core.Extensions;
using O10.Crypto.ConfidentialAssets;
using O10.Core.ExtensionMethods;
using O10.Client.Web.Common.Exceptions;
using O10.Client.Web.Saml.Common.Services;
using O10.Client.Web.Saml.Common.Dtos;
using O10.Client.Web.Common.Dtos.SamlIdp;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Client.Web.Saml.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using dk.nita.saml20;
using System.Text;
using System;

namespace O10.Client.Web.Saml.Common.Controllers
{
    [ApiController]
	[AllowAnonymous]
    [Route("[controller]")]
    public class SamlIdpController : ControllerBase
    {
		private readonly ISamlIdentityProvidersManager _samlIdentityProvidersManager;
        private readonly IDataAccessService _dataAccessService;

        public SamlIdpController(ISamlIdentityProvidersManager samlIdentityProvidersManager, IDataAccessService dataAccessService)
        {
			_samlIdentityProvidersManager = samlIdentityProvidersManager;
            _dataAccessService = dataAccessService;
        }

        [HttpPut("DefaultIdpServiceProvider")]
        public ActionResult CreateDefaultIdpServiceProvider()
        {
            _samlIdentityProvidersManager.CreateNewDefaultSamlIdentityProvider();
            return Ok();
        }

		[HttpPut("SamlServiceProvider")]
		public IActionResult StoreSamlServiceProvider([FromQuery] string entityId, [FromQuery] string singleLogoutUri)
		{
			_dataAccessService.StoreSamlServiceProvider(entityId, singleLogoutUri);
			return Ok();
		}

        [HttpGet("SecretKey")]
        public ActionResult<string> GenerateSecretKey()
        {
            return ConfidentialAssetsHelper.GetRandomSeed().ToHexString();
        }

        [HttpGet("InitiateSamlSession")]
        public ActionResult<SamlIdpSessionDescriptor> InitiateSamlSession([FromQuery] string samlRequest, [FromQuery] string relayState)
        {
            //byte[] samlCompressed = Convert.FromBase64String(samlRequest);
            //byte[] samlDecompressed = Decompress(samlCompressed);
			string saml2RequestDecoded = samlRequest.DeflateDecompress();

            AuthnRequest request = Serialization.DeserializeFromXmlString<AuthnRequest>(saml2RequestDecoded);

            SamlServiceProvider samlServiceProvider = _dataAccessService.GetSamlServiceProvider(request.Issuer.Value);

            if(samlServiceProvider == null)
            {
                throw new SamlSpIsNotRegisteredException(request.Issuer.Value);
            }

			SamlIdpService samlIdpService = _samlIdentityProvidersManager.GetSamlIdpService(request.Issuer.Value);

			if(samlIdpService == null)
			{
				throw new SamlIdpServiceNotFoundException(request.Issuer.Value);
			}

			SamlIdpSession samlIdpSession = samlIdpService.InstantiateSamlIdpSession(request.ID, request.AssertionConsumerServiceURL, relayState, samlServiceProvider.SingleLogoutUrl);

            string samlIdpSessionExpr = Serialization.SerializeToXmlString(samlIdpSession);
			
			return Ok(new SamlIdpSessionDescriptor { SessionInfo = samlIdpSessionExpr.DeflateEncode(), SessionKey = samlIdpSession.SessionKey });
        }

		[HttpGet("Logout")]
		public ActionResult<SamlIdpSessionResponse> Logout([FromQuery] string samlRequest, [FromQuery] string relayState)
		{
			string saml2RequestDecoded = samlRequest.DeflateDecompress();

			LogoutRequest logoutRequest = Serialization.DeserializeFromXmlString<LogoutRequest>(saml2RequestDecoded);
			SamlIdpService samlIdpService = _samlIdentityProvidersManager.GetSamlIdpService(logoutRequest.Issuer.Value);
			LogoutResponse response = new LogoutResponse
			{
				InResponseTo = logoutRequest.ID,
				Status = new Status
				{
					StatusCode = new StatusCode
					{
						Value = Saml2Constants.StatusCodes.Success
					}
				}
			};
			string responseString = Serialization.SerializeToXmlString(response);
			string responseStringDeflated = responseString.DeflateEncode();
			samlIdpService.ProduceSignedResponse(responseStringDeflated, out string signature, out string signatureAlgorithm);

			SamlIdpSessionResponse sessionResponse = new SamlIdpSessionResponse
			{
				RedirectUri = $"{logoutRequest.Issuer.Value}/Saml2/SingleLogoutService",
				Saml2Response = new Saml2.Authentication.Core.Bindings.Saml2Response
				{
					Response = responseStringDeflated.UrlEncode(),
					RelayState = relayState
				},
				Signature = signature.UrlEncode(),
				SignatureAlgorithm = signatureAlgorithm.UrlEncode()
			};
			return Ok(sessionResponse);
		}

		/// <summary>
		/// This function is invoked from a mobile device for obtaining details about authentication process to be performed
		/// </summary>
		/// <param name="sessionInfo"></param>
		/// <returns></returns>
		[HttpGet("GetSessionInfo")]
		public ActionResult<SamlIdpSessionInfo> GetSessionInfo([FromQuery] string sessionInfo)
		{
            SamlIdpSession samlIdpSession = Serialization.DeserializeFromXmlString<SamlIdpSession>(sessionInfo.DeflateDecompress());
			SamlIdpService samlIdpService = _samlIdentityProvidersManager.GetSamlIdpService(samlIdpSession.ServiceName);
            SamlIdpSessionPersistence samlIdpSessionPersistence = samlIdpService.GetSamlIdpSessionPersistence(samlIdpSession.SessionKey);

            return samlIdpSessionPersistence.SessionInfo;
        }

		[HttpGet("SamlLogoutKeyImage")]
		public IActionResult SamlLogoutKeyImage([FromQuery] string entityId, [FromQuery] string keyImage)
		{
			SamlIdpService samlIdpService = _samlIdentityProvidersManager.GetSamlIdpService(entityId);
			samlIdpService.SamlForceLogout(keyImage);

			return Ok();
		}

        [HttpPut("ForceLogin")]
        public IActionResult ForceLogin([FromQuery] string entityId, [FromQuery] string registrationCommitment, [FromQuery] string sessionKey, [FromQuery] string keyImage)
        {
            SamlIdpService samlIdpService = _samlIdentityProvidersManager.GetSamlIdpService(entityId);
            samlIdpService.PropagateResponse(registrationCommitment, sessionKey, keyImage);

            return Ok();
        }
    }
}
