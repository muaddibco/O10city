//using System;
//using O10.Transactions.Core.Enums;
//using O10.Core.Cryptography;

//namespace O10.Transactions.Core.Ledgers.Stealth
//{
//    public class UniversalTransport : StealthTransactionBase
//    {
//        public override ushort Version => 1;

//        public override ushort TransactionType => TransactionTypes.Stealth_UniversalTransport;

//        /// <summary>
//        /// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
//        /// </summary>
//        public SurjectionProof? OwnershipProof { get; set; }

//        public Memory<byte> MessageHash { get; set; }
//    }
//}
