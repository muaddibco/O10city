﻿using O10.Core.Cryptography;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public class RevokeIdentityTransaction : O10StealthTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Stealth_RevokeIdentity;

        public SurjectionProof EligibilityProof { get; set; }
    }
}
