using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Gateway.WebApp.Common.Services
{
    [ServiceContract]
    interface INotificationsHubService
    {
		ITargetBlock<WitnessPackage> PipeIn { get; }

        void Initialize(CancellationToken cancellationToken);
    }
}
