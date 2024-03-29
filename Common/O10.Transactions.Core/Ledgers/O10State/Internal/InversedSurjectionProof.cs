﻿using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class InversedSurjectionProof
    {
        public InversedSurjectionProof()
        {
            Rs = new BorromeanRingSignature();
        }

        public byte[] AssetCommitment { get; set; }
        public BorromeanRingSignature Rs { get; set; }
    }
}
