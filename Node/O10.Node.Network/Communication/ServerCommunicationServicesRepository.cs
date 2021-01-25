using System.Collections.Generic;
using O10.Network.Exceptions;
using O10.Network.Interfaces;
using O10.Core.Architecture;

namespace O10.Network.Communication
{
    [RegisterDefaultImplementation(typeof(IServerCommunicationServicesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ServerCommunicationServicesRepository : IServerCommunicationServicesRepository
    {
        private readonly Dictionary<string, IServerCommunicationService> _communicationServicesPool;

        public ServerCommunicationServicesRepository(IEnumerable<IServerCommunicationService> communicationServices)
        {
            _communicationServicesPool = new Dictionary<string, IServerCommunicationService>();

            foreach (IServerCommunicationService communicationService in communicationServices)
            {
                if(!_communicationServicesPool.ContainsKey(communicationService.Name))
                {
                    _communicationServicesPool.Add(communicationService.Name, communicationService);
                }
            }
        }

        public IServerCommunicationService GetInstance(string key)
        {
            if(!_communicationServicesPool.ContainsKey(key))
            {
                throw new CommunicationServiceNotSupportedException(key);
            }

            return _communicationServicesPool[key];
        }
    }
}
