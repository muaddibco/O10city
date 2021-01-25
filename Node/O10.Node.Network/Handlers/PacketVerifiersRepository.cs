using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketVerifiersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class PacketVerifiersRepository : IPacketVerifiersRepository
    {
        private readonly ILogger _log;
        private readonly Dictionary<PacketType, IPacketVerifier> _packetVerifiers;

        public PacketVerifiersRepository(IEnumerable<IPacketVerifier> packetVerifiers, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _packetVerifiers = new Dictionary<PacketType, IPacketVerifier>();

            foreach (var packetVerifier in packetVerifiers)
            {
                if(!_packetVerifiers.ContainsKey(packetVerifier.PacketType))
                {
                    _packetVerifiers.Add(packetVerifier.PacketType, packetVerifier);
                }
            }
        }

        public IPacketVerifier GetInstance(PacketType packetType)
        {
            if (!_packetVerifiers.ContainsKey(packetType))
            {
                _log.Debug($"No verifier found for packet type {packetType}");

                return null;
            }

            return _packetVerifiers[packetType];
        }
    }
}
