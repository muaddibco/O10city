using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System;
using System.Collections.Generic;
using O10.Transactions.Core.Parsers;
using O10.Client.Common.Interfaces;
using O10.Client.DataModel.Enums;
using O10.Client.DataModel.Model;
using O10.Client.DataModel.Services;
using O10.Core.Configuration;
using O10.Network.Tests.Fixtures;
using O10.Client.Web.Portal.Hubs;
using O10.Client.Web.Portal.Services;
using Xunit;

namespace O10.Client.Web.Portal.Test
{
    
    public class ExecutionContextManagerTest : IClassFixture<DependencyInjectionFixture>
    {
        #region ============================================ MEMBERS ==================================================

        private readonly ExecutionContextManager _executionContextManager;

        private readonly IHubContext<IdentitiesHub> _identitiesHubContext;
        private readonly IAssetsService _assetsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        private readonly IAppConfig _appConfig;
        private readonly IGatewayService _gatewayService;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public ExecutionContextManagerTest()
        {
            _executionContextManager = new ExecutionContextManager(
                _identitiesHubContext = Substitute.For<IHubContext<IdentitiesHub>>(),
                _assetsService = Substitute.For<IAssetsService>(),
                _dataAccessService = Substitute.For<IDataAccessService>(),
                _identityAttributesService = Substitute.For<IIdentityAttributesService>(),
                _blockParsersRepositoriesRepository = Substitute.For<IBlockParsersRepositoriesRepository>(),
                _appConfig = Substitute.For<IAppConfig>(),
                _gatewayService = Substitute.For<IGatewayService>()
                );
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void InitializeStateExecutionServicesSucceeded()
        {
            _executionContextManager.InitializeServiceProviderExecutionServices(1, Array.Empty<byte>());

            Assert.True(true);
        }

        [Fact]
        public void InitializeUtxoExecutionServicesSucceeded()
        {
            _executionContextManager.InitializeUtxoExecutionServices((ulong)1, Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>());

            Assert.True(true);
        }

        [Fact]
        public void CleanSucceeded()
        {
            _executionContextManager.Clean(1);

            Assert.True(true);
        }

       

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}