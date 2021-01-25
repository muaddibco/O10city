using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Node.Core.Centralized
{
	[ServiceContract]
	public interface INotificationsService
	{
		void Initialize(CancellationToken cancellationToken);

		void UpdateGateways();

		Task GatewaysConnectivityCheck(InfoMessage message);
	}
}
