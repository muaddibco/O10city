//using NSubstitute;
//using System;
//using System.Threading;
//using O10.Transactions.Core.Parsers;
//using O10.Client.Common.Communication;
//using O10.Client.Common.Communication.SynchronizerNotifications;
//using O10.Client.Common.Configuration;
//using O10.Gateway.DataModel.Services;
//using O10.Core.Communication;
//using O10.Core.Configuration;
//using O10.Core.HashCalculations;
//using O10.Core.Identity;
//using O10.Core.Logging;
//using O10.Gateway.Common.Services;
//using O10.Network.Interfaces;
//using O10.Network.Topology;
//using O10.Tests.Core.Fixtures;
//using Xunit;

//namespace O10.Client.Common.Tests
//{
//    public class NetworkSynchronizerTest : IClassFixture<DependencyInjectionSupportFixture>
//    {
//        #region ============================================ MEMBERS ==================================================


//        NetworkSynchronizer _networkSynchronizer;

//        IClientCommunicationServiceRepository _clientCommunicationServiceRepository;
//        ICommunicationService _communicationService;
//        IDataAccessService _dataAccessService;
//        IHashCalculationsRepository _hashCalculationsRepository;
//        IConfigurationService _configurationService;
//        IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
//        IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
//        INodesResolutionService _nodesResolutionService;
//        IHashCalculation _defaultHashCalculation;
//        ISynchronizerConfiguration _synchronizerConfiguration;
//        IIdentityKeyProvider _identityKeyProvider;
//        ILogger _logger;
//        ILoggerService _loggerService;

//        #endregion

//        #region ========================================== CONSTRUCTORS ===============================================

//        public NetworkSynchronizerTest()
//        {
//            _clientCommunicationServiceRepository = Substitute.For<IClientCommunicationServiceRepository>();
//            _communicationService = Substitute.For<ICommunicationService>();
//            _dataAccessService = Substitute.For<IDataAccessService>(); 
//            _hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
//            _configurationService = Substitute.For<IConfigurationService>();
//            _blockParsersRepositoriesRepository = Substitute.For<IBlockParsersRepositoriesRepository>();
//            _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
//            _nodesResolutionService = Substitute.For<INodesResolutionService>();
//            _loggerService = Substitute.For<ILoggerService>();
//            _logger = Substitute.For<ILogger>();
//            _networkSynchronizer = new NetworkSynchronizer(_clientCommunicationServiceRepository, _dataAccessService, _hashCalculationsRepository, _configurationService, _blockParsersRepositoriesRepository, _identityKeyProvidersRegistry, _nodesResolutionService, _loggerService);
//        }

//        #endregion

//        #region ======================================== PUBLIC FUNCTIONS =============================================

//        [Fact]
//        public void SendDataSucceeded()
//        {
//            IPacketProvider transactionPacketProvider = null;
//            IPacketProvider witnessPacketProvider = null;
//            bool retval = _networkSynchronizer.SendData(transactionPacketProvider, witnessPacketProvider);

//            Assert.True(retval);
//        }

//        [Fact]
//        public void SendDataWithKeyImageSucceeded()
//        {
//            IPacketProvider transactionPacketProvider = null;
//            IPacketProvider witnessPacketProvider = null;
//            byte[] keyImage = null;
//            _networkSynchronizer.SendData(transactionPacketProvider, witnessPacketProvider, keyImage)
//                .ContinueWith(t =>
//                {
//                    if (!t.IsCanceled && t.IsCompleted && !t.IsFaulted)
//                    {
//                        Assert.True(t.Result);
//                    }
//                });
//        }

//        [Fact]
//        public void StartSucceeded()
//        {
//            _networkSynchronizer.Start();

//            _logger.WhenForAnyArgs(t => 
//                t.Info(""));
//            _communicationService.WhenForAnyArgs(t => 
//                t.Start());
//        }

//        [Fact]
//        public void InitializeSucceeded()
//        {
//            CancellationToken token = new CancellationToken(true);

//            _networkSynchronizer.Initialize(token);

//            _logger.Received(0).Error("Failure during initializtion");
//        }

//        [Fact]
//        public void GetLastSyncBlockSucceeded()
//        {
//            var lastBlock = _networkSynchronizer.GetLastSyncBlock();

//            Assert.True(lastBlock != null);
//        }

//        [Fact]
//        public void GetLastBlockSucceeded()
//        {
//            var lastBlock = _networkSynchronizer.GetLastBlock(new byte[] { 1,2 });

//            Assert.True(lastBlock != null);
//        }

//        #endregion

//        #region ======================================== PRIVATE FUNCTIONS ============================================

//        #endregion

//    }
//}