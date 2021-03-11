using O10.Core.Cryptography;
using O10.Transactions.Core.Enums;
using System;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public class UniversalStealthTransaction : O10StealthTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Stealth_UniversalTransport;
    }
}
