using O10.Client.Common.Interfaces;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.LedgerWriters
{
    public class O10StateLedgerWriter : ILedgerWriter
    {
        private readonly IGatewayService _gatewayService;
        private readonly ITargetBlock<IPacketBase> _pipe;
        private long _accountId;

        public O10StateLedgerWriter(IGatewayService gatewayService)
        {
            _gatewayService = gatewayService;
            _pipe = new ActionBlock<IPacketBase>(p => 
            {
                _gatewayService.PipeInTransactions.Post(p);
            });
        }

        public LedgerType LedgerType => LedgerType.O10State;

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            throw new NotImplementedException();
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            throw new NotImplementedException();
        }

        public void Initialize(long accountId)
        {
            _accountId = accountId;
        }
    }
}
