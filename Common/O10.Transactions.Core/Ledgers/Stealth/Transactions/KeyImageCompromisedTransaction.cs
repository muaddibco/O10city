﻿using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public class KeyImageCompromisedTransaction : O10StealthTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Stealth_KeyImageCompromised;
    }
}
