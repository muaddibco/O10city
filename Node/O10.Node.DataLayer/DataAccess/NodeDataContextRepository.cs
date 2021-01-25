using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.Exceptions;
using O10.Core.Architecture;

namespace O10.Node.DataLayer.DataAccess
{
    [RegisterDefaultImplementation(typeof(INodeDataContextRepository), Lifetime = LifetimeManagement.Singleton)]
    public class NodeDataContextRepository : INodeDataContextRepository
    {
        private readonly IEnumerable<INodeDataContext> _nodeDataContexts;

        public NodeDataContextRepository(IEnumerable<INodeDataContext> nodeDataContexts)
        {
            _nodeDataContexts = nodeDataContexts;
        }

        public INodeDataContext GetInstance(PacketType packetType, string dataProvider)
        {
            var dctx = _nodeDataContexts.FirstOrDefault(d => d.PacketType == packetType && d.DataProvider == dataProvider);

            if(dctx == null)
            {
                throw new NodeDataContextNotFoundException(packetType, dataProvider);
            }

            return dctx;
        }
    }
}
