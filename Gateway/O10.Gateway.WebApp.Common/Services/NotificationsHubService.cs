using O10.Core.Architecture;
using System.Threading.Tasks.Dataflow;
using System.Threading;
using O10.Core.Models;
using O10.Core.Tracking;
using System.Collections.Generic;
using System.Globalization;
using O10.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using O10.Core.Serialization;

namespace O10.Gateway.WebApp.Common.Services

{
    [RegisterDefaultImplementation(typeof(INotificationsHubService), Lifetime = LifetimeManagement.Singleton)]
    public class NotificationsHubService : INotificationsHubService
    {
        private readonly IEnumerable<INotificationService> _notificationServices;
        private readonly ITrackingService _trackingService;
        private readonly ILogger _logger;
		private readonly object _sync = new object();
        private bool _isInitialized;

		public NotificationsHubService(IEnumerable<INotificationService> notificationServices, ITrackingService trackingService, ILoggerService loggerService)
        {
            _notificationServices = notificationServices;
            _trackingService = trackingService;
            _logger = loggerService.GetLogger(nameof(NotificationsHubService));

            if(notificationServices == null || !notificationServices.Any())
            {
                _logger.Error("No notification channels obtained");
            }
            else
            {
                _logger.Info($"Gateway notification channels: {string.Join(',', notificationServices?.Select(s => s.GetType().Name))}");
            }
        }

		public ITargetBlock<WitnessPackage> PipeIn { get; private set; }

        public void Initialize(CancellationToken cancellationToken)
        {
            if(_isInitialized)
            {
                return;
            }

            lock(_sync)
            {
                if(_isInitialized)
                {
                    return;
                }

                _isInitialized = true;
            }

            PipeIn = new ActionBlock<WitnessPackage>(p =>
            {
                _logger.Info($"[G2C]: Sending {p.Witnesses?.Count()?? 0} Witnesses");

                foreach (var item in p.Witnesses)
                {
                    _logger.LogIfDebug(() => $"[G2C]: PacketsUpdate TransactionalWitness {JsonConvert.SerializeObject(item, new ByteArrayJsonConverter())}");
                }

                _trackingService.TrackEvent($"{nameof(NotificationsHubService)}_PacketsUpdate", new Dictionary<string, string> { { nameof(p.CombinedBlockHeight), p.CombinedBlockHeight.ToString(CultureInfo.InvariantCulture) } });

                foreach (var notificationService in _notificationServices)
                {
                    try
                    {
                        notificationService.Send(p);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Notification through {notificationService.GetType().Name} failed", ex);
                    }
                }
            }, new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken });
        }
    }
}
