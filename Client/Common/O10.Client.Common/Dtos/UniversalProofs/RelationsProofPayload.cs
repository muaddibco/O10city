using O10.Core.Identity;
using O10.Crypto.Models;
using System.Collections.Generic;

namespace O10.Client.Common.Dtos.UniversalProofs
{
    /// <summary>
    /// Commitment that is either Bounded commitment (C = Pb + Ir + Ig) at the time of relation creation 
    /// or a Commitment with a random blinding factor (C = Px + Ir + Ig) at the time of relation proving
    /// </summary>
    public class RelationsProofPayload : PayloadBase
    {
        public IKey Commitment { get; set; }

        public List<GroupRelation> GroupRelations { get; set; }

        public SurjectionProof RelationsProof { get; set; }
    }

    public class GroupRelation
    {
        public string GroupDid { get; set; }
        public List<string> RelationDids { get; set; }
    }
}
