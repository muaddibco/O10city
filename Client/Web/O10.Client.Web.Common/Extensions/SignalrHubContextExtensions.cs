using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using O10.Core.Logging;
using O10.Core.Serialization;
using System;
using System.Threading.Tasks;

namespace O10.Client.Web.Common.Extensions
{
    public static class SignalrHubContextExtensions
    {
        public static async Task SendMessageAsync<THub>(this IHubContext<THub> hubContext, ILogger? logger, string target, string method, object payload = null) where THub : Hub
        {
            if (hubContext is null)
            {
                throw new ArgumentNullException(nameof(hubContext));
            }

            logger?.LogIfDebug(() => $"SignalR sending to {target} method {method}...\r\nPayload: { (payload != null ? JsonConvert.SerializeObject(payload, new ByteArrayJsonConverter(), new MemoryByteJsonConverter(), new KeyJsonConverter()) : "NULL")}");

            if (payload == null)
            {
                await hubContext.Clients.Group(target).SendAsync(method).ConfigureAwait(false);
            }
            else
            {
                await hubContext.Clients.Group(target).SendAsync(method, payload).ConfigureAwait(false);
            }
        }
    }
}
