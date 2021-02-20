using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.DataLayer;
using O10.Node.DataLayer.Exceptions;

namespace O10.Node.DataLayer.DataAccess
{
    [RegisterDefaultImplementation(typeof(INodeDataAccessServiceRepository), Lifetime = LifetimeManagement.Singleton)]
    public class NodeDataAccessServiceRepository : INodeDataAccessServiceRepository
    {
        private readonly IEnumerable<INodeDataAccessService> _dataAccessServices;

        public NodeDataAccessServiceRepository(IEnumerable<IDataAccessService> dataAccessServices)
        {
            _dataAccessServices = dataAccessServices.OfType<INodeDataAccessService>();
        }

        public INodeDataAccessService GetInstance(LedgerType key)
        {
            var dataAccessService = _dataAccessServices.FirstOrDefault(s => s.LedgerType == key);

            if(dataAccessService == null)
            {
                throw new DataAccessServiceNotFoundException(key);
            }

            return dataAccessService;
        }
    }
}
