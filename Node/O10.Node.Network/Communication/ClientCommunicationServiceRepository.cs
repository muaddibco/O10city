using System.Collections.Generic;
using O10.Network.Exceptions;
using O10.Network.Interfaces;
using O10.Core.Architecture;

namespace O10.Network.Communication
{

    [RegisterDefaultImplementation(typeof(IClientCommunicationServiceRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ClientCommunicationServiceRepository : IClientCommunicationServiceRepository
    {
        private readonly Dictionary<string, ICommunicationService> _communicationServices;

        public ClientCommunicationServiceRepository(IEnumerable<ICommunicationService> communicationServices)
        {
            _communicationServices = new Dictionary<string, ICommunicationService>();

            foreach (ICommunicationService communicationService in communicationServices)
            {
                if(!_communicationServices.ContainsKey(communicationService.Name))
                {
                    _communicationServices.Add(communicationService.Name, communicationService);
                }
            }
        }

        public ICommunicationService GetInstance(string key)
        {
            if(!_communicationServices.ContainsKey(key))
            {
                throw new CommunicationServiceNotSupportedException(key);
            }

            return _communicationServices[key];
        }
    }

}
