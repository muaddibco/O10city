﻿using O10.Core;
using O10.Core.Architecture;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface IClientCommunicationServiceRepository : IRepository<ICommunicationService, string>
    {
    }
}
