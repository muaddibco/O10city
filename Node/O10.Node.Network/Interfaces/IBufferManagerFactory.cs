﻿using O10.Core;
using O10.Core.Architecture;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface IBufferManagerFactory : IFactory<IBufferManager>
    {
    }
}
