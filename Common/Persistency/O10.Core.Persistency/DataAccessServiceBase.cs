using O10.Core.Configuration;
using O10.Core.Logging;
using System;
using O10.Core.Persistency.Configuration;

namespace O10.Core.Persistency
{
    public abstract class DataAccessServiceBase<T> : IDataAccessService where T : DataContextBase
	{
        protected readonly IDataLayerConfiguration _configuration;

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

		protected T DataContext { get; private set; }

        public void Initialize()
		{
			if (IsInitialized)
            {
                return;
            }

			try
			{
				Logger.Info($"{ContextName} Initialize started");

				Logger.Info($"ConnectionString = {_configuration.ConnectionString}");

				DataContext = GetDataContext();
				DataContext.Initialize(_configuration.ConnectionString);
				DataContext.Migrate();
				DataContext.EnsureConfigurationCompleted();

				PostInitTasks();

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

		protected virtual void PostInitTasks() { }
	}
}
