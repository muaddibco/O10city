using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.States;
using O10.Network.Interfaces;

namespace O10.Node.Core.Storage
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class StealthStorageHandler : StorageHandlerBase<StealthBase>
    {
        public const string NAME = "StealthStorage";

        public StealthStorageHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IChainDataServicesManager chainDataServicesManager) 
			: base(statesRepository, communicationServicesRegistry, chainDataServicesManager)
        {
        }

        public override string Name => NAME;

        public override LedgerType PacketType => LedgerType.Stealth;
    }
}
