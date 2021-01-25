using O10.Core.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class SelectionCommitmentRequest
    {
        public byte[] Commitment { get; set; }

        public SurjectionProof CandidateCommitmentProofs { get; set; }
        public CandidateCommitment[] CandidateCommitments { get; set; }
    }

    public class CandidateCommitment
    {
        public byte[] Commitment { get; set; }
        public SurjectionProof IssuanceProof { get; set; }
    }
}
