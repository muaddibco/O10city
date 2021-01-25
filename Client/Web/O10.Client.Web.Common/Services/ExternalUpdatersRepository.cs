using O10.Core.Architecture;

using System.Collections.Generic;

namespace O10.Client.Web.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExternalUpdatersRepository), Lifetime = LifetimeManagement.Scoped)]
    public class ExternalUpdatersRepository : IExternalUpdatersRepository
    {
        private readonly IEnumerable<IExternalUpdater> _updaters;

        public ExternalUpdatersRepository(IEnumerable<IExternalUpdater> updaters)
        {
            _updaters = updaters;
        }

        public IEnumerable<IExternalUpdater> GetInstances()
        {
            return _updaters;
        }
    }
}
