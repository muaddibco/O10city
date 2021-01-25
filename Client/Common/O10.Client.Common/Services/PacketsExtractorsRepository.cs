using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IPacketsExtractorsRepository), Lifetime = LifetimeManagement.Scoped)]
    public class PacketsExtractorsRepository : IPacketsExtractorsRepository
    {
        private readonly IEnumerable<IPacketsExtractor> _packetsExtractors;

        public PacketsExtractorsRepository(IEnumerable<IPacketsExtractor> packetsExtractors)
        {
            _packetsExtractors = packetsExtractors;
        }

        public IPacketsExtractor GetInstance(string key)
        {
            return _packetsExtractors.FirstOrDefault(s => s.Name == key);
        }
    }
}
