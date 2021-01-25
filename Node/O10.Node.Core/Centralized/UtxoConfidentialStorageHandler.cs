using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.States;
using O10.Network.Interfaces;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class StealthStorageHandler : StorageHandlerBase<StealthBase>
    {
        public const string NAME = "StealthStorageRT";

        public StealthStorageHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IChainDataServicesManager chainDataServicesManager, IRealTimeRegistryService realTimeRegistryService, ILoggerService loggerService)
			: base(statesRepository, communicationServicesRegistry, chainDataServicesManager, realTimeRegistryService, loggerService)
        {
        }

        public override string Name => NAME;

        public override PacketType PacketType => PacketType.Stealth;
    }
}
