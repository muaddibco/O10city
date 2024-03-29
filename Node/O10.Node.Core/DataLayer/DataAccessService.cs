﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Persistency;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Node.Core.DataLayer.DataContexts;
using System.Threading.Tasks;
using System.Threading;

namespace O10.Node.Core.DataLayer
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Scoped)]
    public class DataAccessService : DataAccessServiceBase<InternalDataContextBase>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IDataContextRepository _dataContextRepository;
        private readonly ConcurrentDictionary<IKey, NodeRecord> _keyToNodeMap;

        public DataAccessService(IDataContextRepository dataContextRepository,
                                    IConfigurationService configurationService,
                                    ILoggerService loggerService,
                                    IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
            : base(configurationService, loggerService)
        {
            if (configurationService is null)
            {
                throw new System.ArgumentNullException(nameof(configurationService));
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

        protected override InternalDataContextBase GetDataContext()
        {
            return _dataContextRepository.GetInstance<InternalDataContextBase>(_configuration.ConnectionType);
        }

        protected override async Task PostInitTasks()
        {
            await LoadAllKnownNodeIPs();
            await base.PostInitTasks();
        }

        public async Task<bool> RemoveNodeByIp(IPAddress ipAddress)
        {
            if (ipAddress is null)
            {
                throw new System.ArgumentNullException(nameof(ipAddress));
            }

            string sql = "DELETE FROM NodeRecords WHERE IPAddress=@IPAddress";

            var res = await DataContext.ExecuteAsync(sql, new { IPAddress = ipAddress.ToString() }, cancellationToken: CancellationToken);

            return res > 0;
        }

        public async Task<bool> AddNode(IKey key, byte nodeRole, IPAddress ipAddress)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (ipAddress is null)
            {
                throw new System.ArgumentNullException(nameof(ipAddress));
            }

            string sql = "IF NOT EXISTS (SELECT * FROM NodeRecords WHERE PublicKey=@PublicKey AND NodeRole=@NodeRole)\r\n";
            sql += "BEGIN\r\n";
            sql += "INSERT INTO NodeRecords(PublicKey, IPAddress, NodeRole) VALUES (@PublicKey, @IPAddress, @NodeRole)\r\n";
            sql += "END\r\n";

            var node = new NodeRecord { PublicKey = key.ToString(), IPAddress = ipAddress.ToString(), NodeRole = nodeRole };
            var res = await DataContext.ExecuteAsync(sql, node, cancellationToken: CancellationToken);

            if (res > 0)
            {
                _keyToNodeMap.AddOrUpdate(key, node, (_, __) => node);
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateNode(IKey key, string ipAddressExpression = null)
        {
            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out IPAddress ipAddress))
            {
                return await UpdateNode(key, ipAddress);
            }

            return false;
        }

        public async Task<bool> UpdateNode(IKey key, IPAddress ipAddress)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (ipAddress == null)
            {
                throw new System.ArgumentNullException(nameof(ipAddress));
            }
            
            if(_keyToNodeMap.TryGetValue(key, out var node))
            {
                node.IPAddress = ipAddress.ToString();

                string sql = "UPDATE NodeRecords SET IPAddress=@IPAddress WHERE PublicKey=@PublicKey";

                await DataContext.ExecuteAsync(sql, node);

                return true;
            }

            return false;
        }

        private async Task LoadAllKnownNodeIPs()
        {
            _keyToNodeMap.Clear();

            string sql = "SELECT * FROM NodeRecords";
            var nodeRecords = await DataContext.QueryAsync<NodeRecord>(sql, cancellationToken: CancellationToken);
            foreach (var node in nodeRecords)
            {
                IKey key = _identityKeyProvider.GetKey(node.PublicKey.HexStringToByteArray());
                if (!_keyToNodeMap.ContainsKey(key))
                {
                    _keyToNodeMap.AddOrUpdate(key, node, (_, __) => node);
                }
            }
        }

        public async Task<IEnumerable<NodeRecord>> GetAllNodes(CancellationToken cancellationToken)
        {

            string sql = "SELECT * FROM NodeRecords";
            return await DataContext.QueryAsync<NodeRecord>(sql, cancellationToken: cancellationToken);
        }

        public NodeRecord GetNode(IKey key)
        {
            if (_keyToNodeMap.ContainsKey(key))
            {
                return _keyToNodeMap[key];
            }

            return null;
        }

        public async Task<IEnumerable<Gateway>> GetGateways(CancellationToken cancellationToken)
        {
            string sql = "SELECT * FROM gateways";
            return await DataContext.QueryAsync<Gateway>(sql, cancellationToken: cancellationToken);
        }

        public async Task<Gateway> RemoveGateway(long gatewayId)
        {
            string sql = "DELETE FROM gateways OUTPUT DELETED.* WHERE GatewayId=@GatewayId";
            return await DataContext.QueryFirstOrDefaultAsync<Gateway>(sql, new { GatewayId = gatewayId });
        }

        public async Task<bool> AddGateway(string alias, string uri, CancellationToken cancellationToken)
        {
            using var dbContext = GetDataContext();

            string sql = "BEGIN TRANSACTION;\r\n" +
                "UPDATE Gateways WITH (UPDLOCK, SERIALIZABLE) SET Alias = @Alias WHERE BaseUri = @BaseUri;\r\n" +
                "IF @@ROWCOUNT = 0\r\n" +
                "BEGIN\r\n" +
                "   INSERT Gateways(BaseUri, Alias) VALUES(@BaseUri, @Alias);\r\n" +
                "END\r\n" +
                "COMMIT TRANSACTION;";

            var gateway = new Gateway
            {
                Alias = alias,
                BaseUri = uri
            };

            return await DataContext.ExecuteAsync(sql, gateway, cancellationToken: cancellationToken) > 0;
        }
    }
}
