using O10.Core.Identity;
using O10.Crypto.Models;

namespace O10.Client.Common.Dtos.UniversalProofs
{
    public class PayloadBase
    {
        public IKey Commitment { get; set; }
        public SurjectionProof BindingProof { get; set; }
    }
}
