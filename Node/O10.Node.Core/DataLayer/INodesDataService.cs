using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Node.Network.Topology;

namespace O10.Node.Core.DataLayer
{
    [ServiceContract]
    public interface INodesDataService : IDataService<NodeEntity>
    {
    }
}
