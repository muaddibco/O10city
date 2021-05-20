using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.LedgerWriters
{
    [RegisterExtension(typeof(ILedgerWriter), Lifetime = LifetimeManagement.Scoped)]
    public class O10StealthLedgerWriter : LedgerWriterBase
    {
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> _pipeIn;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly ISigningService _signingService;
        private readonly IGatewayService _gatewayService;

        public O10StealthLedgerWriter(IStealthClientCryptoService clientCryptoService,
                                      ISigningService signingService,
                                      IGatewayService gatewayService,
                                      ILoggerService loggerService)
            :base(loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _signingService = signingService;
            _gatewayService = gatewayService;
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public override ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn => _pipeIn;

        public override async Task Initialize(long accountId)
        {
            await base.Initialize(accountId).ConfigureAwait(false);
        }
    }
}
