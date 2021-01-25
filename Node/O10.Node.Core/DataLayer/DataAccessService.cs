using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.DataLayer;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Node.Core.DataLayer.DataContexts;

namespace O10.Node.Core.DataLayer
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : DataAccessServiceBase<InternalDataContextBase>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IDataContextRepository _dataContextRepository;
        private readonly ConcurrentDictionary<IKey, NodeRecord> _keyToNodeMap;

        public DataAccessService(IDataContextRepository dataContextRepository,
                                    IConfigurationService configurationService,
                                    ITrackingService trackingService,
                                    ILoggerService loggerService,
                                    IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
            : base(configurationService, trackingService, loggerService)
        {
            if (configurationService is null)
            {
                throw new System.ArgumentNullException(nameof(configurationService));
            }

            if (trackingService is null)
            {
                throw new System.ArgumentNullException(nameof(trackingService));
            }

            if (loggerService is null)
            {
                throw new System.ArgumentNullException(nameof(loggerService));
            }

            if (identityKeyProvidersRegistry is null)
            {
                throw new System.ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _dataContextRepository = dataContextRepository ?? throw new System.ArgumentNullException(nameof(dataContextRepository));
            _keyToNodeMap = new ConcurrentDictionary<IKey, NodeRecord>(new KeyEqualityComparer());
        }

        protected override DataContexts.InternalDataContextBase GetDataContext(string connectionType)
        {
            return _dataContextRepository.GetInstance<InternalDataContextBase>(connectionType);
        }

        protected override void PostInitTasks()
        {
            LoadAllKnownNodeIPs();
            base.PostInitTasks();
        }

        public bool AddNode(IKey key, byte nodeRole, string ipAddressExpression = null)
        {
            if (key is null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrEmpty(ipAddressExpression))
            {
                throw new System.ArgumentException($"'{nameof(ipAddressExpression)}' cannot be null or empty", nameof(ipAddressExpression));
            }

            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out IPAddress ipAddress))
            {
                return AddNode(key, nodeRole, ipAddress);
            }

            return false;
        }

        public bool RemoveNodeByIp(IPAddress ipAddress)
        {
            if (ipAddress is null)
            {
                throw new System.ArgumentNullException(nameof(ipAddress));
            }

            string addr = ipAddress.ToString();

            lock (Sync)
            {
                List<NodeRecord> nodeRecords = DataContext.NodeRecords.Where(n => n.IPAddress == addr).ToList();

                foreach (var item in nodeRecords)
                {
                    DataContext.NodeRecords.Remove(item);
                }

                DataContext.SaveChanges();

                return nodeRecords.Count > 0;
            }
        }

        public bool AddNode(IKey key, byte nodeRole, IPAddress ipAddress)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (ipAddress is null)
            {
                throw new System.ArgumentNullException(nameof(ipAddress));
            }

            string publicKey = key.ToString();

            lock (Sync)
            {
                NodeRecord node = DataContext.NodeRecords.FirstOrDefault(n => n.PublicKey == key.ToString() && n.NodeRole == nodeRole);

                if (node == null)
                {
                    node = new NodeRecord { PublicKey = publicKey, IPAddress = ipAddress.ToString(), NodeRole = nodeRole };
                    DataContext.NodeRecords.Add(node);
                    _keyToNodeMap.AddOrUpdate(key, node, (_, __) => node);
                    return true;
                }
            }

            return false;
        }

        public bool UpdateNode(IKey key, string ipAddressExpression = null)
        {
            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out IPAddress ipAddress))
            {
                return UpdateNode(key, ipAddress);
            }

            return false;
        }

        public bool UpdateNode(IKey key, IPAddress ipAddress)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (ipAddress == null)
            {
                throw new System.ArgumentNullException(nameof(ipAddress));
            }

            lock (Sync)
            {
                foreach (var node in DataContext.NodeRecords.Where(n => n.PublicKey == key.ToString()))
                {
                    node.IPAddress = ipAddress.ToString();
                    _keyToNodeMap.AddOrUpdate(key, node, (_, __) => node);
                    DataContext.Update(node);
                }
            }

            return false;
        }

        public void LoadAllKnownNodeIPs()
        {
            lock (Sync)
            {
                _keyToNodeMap.Clear();
                foreach (var node in DataContext.NodeRecords)
                {
                    IKey key = _identityKeyProvider.GetKey(node.PublicKey.HexStringToByteArray());
                    if (!_keyToNodeMap.ContainsKey(key))
                    {
                        _keyToNodeMap.AddOrUpdate(key, node, (_, __) => node);
                    }
                }
            }
        }

        public IPAddress GetNodeIpAddress(IKey key)
        {
            if (_keyToNodeMap.ContainsKey(key))
            {
                return IPAddress.Parse(_keyToNodeMap[key].IPAddress);
            }

            return IPAddress.None;
        }

        public IEnumerable<NodeRecord> GetAllNodes()
        {
            lock (Sync)
            {
                return DataContext.NodeRecords.ToList();
            }
        }

        public NodeRecord GetNode(IKey key)
        {
            if (_keyToNodeMap.ContainsKey(key))
            {
                return _keyToNodeMap[key];
            }

            return null;
        }

        public IEnumerable<Gateway> GetGateways()
        {
            lock(Sync)
            {
                return DataContext.Gateways;
            }
        }

        public Gateway RemoveGateway(long gatewayId)
        {
            lock(Sync)
            {
                var gateway = DataContext.Gateways.FirstOrDefault(gateway => gateway.GatewayId == gatewayId);
                if(gateway != null)
                {
                    DataContext.Gateways.Remove(gateway);
                }

                return gateway;
            }
        }

        public bool AddGateway(string alias, string uri)
        {
            lock(Sync)
            {
                Gateway gateway = DataContext.Gateways.FirstOrDefault(g => g.BaseUri == uri);

                if(gateway != null)
                {
                    if (gateway.Alias != alias)
                    {
                        gateway.Alias = alias;

                        DataContext.SaveChanges();
                        return true;
                    }
                }
                else
                {
                    gateway = new Gateway
                    {
                        Alias = alias,
                        BaseUri = uri
                    };

                    DataContext.Gateways.Add(gateway);
                    DataContext.SaveChanges();
                    return true;
                }
            }

            return false;
        }
    }
}
