﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using System;
using O10.Node.Network.Topology;
using O10.Core.Persistency;
using System.Threading.Tasks;
using O10.Node.DataLayer.DataServices;

namespace O10.Node.Core.DataLayer
{

    [RegisterDefaultImplementation(typeof(INodesDataService), Lifetime = LifetimeManagement.Scoped)]
    public class NodesDataService : INodesDataService
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly DataAccessService _dataAccessService;
        private CancellationToken _cancellationToken;

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

        public async Task<DataResult<NodeEntity>> Add(NodeEntity item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            await _dataAccessService.AddNode(item.Key, (byte)item.NodeRole, item.IPAddress);

            return new DataResult<NodeEntity>(null, item);
        }

        public async Task AddDataKey(IDataKey key, IDataKey newKey, CancellationToken cancellation)
        {
        }

        public async Task<IEnumerable<NodeEntity>> Get(IDataKey key, CancellationToken cancellationToken)
        {
            if (key == null)
            {
                return (await _dataAccessService.GetAllNodes(cancellationToken))
                    .Select(n =>
                    new NodeEntity
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
                    return new List<NodeEntity> { new NodeEntity { Key = uniqueKey.IdentityKey, IPAddress = IPAddress.Parse(node.IPAddress) } };
                }
            }

            throw new ArgumentOutOfRangeException(nameof(key));
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }
    }
}
