using System;
using O10.Core.Architecture;
using System.Net;

namespace O10.Network.Interfaces
{
    [ExtensionPoint]
    public interface IServerCommunicationService : ICommunicationService
    {
        void InitCommunicationProvisioning(ICommunicationProvisioning communicationProvisioning = null);

        void RegisterOnReceivedExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceiveExtendedValidation);
    }
}
