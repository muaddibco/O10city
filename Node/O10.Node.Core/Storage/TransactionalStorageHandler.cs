//using O10.Transactions.Core.Ledgers.O10State;
//using O10.Transactions.Core.Enums;
//using O10.Transactions.Core.Interfaces;
//using O10.Node.DataLayer.DataServices;
//using O10.Core.Architecture;
//using O10.Core.States;
//using O10.Network.Interfaces;

//namespace O10.Node.Core.Storage
//{
//    [RegisterExtension(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
//    public class TransactionalStorageHandler : StorageHandlerBase<O10StatePacket>
//    {
//        public const string NAME = "TransactionalStorage";
//        public TransactionalStorageHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IChainDataServicesManager chainDataServicesManager) 
//            : base(statesRepository, communicationServicesRegistry, chainDataServicesManager)
//        {
//        }

//        public override string Name => NAME;

//        public override LedgerType LedgerType => LedgerType.O10State;
//    }
//}
