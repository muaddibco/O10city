using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using System;
using O10.Network.Topology;
using O10.Core.DataLayer;
using O10.Core.Models;
using O10.Core.Notifications;

namespace O10.Node.Core.DataLayer
{

    [RegisterDefaultImplementation(typeof(INodesDataService), Lifetime = LifetimeManagement.Singleton)]
    public class NodesDataService : INodesDataService
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly DataAccessService _dataAccessService;

        public NodesDataService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IDataAccessServiceRepository dataAccessServiceRepository)
        {
            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            if (dataAccessServiceRepository is null)
            {
                throw new ArgumentNullException(nameof(dataAccessServiceRepository));
            }

            _dataAccessService = dataAccessServiceRepository.GetInstance<DataAccessService>();
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public TaskCompletionWrapper<Network.Topology.Node> Add(Network.Topology.Node item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _dataAccessService.AddNode(item.Key, (byte)item.NodeRole, item.IPAddress);

            var wrapper = new TaskCompletionWrapper<Network.Topology.Node>(item);
            wrapper.TaskCompletion.SetResult(new SucceededNotification());

            return wrapper;
        }

        public void AddDataKey(IDataKey key, IDataKey newKey)
        {
        }

        public IEnumerable<Network.Topology.Node> Get(IDataKey key)
        {
            if (key == null)
            {
                return _dataAccessService
                    .GetAllNodes()
                    .Select(n =>
                    new Network.Topology.Node
                    {
                        Key = _identityKeyProvider.GetKey(n.PublicKey.HexStringToByteArray()),
                        IPAddress = IPAddress.Parse(n.IPAddress),
                        NodeRole = (NodeRole)n.NodeRole
                    });
            }

            if (key is UniqueKey uniqueKey)
            {
                DataContexts.NodeRecord node = _dataAccessService.GetNode(uniqueKey.IdentityKey);

                if (node != null)
                {
                    return new List<Network.Topology.Node> { new Network.Topology.Node { Key = uniqueKey.IdentityKey, IPAddress = IPAddress.Parse(node.IPAddress) } };
                }
            }

            throw new ArgumentOutOfRangeException(nameof(key));
        }

        public void Initialize(CancellationToken cancellationToken)
        {
        }
    }
}
