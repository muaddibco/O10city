﻿using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class IssuanceProof
    {
        /// <summary>
        /// Surjection Proof 1 X 1 of Blinded Asset against issued raw AssetId
        /// </summary>
        public SurjectionProof SurjectionProof { get; set; }

        /// <summary>
        /// Masked Blinding Factor
        /// </summary>
        public byte[] Mask { get; set; }
    }
}
