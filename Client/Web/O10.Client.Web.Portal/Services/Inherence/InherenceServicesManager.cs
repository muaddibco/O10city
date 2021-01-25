using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.Services.Inherence
{
    [RegisterDefaultImplementation(typeof(IInherenceServicesManager), Lifetime = LifetimeManagement.Singleton)]
    public class InherenceServicesManager : IInherenceServicesManager
    {
        private readonly IEnumerable<IInherenceService> _inherenceServices;

        public InherenceServicesManager(IEnumerable<IInherenceService> inherenceServices)
        {
            _inherenceServices = inherenceServices;
        }

        public IEnumerable<IInherenceService> GetAll()
        {
            return _inherenceServices;
        }

        public IInherenceService GetInstance(string key)
        {
            return _inherenceServices?.FirstOrDefault(s => s.Name == key);
        }
    }
}
