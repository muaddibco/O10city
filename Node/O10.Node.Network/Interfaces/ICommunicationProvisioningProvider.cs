using System.Net;
using O10.Core.Architecture;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface ICommunicationProvisioningProvider
    {
        void UpdateAllowedEndpoints(IPEndPoint[] allowedEndpoints);
    }
}
