using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Crypto.Services;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.LedgerWriters
{
    [RegisterExtension(typeof(ILedgerWriter), Lifetime = LifetimeManagement.Scoped)]
    public class O10StateLedgerWriter : LedgerWriterBase
    {
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> _pipeIn;
        private readonly IStateClientCryptoService _clientCryptoService;
        private readonly IGatewayService _gatewayService;
        private long _lastHeight;

        public O10StateLedgerWriter(IStateClientCryptoService clientCryptoService,
                                    IGatewayService gatewayService,
                                    ILoggerService loggerService)
            : base(loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _gatewayService = gatewayService;

            _pipeIn = new TransformBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>>(t => ProducePacket(t));
            _pipeIn.LinkTo(_gatewayService.PipeInTransactions);
        }

        // TODO: make is configurable!
        public override LedgerType LedgerType => LedgerType.O10State;

        public override ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn => _pipeIn;

        public override async Task Initialize(long accountId)
        {
            long lastBlockHeight = (await _gatewayService.GetLastPacketInfo(_clientCryptoService.GetPublicKey()).ConfigureAwait(false)).Height;
            _lastHeight = lastBlockHeight + 1;

            await base.Initialize(accountId).ConfigureAwait(false);
        }

        private DependingTaskCompletionWrapper<IPacketBase, TransactionBase> ProducePacket(TaskCompletionWrapper<TransactionBase> wrapper)
        {
            if (!(wrapper.State is O10StateTransactionBase o10StateTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(wrapper));
            }

            O10StatePayload payload = new O10StatePayload(o10StateTransaction, _lastHeight++);

            var signature = _clientCryptoService.Sign(payload) as SingleSourceSignature;

            var packet = new O10StatePacket()
            {
                Payload = payload,
                Signature = signature
            };

            return new DependingTaskCompletionWrapper<IPacketBase, TransactionBase>(packet, wrapper);
        }
    }
}
