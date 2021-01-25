using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.DataLayer.Services;
using O10.Client.Web.Portal.Dtos.ElectionCommittee;
using O10.Client.Web.Portal.ElectionCommittee;
using O10.Client.Web.Portal.ElectionCommittee.Models;
using O10.Client.Web.Portal.Services;
using O10.Core.Logging;
using System;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElectionCommitteeController : ControllerBase
    {
        private readonly IElectionCommitteeService _electionCommitteeService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly ILogger _logger;

        public ElectionCommitteeController(
            IElectionCommitteeService electionCommitteeService,
            IDataAccessService dataAccessService,
            IExecutionContextManager executionContextManager,
            ILoggerService loggerService)
        {
            _electionCommitteeService = electionCommitteeService;
            _dataAccessService = dataAccessService;
            _executionContextManager = executionContextManager;
            _logger = loggerService.GetLogger(nameof(ElectionCommitteeController));
        }

        [HttpPost("Poll")]
        public IActionResult RegisterPoll([FromBody] RegisterPollDto pollRequest)
        {
            if (pollRequest is null)
            {
                throw new ArgumentNullException(nameof(pollRequest));
            }

            return Ok(_electionCommitteeService.RegisterPoll(pollRequest.Name));
        }

        [HttpGet("Poll/{pollId}")]
        public IActionResult GetPoll(long pollId)
        {
            return Ok(_electionCommitteeService.GetPoll(pollId));
        }

        [HttpPut("Poll/{pollId}/State")]
        public IActionResult SetPollState(long pollId, [FromBody] SetPollStateRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var poll = _electionCommitteeService.SetPollState(pollId, request.State);
            var pollModel = _dataAccessService.GetEcPoll(pollId);
            var account = _dataAccessService.GetAccount(pollModel.AccountId);
            if(poll.State == PollState.Started)
            {
                _executionContextManager.InitializeStateExecutionServices(account.AccountId, account.SecretSpendKey);
                _electionCommitteeService.IssueVotersRegistrations(pollId, request.SourceAccountId);
            }
            else
            {
                _executionContextManager.UnregisterExecutionServices(pollModel.AccountId);
            }


            return Ok(poll);
        }

        [HttpGet("Polls")]
        public IActionResult GetPolls(PollState? pollState)
        {
            return Ok(_electionCommitteeService.GetPolls(pollState));
        }

        [HttpPost("Poll/{pollId}/Candidate")]
        public IActionResult AddPollCandidate(long pollId, RegisterCandidateDto registerCandidate)
        {
            if (registerCandidate is null)
            {
                throw new ArgumentNullException(nameof(registerCandidate));
            }

            return Ok(_electionCommitteeService.AddCandidateToPoll(pollId, registerCandidate.Name));
        }

        [HttpPost("Poll/{pollId}/Commitment")]
        public IActionResult GenerateDerivedCommitment(long pollId, [FromBody] SelectionCommitmentRequest request)
        {
            return Ok(_electionCommitteeService.GenerateDerivedCommitment(pollId, request));
        }

        [HttpPost("Poll/{pollId}/Proof")]
        public IActionResult CalculateSurjectionProof(long pollId, [FromBody] EcSurjectionProofRequest request)
        {
            return Ok(_electionCommitteeService.CalculateEcCommitmentProof(pollId, request));
        }

        [HttpPost("Poll/{pollId}/Vote")]
        public async Task<ActionResult<VoteCastedResult>> CastVote(long pollId, [FromBody] ElectionCommitteePayload proofs)
        {
            return Ok(new VoteCastedResult { Result = await _electionCommitteeService.WaitForVoteCast(pollId, proofs.EcCommitment).ConfigureAwait(false) });
        }

        [HttpGet("Poll/{pollId}/Votes")]
        public IActionResult GetVotes(long pollId)
        {
            return Ok(_electionCommitteeService.CalculateResults(pollId));
        }
    }
}
