using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;

namespace O10.Node.Core.DataLayer
{
    [ServiceContract]
    public interface INodesDataService : IDataService<Network.Topology.Node>
    {
    }
}
