using System;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.Web.Common.Services;
using O10.Core.Architecture;
using O10.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Communication;
using O10.Core.Models;
using O10.Client.Common.Communication;
using O10.Transactions.Core.Ledgers;

namespace O10.Server.IdentityProvider.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionContext), Lifetime = LifetimeManagement.Singleton)]
	public class ExecutionContext : IExecutionContext
	{
		private Persistency _persistency;
		private readonly IServiceProvider _serviceProvider;
		private readonly IGatewayService _gatewayService;
		private readonly ILogger _logger;

		public ExecutionContext(IServiceProvider serviceProvider, IGatewayService gatewayService, ILoggerService loggerService)
		{
			_serviceProvider = serviceProvider;
			_gatewayService = gatewayService;
			_logger = loggerService.GetLogger(nameof(ExecutionContext));
		}

		public void Initialize(long accountId, byte[] secretKey)
		{
			_logger.Info($"{nameof(Initialize)} for account with id {accountId}");


			_persistency = new Persistency(accountId, _serviceProvider);

			try
			{
				IStateTransactionsService transactionsService = _persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
				IStateClientCryptoService clientCryptoService = _persistency.Scope.ServiceProvider.GetService<IStateClientCryptoService>();

				clientCryptoService.Initialize(secretKey);

				transactionsService.Initialize(accountId);
				transactionsService.GetSourcePipe<TaskCompletionWrapper<PacketBase>>().LinkTo(_gatewayService.PipeInTransactions);

			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during {nameof(Initialize)} for account with id {accountId}", ex);
				throw;
			}
		}

		public Persistency GetContext() => _persistency;
	}
}
