using O10.Client.Web.Portal.ElectionCommittee.Models;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.ElectionCommittee
{
    [ServiceContract]
    public interface IElectionCommitteeService
    {
        void Initialize();
        Poll RegisterPoll(string name);
        Poll SetPollState(long pollId, PollState pollState);
        Poll GetPoll(long pollId);
        List<Poll> GetPolls(PollState? pollState);
        Candidate AddCandidateToPoll(long pollId, string name);
        Candidate SetCandidateState(long candidateId, bool isActive);
        Task IssueVotersRegistrations(long pollId, long issuerAccountId);
        SignedEcCommitment GenerateDerivedCommitment(long pollId, SelectionCommitmentRequest request);

        SurjectionProof CalculateEcCommitmentProof(long pollId, EcSurjectionProofRequest proofRequest);
        void UpdatePollSelection(long pollId, ElectionCommitteePayload payload);
        Task<bool> WaitForVoteCast(long pollId, IKey ecCommitment);

        IEnumerable<PollResult> CalculateResults(long pollId);
    }
}
