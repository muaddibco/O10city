using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.Web.Portal.Hubs;
using O10.Client.Web.Portal.Services;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsentManagementController : ControllerBase
    {
        private readonly IConsentManagementService _consentManagementService;
        private readonly IHubContext<ConsentManagementHub> _hubContext;

        public ConsentManagementController(IConsentManagementService consentManagementService, IHubContext<ConsentManagementHub> hubContext)
        {
            _consentManagementService = consentManagementService;
            _hubContext = hubContext;
        }

        [HttpPost("RelationProofsData")]
        public IActionResult PushRelationProofsData(string sessionKey, [FromBody] RelationProofsData relationProofSession)
        {
            return Ok(new { suceeded = _consentManagementService.PushRelationProofsData(sessionKey, relationProofSession) });
        }

        [HttpGet("RelationProofSession")]
        public IActionResult PopRelationProofSession(string sessionKey)
        {
            var relationProofSession = _consentManagementService.PopRelationProofSession(sessionKey);

            if (relationProofSession != null)
            {
                return Ok(relationProofSession);
            }

            return BadRequest();
        }

        [HttpPost("ChallengeProofs")]
        public async Task<IActionResult> ChallengeProofs(string key, [FromBody] ProofsRequest proofsRequest)
        {
            string sessionKey = _consentManagementService.InitiateRelationProofsSession(proofsRequest);

            ProofsChallenge proofsChallenge = new ProofsChallenge
            {
                Key = key,
                PublicSpendKey = _consentManagementService.PublicSpendKey,
                PublicViewKey = _consentManagementService.PublicViewKey,
                SessionKey = sessionKey,
                WithKnowledgeProof = proofsRequest.WithKnowledgeProof,
                WithBiometricProof = proofsRequest.WithBiometricProof
            };

            await _hubContext.Clients.Group(key).SendAsync("ChallengeProofs", proofsChallenge).ConfigureAwait(false);

            return Ok(new { SessionKey = sessionKey });
        }

        [HttpPost("TransactionForConsent")]
        public IActionResult RegisterTransactionForConsent([FromBody] TransactionConsentRequest consentRequest)
        {
            _consentManagementService.RegisterTransactionForConsent(consentRequest);

            _hubContext.Clients.Group(consentRequest.RegistrationCommitment).SendAsync("ConsentRequest", consentRequest);
            return Ok();
        }

        [HttpGet("TransactionsForConsent")]
        public IActionResult GetTransactionsForConsent(string registrationCommitment)
        {
            return Ok(_consentManagementService.GetTransactionConsentRequests(registrationCommitment));
        }
    }
}
