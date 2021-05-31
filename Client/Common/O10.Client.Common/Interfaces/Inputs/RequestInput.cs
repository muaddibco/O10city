using System;
using O10.Core.Identity;
using O10.Transactions.Core.Ledgers.Stealth.Internal;

namespace O10.Client.Common.Interfaces.Inputs
{
    public class RequestInput
    {
        public byte[] Issuer { get; set; }
        public byte[] Payload { get; set; }
        public byte[] AssetId { get; set; }
        public byte[] EligibilityBlindingFactor { get; set; }
        public byte[] EligibilityCommitment { get; set; }
        public byte[] PrevTransactionKey { get; set; }
        [Obsolete("No need in the previous blinding factor since Ownership Proofs are not required any more")]
        public byte[] PrevBlindingFactor { get; set; }
        public byte[] PrevAssetCommitment { get; set; }
        public byte[] PrevDestinationKey { get; set; }
        public byte[] PublicSpendKey { get; set; }
        public byte[] PublicViewKey { get; set; }
        public IKey? AssetCommitment { get; set; }

        public BiometricProof BiometricProof { get; set; }
    }
}
