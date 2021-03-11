using O10.Crypto.Models;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Accessors
{
    public class O10StateAccessor : AccessorBase
    {
        public override LedgerType LedgerType => LedgerType.O10State;

        public override Task<Memory<byte>> GetPacket(EvidenceDescriptor evidence)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> VerifyPacket(EvidenceDescriptor evidence, Memory<byte> packet)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetAccessingKeys()
        {
            throw new NotImplementedException();
        }

        protected override Task<TransactionBase> GetTransactionInner(EvidenceDescriptor accessDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}
