using System.Collections.Generic;

namespace O10.Client.Common.Dtos.UniversalProofs
{
    public class RelationsCreationPayload : PayloadBase
    {
        public List<GroupRelationCreation>? GroupRelations { get; set; }
    }

    /// <summary>
    /// Commitment:
    /// Commitment that is Bounded commitment (C = Pb + Ir + Ig)
    /// BindingProof:
    /// A proof of binding to the commitment of the payload
    /// </summary>
    public class GroupRelationCreation : BoundedAttributeProof
    {
        public string? GroupDid { get; set; }
    }
}
