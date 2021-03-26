using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Identity;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Topology
{
    [RegisterDefaultImplementation(typeof(INodesResolutionService), Lifetime = LifetimeManagement.Singleton)]
    public class NodesResolutionService : INodesResolutionService
    {
        private readonly ConcurrentDictionary<IKey, NodeAddress> _nodeAddresses;
        private readonly ConcurrentDictionary<IKey, NodeAddress> _storageLayerNodeAddresses;

        public NodesResolutionService()
        {
            _nodeAddresses = new ConcurrentDictionary<IKey, NodeAddress>();
            _storageLayerNodeAddresses = new ConcurrentDictionary<IKey, NodeAddress>();
        }

        public IPAddress ResolveNodeAddress(IKey key)
        {
			if (_nodeAddresses.ContainsKey(key))
			{
				return _nodeAddresses[key].IP;
			}

            return IPAddress.None;
        }

        public void AddNode(NodeAddress nodeAddress, NodeRole nodeRole)
        {
            if (nodeAddress == null)
            {
                throw new System.ArgumentNullException(nameof(nodeAddress));
            }

            if (nodeRole == NodeRole.StorageLayer)
            {
                _storageLayerNodeAddresses.AddOrUpdate(nodeAddress.Key, nodeAddress, (k, v) => v);
            }

            _nodeAddresses.AddOrUpdate(nodeAddress.Key, nodeAddress, (k, v) => nodeAddress);
        }

        public IEnumerable<IKey> GetStorageNodeKeys(IPacketBase packetBase)
        {
            //TODO: need to understand logic of distribution of transactions between storage nodes
            //IKey key = transactionKeyProvider.GetKey();

            //TODO: implement logic of recognizing storage nodes basing on MurMur Hash value of transaction content
            return _storageLayerNodeAddresses.Keys;
        }
    }
}
