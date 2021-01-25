using System.Threading;
using O10.Core.Architecture;

namespace O10.Client.Web.Saml.Common.Services
{
    [ServiceContract]
	public interface ISamlIdentityProvidersManager
	{
		void Initialize(CancellationToken cancellationToken);
		void Start();
		SamlIdpService GetSamlIdpService(string entityId);

		void CreateNewDefaultSamlIdentityProvider();
	}
}
