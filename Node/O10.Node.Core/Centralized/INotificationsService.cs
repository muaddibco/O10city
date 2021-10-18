using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Node.Core.Centralized
{
	[ServiceContract]
	public interface INotificationsService
	{
		Task Initialize(CancellationToken cancellationToken);

		Task UpdateGateways(CancellationToken cancellationToken);

		Task GatewaysConnectivityCheck(InfoMessage message);
	}
}
