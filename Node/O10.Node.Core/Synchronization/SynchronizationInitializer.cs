//using System;
//using System.Linq;
//using System.Threading;
//using O10.Transactions.Core.Ledgers.Synchronization;
//using O10.Transactions.Core.Enums;
//using O10.Node.DataLayer.DataServices;
//using O10.Node.DataLayer.DataServices.Keys;
//using O10.Core;
//using O10.Core.Architecture;
//using O10.Core.HashCalculations;
//using O10.Core.Logging;
//using O10.Core.States;
//using O10.Core.Synchronization;

//namespace O10.Node.Core.Synchronization
//{
//    /// <summary>
//    /// Set SynchronizationContext according to information in database
//    /// </summary>
//    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
//    public class SynchronizationInitializer : InitializerBase
//    {
//        private readonly ILogger _logger;
//        private readonly IChainDataService _chainDataService;
//        private readonly ISynchronizationContext _synchronizationContext;
//        private readonly IHashCalculation _hashCalculation;

//        public SynchronizationInitializer(IStatesRepository statesRepository, IChainDataServicesManager chainDataServicesManager, ILoggerService loggerService, IHashCalculationsRepository hashCalculationsRepository)
//        {
//            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
//            _chainDataService = chainDataServicesManager.GetChainDataService(LedgerType.Synchronization);
//            _logger = loggerService.GetLogger(typeof(SynchronizationInitializer).Name);
//            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
//        }

//        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

//        protected override void InitializeInner(CancellationToken cancellationToken)
//        {
//            _logger.Info("Starting Synchronization Initializer");

//            try
//            {
//                SynchronizationConfirmedBlock synchronizationConfirmedBlock = _chainDataService.Single<SynchronizationConfirmedBlock>(new SingleByBlockTypeKey(TransactionTypes.Synchronization_ConfirmedBlock));

//                if (synchronizationConfirmedBlock != null)
//                {
//                    _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationConfirmedBlock.Height, _hashCalculation.CalculateHash(synchronizationConfirmedBlock.RawData), synchronizationConfirmedBlock.ReportedTime, DateTime.Now, synchronizationConfirmedBlock.Round));
//                }

//                SynchronizationRegistryCombinedBlock combinedBlock = _chainDataService.Single<SynchronizationRegistryCombinedBlock>(new SingleByBlockTypeKey(TransactionTypes.Synchronization_RegistryCombinationBlock));
//                if(combinedBlock != null)
//                {
//                    _synchronizationContext.LastRegistrationCombinedBlockHeight = combinedBlock.Height;
//                }
//            }
//            finally
//            {
//                _logger.Info("Synchronization Initializer completed");
//            }
//        }
//    }
//}
