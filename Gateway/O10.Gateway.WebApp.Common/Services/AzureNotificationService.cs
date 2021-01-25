using Microsoft.Azure.NotificationHubs;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Tracking;

namespace O10.Gateway.WebApp.Common.Services
{
    [RegisterExtension(typeof(INotificationService), Lifetime = LifetimeManagement.Singleton)]
    public class AzureNotificationService : INotificationService
    {
        private readonly NotificationHubClient _hub;
        private readonly ITrackingService _trackingService;

        public AzureNotificationService(ITrackingService trackingService)
        {
            _hub = NotificationHubClient.CreateClientFromConnectionString(DispatcherConstants.FullAccessConnectionString, DispatcherConstants.NotificationHubName);
            _trackingService = trackingService;
        }

        public async Task Send(WitnessPackage witnessPackage)
        {
            _trackingService.TrackEvent(nameof(AzureNotificationService), new Dictionary<string, string> { { nameof(witnessPackage.CombinedBlockHeight), witnessPackage.CombinedBlockHeight.ToString(CultureInfo.InvariantCulture) } });

            FcmNotification fcmNotification = new FcmNotification("{\"data\":{\"message\":\"CombinedBlockHeight = " + witnessPackage.CombinedBlockHeight + "\"}}");
            await _hub.SendNotificationAsync(fcmNotification).ConfigureAwait(false);
        }
    }
}
