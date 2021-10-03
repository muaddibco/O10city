using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketVerifiersRepository), Lifetime = LifetimeManagement.Scoped)]
    public class PacketVerifiersRepository : IPacketVerifiersRepository
    {
        private readonly ILogger _log;
        private readonly Dictionary<LedgerType, IPacketVerifier> _packetVerifiers;

        public PacketVerifiersRepository(IEnumerable<IPacketVerifier> packetVerifiers, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _packetVerifiers = new Dictionary<LedgerType, IPacketVerifier>();

            foreach (var packetVerifier in packetVerifiers)
            {
                if(!_packetVerifiers.ContainsKey(packetVerifier.LedgerType))
                {
                    _packetVerifiers.Add(packetVerifier.LedgerType, packetVerifier);
                }
            }
        }

        public IPacketVerifier GetInstance(LedgerType ledgerType)
        {
            if (!_packetVerifiers.ContainsKey(ledgerType))
            {
                _log.Debug($"No verifier found for packet type {ledgerType}");

                return null;
            }

            return _packetVerifiers[ledgerType];
        }
    }
}
