using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Crypto.Models;

namespace O10.Client.Common.Communication
{
    public abstract class SynchronizerBase : ISynchronizer
    {
        private bool _disposedValue; // To detect redundant calls

        private readonly IPropagatorBlock<TransactionBase, TransactionBase> _pipeOutTransactions;
        private readonly IPropagatorBlock<NotificationBase, NotificationBase> _pipeOutNotifications;

        protected long _accountId;
        protected readonly IDataAccessService _dataAccessService;
        private readonly ITargetBlock<TaskCompletionWrapper<TransactionBase>> _pipeInTransactions;
        private readonly ITargetBlock<WitnessPackage> _pipeInPackage;
        protected readonly IClientCryptoService _clientCryptoService;
        protected ILogger _logger;

        protected SynchronizerBase(IDataAccessService dataAccessService, IClientCryptoService clientCryptoService, ILoggerService loggerService)
        {
            _dataAccessService = dataAccessService;
            _clientCryptoService = clientCryptoService;
            _logger = loggerService.GetLogger(GetType().Name);

            _pipeInTransactions = new ActionBlock<TaskCompletionWrapper<TransactionBase>>(async p =>
            {
                try
                {
                    if (p.State is StealthTransactionBase packet)
                    {
                        _logger.LogIfDebug(() => $"[{_accountId}]: ==> Processing {packet.GetType().Name} with {nameof(packet.KeyImage)}={packet.KeyImage}...");
                    }
                    else
                    {
                        _logger.LogIfDebug(() => $"[{_accountId}]: ==> Processing {p.State.GetType().Name}...");
                    }

                    await StorePacket(p.State).ConfigureAwait(false);

                    _logger.LogIfDebug(() => $"[{_accountId}]: ==> {p.State.GetType().Name} completed");
                    p.TaskCompletion.SetResult(new SucceededNotification());
                }
                catch (Exception ex)
                {
                    p.TaskCompletion.SetException(ex);

                    _logger.Error($"[{_accountId}]: Failed to proccess incoming packet {p.GetType().Name}", ex);
                }
            });

            _pipeInPackage = new ActionBlock<WitnessPackage>(async w =>
            {
                _logger.Debug($"[{_accountId}]: updating in DB last updated aggregated height {w.CombinedBlockHeight} started...");
                try
                {
                    bool tryAgain = true;
                    int attempts = 3;
                    do
                    {
                        try
                        {
                            _dataAccessService.StoreLastUpdatedCombinedBlockHeight(_accountId, w.CombinedBlockHeight);
                            tryAgain = false;
                        }
                        catch (Exception ex)
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                            _logger.Error($"[{_accountId}]: Failure during updating last aggregated height {w.CombinedBlockHeight}", ex);
                        }
                    } while (tryAgain && attempts-- > 0);
                }
                catch(Exception ex)
                {
                    _logger.Debug($"[{_accountId}]: updating in DB last updated aggregated height {w.CombinedBlockHeight} failed", ex);
                }
                finally
                {
                    _logger.Debug($"[{_accountId}]: updating in DB last updated aggregated height {w.CombinedBlockHeight} completed");
                }
            });

            _pipeOutTransactions = new TransformBlock<TransactionBase, TransactionBase>(w => w);
            _pipeOutNotifications = new TransformBlock<NotificationBase, NotificationBase>(p => p);
        }

        public abstract string Name { get; }

        public virtual void Initialize(long accountId)
        {
            _accountId = accountId;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if (typeof(T) == typeof(TransactionBase))
            {
                return (ISourceBlock<T>)_pipeOutTransactions;
            }
            else if (typeof(T) == typeof(NotificationBase))
            {
                return (ISourceBlock<T>)_pipeOutNotifications;
            }

            throw new InvalidOperationException($"No source blocks are available for type {typeof(T).FullName}");
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            if (typeof(T) == typeof(TaskCompletionWrapper<TransactionBase>))
            {
                return (ITargetBlock<T>)_pipeInTransactions;
            }
            else if (typeof(T) == typeof(WitnessPackage))
            {
                return (ITargetBlock<T>)_pipeInPackage;
            }

            throw new InvalidOperationException($"No target blocks are available for type {typeof(T).FullName}");
        }

        protected virtual async Task StorePacket(TransactionBase transaction)
        {
            _logger.LogIfDebug(() => $"[{_accountId}]: => awaiting for {transaction.GetType().Name} updater completion...");
            await _pipeOutTransactions.SendAsync(transaction).ConfigureAwait(false);
            _logger.LogIfDebug(() => $"[{_accountId}]: => awaiting for {transaction.GetType().Name} updater completed");
        }

        protected void NotifyObservers(NotificationBase notification)
        {
            _pipeOutNotifications.SendAsync(notification);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _pipeInPackage.Complete();
                    _pipeInTransactions.Complete();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WalletSynchronizer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
