using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Tracking;
using O10.Gateway.WebApp.Common.Hubs;

namespace O10.Gateway.WebApp.Common.Services
{
    [RegisterExtension(typeof(INotificationService), Lifetime = LifetimeManagement.Singleton)]
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationsHub> _notificationsHubContext;
        private readonly ITrackingService _trackingService;

        public SignalRNotificationService(IHubContext<NotificationsHub> notificationsHubContext, ITrackingService trackingService)
        {
            _notificationsHubContext = notificationsHubContext;
            _trackingService = trackingService;
        }
        
        public async Task Send(WitnessPackage witnessPackage)
        {
            _trackingService.TrackEvent(nameof(SignalRNotificationService), new Dictionary<string, string> { { nameof(witnessPackage.CombinedBlockHeight), witnessPackage.CombinedBlockHeight.ToString(CultureInfo.InvariantCulture) } });

            await _notificationsHubContext.Clients.All.SendAsync("PacketsUpdate", witnessPackage).ConfigureAwait(false);
        }
    }
}
