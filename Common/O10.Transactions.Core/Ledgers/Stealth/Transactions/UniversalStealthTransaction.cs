using O10.Core.Cryptography;
using O10.Transactions.Core.Enums;
using System;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public class UniversalStealthTransaction : O10StealthTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Stealth_UniversalTransport;

        /// <summary>
        /// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
        /// </summary>
        public SurjectionProof? OwnershipProof { get; set; }

        public Memory<byte> MessageHash { get; set; }
    }
}
