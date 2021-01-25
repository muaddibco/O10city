using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IUpdaterRegistry), Lifetime = LifetimeManagement.Scoped)]
    public class UpdaterRegistry : IUpdaterRegistry
    {
        private IUpdater _updater;

        public IUpdater GetInstance()
        {
            return _updater;
        }

        public void RegisterInstance(IUpdater obj)
        {
            _updater = obj;
        }
    }
}
