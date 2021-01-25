using System.Collections.Generic;
using System.Net;
using O10.Transactions.Core.Serializers;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Identity;

namespace O10.Network.Topology
{
    [ServiceContract]
    public interface INodesResolutionService
    {
        void AddNode(NodeAddress nodeAddress, NodeRole nodeRole);

        IPAddress ResolveNodeAddress(IKey key);

        IEnumerable<IKey> GetStorageNodeKeys(ITransactionKeyProvider transactionKeyProvider);
    }
}
