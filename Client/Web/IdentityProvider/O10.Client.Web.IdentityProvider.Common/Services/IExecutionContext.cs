using O10.Client.Web.Common.Services;
using O10.Core.Architecture;

namespace O10.Server.IdentityProvider.Common.Services
{
	[ServiceContract]
	public interface IExecutionContext
	{
		void Initialize(long accountId, byte[] secretKey);

		Persistency GetContext();
	}
}
