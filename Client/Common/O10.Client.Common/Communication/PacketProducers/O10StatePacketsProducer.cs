using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Logging;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.PacketProducers
{
    [RegisterExtension(typeof(IPacketsProducer), Lifetime = LifetimeManagement.Scoped)]
    public class O10StatePacketsProducer : IPacketsProducer
    {
        private readonly IPropagatorBlock<TransactionBase, IPacketBase> _pipe;
        private readonly IStateClientCryptoService _clientCryptoService;
        private readonly ISigningService _signingService;
        private readonly IGatewayService _gatewayService;
        protected readonly ILogger _logger;
        protected long _accountId;
        private long _lastHeight;

        public O10StatePacketsProducer(IStateClientCryptoService clientCryptoService, ISigningService signingService, IGatewayService gatewayService, ILoggerService loggerService)
        {
            _pipe = new TransformBlock<TransactionBase, IPacketBase>(t => ProducePacket(t));
            _clientCryptoService = clientCryptoService;
            _signingService = signingService;
            _gatewayService = gatewayService;
            _logger = loggerService.GetLogger(GetType().Name);
        }

        // TODO: make is configurable!
        public IEnumerable<LedgerType> LedgerTypes => new List<LedgerType> { LedgerType.O10State };

        public async Task Initialize(long accountId)
        {
            _accountId = accountId;
            long lastBlockHeight = (await _gatewayService.GetLastPacketInfo(_clientCryptoService.GetPublicKey()).ConfigureAwait(false)).Height;
            _lastHeight = lastBlockHeight + 1;
        }

        private IPacketBase ProducePacket(TransactionBase transactionBase)
        {
            if (!(transactionBase is O10StateTransactionBase o10StateTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(transactionBase));
            }

            O10StatePayload payload = new O10StatePayload(o10StateTransaction, _lastHeight++);

            var signature = _signingService.Sign(payload) as SingleSourceSignature;

            var packet = new O10StatePacket()
            {
                Payload = payload,
                Signature = signature
            };

            return packet;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if (typeof(T) == typeof(IPacketBase))
            {
                return (ISourceBlock<T>)_pipe;
            }

            throw new InvalidOperationException($"No source blocks are available for type {typeof(T).FullName}");
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            if (typeof(T) == typeof(TransactionBase))
            {
                return (ITargetBlock<T>)_pipe;
            }

            throw new InvalidOperationException($"No target blocks are available for type {typeof(T).FullName}");
        }
    }
}
