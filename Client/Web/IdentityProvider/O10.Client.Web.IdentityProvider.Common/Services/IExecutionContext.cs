using O10.Client.Web.Common.Services;
using O10.Core.Architecture;
using System.Threading.Tasks;

namespace O10.Server.IdentityProvider.Common.Services
{
	[ServiceContract]
	public interface IExecutionContext
	{
		Task Initialize(long accountId, byte[] secretKey);

		Persistency GetContext();
	}
}
