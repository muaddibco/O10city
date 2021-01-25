using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace O10.Gateway.WebApp.Common.Hubs
{
    public class NotificationsHub : Hub
    {
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName).ConfigureAwait(false);
        }

		public async Task Subscribe(string groupId, string query)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, groupId).ConfigureAwait(false);
		}
    }
}
