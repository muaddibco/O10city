using Dapper;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Persistency.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Core.Persistency
{
    [RegisterDefaultImplementation(typeof(IApplicationDbConnection), Lifetime = LifetimeManagement.Scoped)]
    public class ApplicationDbConnection : IApplicationDbConnection
    {
        private readonly IDbConnection _connection;

        public ApplicationDbConnection(IDbConnectionProvidersRepository dbConnectionProvidersRepository, IConfigurationService configurationService)
        {
            var provider = dbConnectionProvidersRepository.GetInstance(configurationService.Get<IDataLayerConfiguration>().ConnectionType);
            _connection = provider.GetDbConnection();
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await _connection.ExecuteAsync(sql, param, transaction);
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await _connection.QueryAsync<T>(sql, param, transaction)).AsList();
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await _connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await _connection.QuerySingleAsync<T>(sql, param, transaction);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
