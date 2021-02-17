using O10.Core.Configuration;
using O10.Core.Logging;
using System;
using O10.Transactions.Core.Enums;
using O10.Core.DataLayer;
using System.Timers;
using O10.Core.Tracking;

namespace O10.Node.DataLayer.DataAccess
{
    public abstract class NodeDataAccessServiceBase<T> : DataAccessServiceBase<T>, INodeDataAccessService where T : NodeDataContextBase
	{
		private readonly INodeDataContextRepository _dataContextRepository;
		private readonly Timer _timer;

		private bool _isSaving;

		protected NodeDataAccessServiceBase(INodeDataContextRepository dataContextRepository,
                                      IConfigurationService configurationService,
									  ITrackingService trackingService,
									  ILoggerService loggerService)
			: base(configurationService, trackingService, loggerService)
		{
            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _dataContextRepository = dataContextRepository ?? throw new ArgumentNullException(nameof(dataContextRepository));
			_timer = new Timer(500);
			_timer.Elapsed += _timer_Elapsed;
		}

		public abstract LedgerType PacketType { get; }

		protected override T GetDataContext(string connectionType) => (T)_dataContextRepository.GetInstance(PacketType, connectionType);

        protected override void PostInitTasks()
        {
			DataContext.ChangeTracker.StateChanged += ChangeTracker_StateChanged;
			_timer.Start();

			base.PostInitTasks();
        }

        #region Private Functions

        private void ChangeTracker_StateChanged(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e)
		{
			Logger.LogIfDebug(() => $"State of {e.Entry.Entity.GetType().Name} changed {e.OldState} -> {e.NewState}");
		}

		private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			TrackingService.TrackEvent($"{ContextName} Saving");

			if (_isSaving)
				return;

			lock (Sync)
			{
				if (_isSaving)
					return;

				_isSaving = true;

				try
				{
					TrackingService.TrackEvent($"{ContextName} Save");
					int writtenEntities = DataContext.SaveChanges();
					TrackingService.TrackMetric($"{ContextName} Save", writtenEntities);
				}
				catch (Exception ex)
				{
					TrackingService.TrackException(ex);
					Logger.Error("Failure during saving data to database", ex);
				}
				finally
				{
					_isSaving = false;
				}
			}
		}

		#endregion Private Functions
	}
}
