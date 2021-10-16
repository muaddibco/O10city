using O10.Core.Configuration;
using O10.Core.Logging;
using System;
using O10.Core.Persistency.Configuration;
using System.Threading.Tasks;
using System.Threading;

namespace O10.Core.Persistency
{
    public abstract class DataAccessServiceBase<T> : IDataAccessService where T : DataContextBase
    {
        protected readonly IDataLayerConfiguration _configuration;
        private T _dataContext;
        private readonly object _sync = new object();

        protected DataAccessServiceBase(IConfigurationService configurationService, ILoggerService loggerService)
        {
            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _configuration = configurationService.Get<IDataLayerConfiguration>();
            ContextName = GetType().FullName;
            Logger = loggerService.GetLogger(ContextName);
        }

        public bool IsInitialized { get; private set; }

        protected string ContextName { get; }
        //protected T DataContext { get; private set; }

        protected ILogger Logger { get; }

        protected T DataContext 
        { 
            get
            {
                if(_dataContext == null)
                {
                    lock(_sync)
                    {
                        if(_dataContext == null)
                        {
                            Logger.Info($"ConnectionString = {_configuration.ConnectionString}");
                            _dataContext = GetDataContext();
                            _dataContext.Initialize(_configuration.ConnectionString);
                            _dataContext.EnsureConfigurationCompleted();
                        }
                    }
                }
                return _dataContext; 
            } 
        }

        protected CancellationToken CancellationToken { get; private set; }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            if (IsInitialized)
            {
                return;
            }

            CancellationToken = cancellationToken;

            try
            {
                Logger.Info($"{ContextName} Initialize started");

                DataContext.Migrate();

                await PostInitTasks();

                IsInitialized = true;
                Logger.Info($"{ContextName} Initialize completed");
            }
            catch (Exception ex)
            {
                Logger.Error($"{ContextName} Initialize failed", ex);

                throw;
            }
        }

        protected abstract T GetDataContext();

        protected virtual async Task PostInitTasks() => await Task.CompletedTask;
    }
}
