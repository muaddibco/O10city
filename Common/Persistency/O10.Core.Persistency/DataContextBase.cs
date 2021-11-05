using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace O10.Core.Persistency
{
    public abstract class DataContextBase : DbContext, IDataContext
    {
        public static readonly LoggerFactory _myLoggerFactory =
            new LoggerFactory(new[] {
                new DebugLoggerProvider()
            });
        private SqlClientListener _sqlClientListener;

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
            //_sqlClientListener = new SqlClientListener();
            ConnectionString = connectionString;
            return this;
        }

        public DataContextBase Migrate()
        {
            Database.Migrate();
            return this;
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (T)await Database.GetDbConnection().ExecuteScalarAsync(sql, param, transaction);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().ExecuteAsync(sql, param, transaction);
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync<T>(sql, param, transaction)).AsList();
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T1, T2, T>(string sql, Func<T1, T2, T> map, string splitOn = null, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync(sql, map, param, transaction, splitOn: splitOn)).AsList();
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T1, T2, T3, T>(string sql, Func<T1, T2, T3, T> map, string splitOn = null, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync(sql, map, param, transaction, splitOn: splitOn)).AsList();
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T1, T2, T3, T4, T>(string sql, Func<T1, T2, T3, T4, T> map, string splitOn = null, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync(sql, map, param, transaction, splitOn: splitOn)).AsList();
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().QueryFirstOrDefaultAsync<T>(sql, param, transaction);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T1, T2, T>(string sql, Func<T1, T2, T> map, string splitOn = null, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync(sql, map, param, transaction, splitOn: splitOn)).FirstOrDefault();
        }

        public async Task<T> QueryFirstOrDefaultAsync<T1, T2, T3, T>(string sql, Func<T1, T2, T3, T> map, string splitOn = null, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync(sql, map, param, transaction, splitOn: splitOn)).FirstOrDefault();
        }

        public async Task<T> QueryFirstOrDefaultAsync<T1, T2, T3, T4, T>(string sql, Func<T1, T2, T3, T4, T> map, string splitOn = null, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return (await Database.GetDbConnection().QueryAsync(sql, map, param, transaction, splitOn: splitOn)).FirstOrDefault();
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            return await Database.GetDbConnection().QuerySingleAsync<T>(sql, param, transaction);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseLoggerFactory(_myLoggerFactory);
        }
    }
}
