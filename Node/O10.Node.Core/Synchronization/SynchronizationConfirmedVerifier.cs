using O10.Transactions.Core.DataModel;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Core.Models;
using O10.Core;
using O10.Network.Handlers;

namespace O10.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfirmedVerifier : IPacketVerifier
    {
        private readonly ISynchronizationContext _synchronizationContext;
		private readonly ILoggerService _loggerService;

		public SynchronizationConfirmedVerifier(IStatesRepository statesRepository, ILoggerService loggerService)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
			_loggerService = loggerService;
		}

        public PacketType PacketType => PacketType.Synchronization;

        public bool ValidatePacket(PacketBase blockBase)
        {
            LinkedPacketBase syncedBlockBase = (LinkedPacketBase)blockBase;

            if(syncedBlockBase.BlockType == ActionTypes.Synchronization_ConfirmedBlock && syncedBlockBase.Version == 1)
            {
                if (_synchronizationContext.LastBlockDescriptor != null && _synchronizationContext.LastBlockDescriptor.BlockHeight + 1 <= syncedBlockBase.BlockHeight || _synchronizationContext.LastBlockDescriptor == null)
                {
                    if (_synchronizationContext.LastBlockDescriptor != null && syncedBlockBase.HashPrev.Equals32(_synchronizationContext.LastBlockDescriptor.Hash) ||
                        _synchronizationContext.LastBlockDescriptor == null && syncedBlockBase.HashPrev.Equals32(new byte[Globals.DEFAULT_HASH_SIZE]))
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
