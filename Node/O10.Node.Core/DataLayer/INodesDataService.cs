using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Network.Topology;

namespace O10.Node.Core.DataLayer
{
    [ServiceContract]
    public interface INodesDataService : IDataService<NodeEntity>
    {
    }
}
