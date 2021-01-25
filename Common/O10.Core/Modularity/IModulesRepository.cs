using O10.Core.Architecture;

namespace O10.Core.Modularity
{
    [ServiceContract]
    public interface IModulesRepository : IRepository<IModule, string>, IBulkRegistry<IModule>
    {
    }
}
