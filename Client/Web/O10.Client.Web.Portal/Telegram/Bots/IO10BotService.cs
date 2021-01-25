using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;

namespace O10.Client.Web.Portal.Telegram.Bots
{
    [ServiceContract]
    public interface IO10BotService
    {
        Task Initialize(CancellationToken ct);

        void Start();
    }
}
