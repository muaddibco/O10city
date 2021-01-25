using O10.Core.Configuration;
using O10.Core.Logging;
using System;
using Microsoft.EntityFrameworkCore;
using O10.Core.Tracking;
using O10.Core.DataLayer.Configuration;

namespace O10.Core.DataLayer
{
    public abstract class DataAccessServiceBase<T> : IDataAccessService where T : DataContextBase
	{
        private readonly IDataLayerConfiguration _configuration;

        protected DataAccessServiceBase(IConfigurationService configurationService, ITrackingService trackingService, ILoggerService loggerService)
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
			TrackingService = trackingService;
			ContextName = GetType().FullName;
			Logger = loggerService.GetLogger(ContextName);
		}

		public bool IsInitialized { get; private set; }

        protected static object Sync { get; } = new object();

        protected string ContextName { get; }
        protected T DataContext { get; private set; }

        protected ITrackingService TrackingService { get; }

        protected ILogger Logger { get; }

        public void Initialize()
		{
			if (IsInitialized)
            {
                return;
            }

            try
			{
				Logger.Info($"{ContextName} Initialize started");

				DataContext = GetDataContext(_configuration.ConnectionType);
				DataContext.Initialize(_configuration.ConnectionString);
				DataContext.Database.Migrate();
				Logger.Info($"ConnectionString = {DataContext.Database.GetDbConnection().ConnectionString}");
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

		protected abstract T GetDataContext(string connectionType);

		protected virtual void PostInitTasks() { }
	}
}
