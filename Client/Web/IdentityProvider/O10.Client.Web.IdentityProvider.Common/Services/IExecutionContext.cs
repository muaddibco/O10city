using O10.Client.Common.Services.ExecutionScope;
using O10.Core.Architecture;
using System.Threading.Tasks;

namespace O10.Server.IdentityProvider.Common.Services
{
    [ServiceContract]
	public interface IExecutionContext
	{
		Task Initialize(long accountId, byte[] secretKey);

		ScopePersistency GetContext();
	}
}
