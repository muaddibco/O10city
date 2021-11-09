using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Core;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Crypto.Models;

namespace O10.Client.Common.Communication
{
    public abstract class TransactionsServiceBase : ITransactionsService
    {
        protected readonly IGatewayService _gatewayService;
        protected readonly IHashCalculation _hashCalculation;
        protected readonly IHashCalculation _proofOfWorkCalculation;
        protected readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, TaskCompletionWrapper<TransactionBase>> _pipeOutTransactions;
        protected ISigningService _signingService;
        protected readonly List<IObserver<NotificationBase>> _observers;
        protected readonly ILogger _logger;
        protected long _accountId;

        protected TransactionsServiceBase(
            IHashCalculationsRepository hashCalculationsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ISigningService signingService,
            IGatewayService gatewayService,
            ILoggerService loggerService)
        {
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _proofOfWorkCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _observers = new List<IObserver<NotificationBase>>();
            _pipeOutTransactions = new TransformBlock<TaskCompletionWrapper<TransactionBase>, TaskCompletionWrapper<TransactionBase>>(w => w);
            _signingService = signingService;
            _gatewayService = gatewayService;
            _logger = loggerService.GetLogger(GetType().Name);
        }

        public ISourceBlock<TaskCompletionWrapper<TransactionBase>> PipeOutTransactions => _pipeOutTransactions;

        protected TaskCompletionSource<NotificationBase> PropagateTransaction(TransactionBase transaction, object? propagationArgument = null)
        {
            var transactionWrapper = new TaskCompletionWrapper<TransactionBase>(transaction, propagationArgument);

            _pipeOutTransactions.SendAsync(transactionWrapper);

            return transactionWrapper.TaskCompletion;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly TransactionsServiceBase _service;
            private IObserver<NotificationBase> _observer;

            public Subscription(TransactionsServiceBase service, IObserver<NotificationBase> observer)
            {
                _service = service;
                _observer = observer;
            }

            public void Dispose()
            {
                IObserver<NotificationBase> observer = _observer;
                if (null != observer)
                {
                    lock (_service._observers)
                    {
                        _service._observers.Remove(observer);
                    }
                    _observer = null;
                }
            }
        }
    }
}
