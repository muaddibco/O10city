using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.States;
using O10.Core;
using O10.Network.Handlers;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Network.Synchronization;

namespace O10.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Scoped)]
    public class SynchronizationConfirmedVerifier : IPacketVerifier
    {
        private readonly ISynchronizationContext _synchronizationContext;

		public SynchronizationConfirmedVerifier(IStatesRepository statesRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
		}

        public LedgerType LedgerType => LedgerType.Synchronization;

        public bool ValidatePacket(IPacketBase packet)
        {
            if (packet is null)
            {
                throw new System.ArgumentNullException(nameof(packet));
            }

            if (packet.Transaction is SynchronizationConfirmedTransaction transaction)
            {
                if (_synchronizationContext.LastBlockDescriptor != null && _synchronizationContext.LastBlockDescriptor.BlockHeight + 1 <= packet.AsPacket<SynchronizationPacket>().Payload.Height || _synchronizationContext.LastBlockDescriptor == null)
                {
                    if (_synchronizationContext.LastBlockDescriptor != null && packet.AsPacket<SynchronizationPacket>().Payload.HashPrev.Equals(_synchronizationContext.LastBlockDescriptor.Hash) ||
                        _synchronizationContext.LastBlockDescriptor == null && packet.AsPacket<SynchronizationPacket>().Payload.HashPrev.Equals(new byte[Globals.DEFAULT_HASH_SIZE]))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }
    }
}
