using O10.Core.Cryptography;

namespace O10.Client.Web.Portal.Services.Inherence
{
    public class InherenceData
    {
        /// <summary>
        /// Commitment to the Root Attribute
        /// </summary>
        public byte[] AssetRootCommitment { get; set; }

        public byte[] Issuer { get; set; }

        /// <summary>
        /// Proofs of registration with the Root Attribute
        /// </summary>
        public SurjectionProof RootRegistrationProof { get; set; }

        /// <summary>
        /// Commitment to the Associated Root Attribute
        /// </summary>
        public byte[] AssociatedRootCommitment { get; set; }

        /// <summary>
        /// Proofs of registration with the Associated Root Attribute
        /// </summary>
        public SurjectionProof AssociatedRegistrationProof { get; set; }
    }
}
