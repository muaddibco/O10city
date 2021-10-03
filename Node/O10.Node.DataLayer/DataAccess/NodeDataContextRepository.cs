using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.Exceptions;
using O10.Core.Architecture;
using System;

namespace O10.Node.DataLayer.DataAccess
{
    [RegisterDefaultImplementation(typeof(INodeDataContextRepository), Lifetime = LifetimeManagement.Scoped)]
    public class NodeDataContextRepository : INodeDataContextRepository
    {
        private readonly IEnumerable<INodeDataContext> _nodeDataContexts;

        public NodeDataContextRepository(IEnumerable<INodeDataContext> nodeDataContexts)
        {
            _nodeDataContexts = nodeDataContexts;
        }

        public INodeDataContext GetInstance(LedgerType ledgerType, string dataProvider)
        {
            var dctx = _nodeDataContexts.FirstOrDefault(d => d.LedgerType == ledgerType && d.DataProvider == dataProvider);

            if(dctx == null)
            {
                throw new NodeDataContextNotFoundException(ledgerType, dataProvider);
            }

            return (INodeDataContext)Activator.CreateInstance(dctx.GetType());
        }
    }
}
