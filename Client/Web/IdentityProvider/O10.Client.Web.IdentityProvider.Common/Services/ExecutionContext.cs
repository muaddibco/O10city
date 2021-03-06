﻿using System;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.Web.Common.Services;
using O10.Core.Architecture;
using O10.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using O10.Transactions.Core.Enums;

namespace O10.Server.IdentityProvider.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionContext), Lifetime = LifetimeManagement.Singleton)]
	public class ExecutionContext : IExecutionContext
	{
		private Persistency _persistency;
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger _logger;

		public ExecutionContext(IServiceProvider serviceProvider, ILoggerService loggerService)
		{
			_serviceProvider = serviceProvider;
			_logger = loggerService.GetLogger(nameof(ExecutionContext));
		}

		public async Task Initialize(long accountId, byte[] secretKey)
		{
			_logger.Info($"{nameof(Initialize)} for account with id {accountId}");


			_persistency = new Persistency(accountId, _serviceProvider);

			try
			{
				IStateTransactionsService transactionsService = _persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
				IStateClientCryptoService clientCryptoService = _persistency.Scope.ServiceProvider.GetService<IStateClientCryptoService>();
				ILedgerWriterRepository ledgerWriterRepository = _persistency.Scope.ServiceProvider.GetService<ILedgerWriterRepository>();

				clientCryptoService.Initialize(secretKey);

				await transactionsService.Initialize(accountId).ConfigureAwait(false);
				var ledgerWriter = ledgerWriterRepository.GetInstance(LedgerType.O10State);
				await ledgerWriter.Initialize(accountId).ConfigureAwait(false);
				transactionsService.PipeOutTransactions.LinkTo(ledgerWriter.PipeIn);

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
