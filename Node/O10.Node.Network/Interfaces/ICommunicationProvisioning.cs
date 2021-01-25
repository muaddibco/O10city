using System;
using System.Net;
using O10.Core.Architecture;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface ICommunicationProvisioning
    {
        IPEndPoint[] AllowedEndpoints { get; }

        event EventHandler AllowedEndpointsChanged;
    }
}
