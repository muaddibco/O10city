using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Collections.Generic;

namespace O10.Core.Persistency
{
    public abstract class DataContextBase : DbContext, IDataContext
    {
        public IDataContext EnsureConfigurationCompleted()
        {
            ManualResetEventSlim.Wait();
            return this;
        }

        public abstract string DataProvider { get; }

        protected string ConnectionString { get; private set; }

        protected ManualResetEventSlim ManualResetEventSlim { get; } = new ManualResetEventSlim(false);

        public IDataContext Initialize(string connectionString)
        {  
            ConnectionString = connectionString;
            return this;
        }

        public DataContextBase Migrate()
        {
            Database.Migrate();
            return this;
        }

        public async Task<object> ExecuteScalarAsync(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().ExecuteScalarAsync(sql, param, transaction);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().ExecuteAsync(sql, param, transaction);
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync<T>(sql, param, transaction)).AsList();
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().QueryFirstOrDefaultAsync<T>(sql, param, transaction);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().QuerySingleAsync<T>(sql, param, transaction);
        }
    }
}
