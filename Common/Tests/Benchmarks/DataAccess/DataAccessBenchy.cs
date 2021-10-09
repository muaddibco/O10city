using BenchmarkDotNet.Attributes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using O10.Core.ExtensionMethods;
using System.Linq;
using Dapper;
using O10.Crypto.ConfidentialAssets;
using System.Threading.Tasks;

namespace Benchmark.DataAccess
{
    [MemoryDiagnoser]
    [SimpleJob(invocationCount: 5, targetCount: 20)]
    public class DataAccessBenchy
    {
        private DataContextTracking _dataContextNoSave;
        private DataContextTracking _dataContextNoTracking;

        [Params(100, 1000, 5000)]
        public int Length { get; set; }

        private string[] _contents;

        [GlobalSetup]
        public void Setup()
        {
            _dataContextNoSave = new DataContextTracking();
            _dataContextNoSave.ChangeTracker.AutoDetectChangesEnabled = true;
            _dataContextNoSave.Database.Migrate();

            _dataContextNoTracking = new DataContextTracking();
            _dataContextNoTracking.ChangeTracker.AutoDetectChangesEnabled = false;

            _contents = new string[Length];

            for (int i = 0; i < Length; i++)
            {
                _contents[i] = CryptoHelper.GetRandomSeed().ToHexString();
            }
        }

        [Benchmark]
        public async Task<int> EfCoreInsertNoSaveWithTracking()
        {
            for (int i = 0; i < Length; i++)
            {
                _dataContextNoSave.Transactions.Add(new Models.Transaction()
                {
                    Content = _contents[i]
                });
            }

            return await _dataContextNoSave.SaveChangesAsync();
        }

        [Benchmark]
        public async Task<int> EfCoreInsertNoSaveWithNoTracking()
        {
            for (int i = 0; i < Length; i++)
            {
                _dataContextNoTracking.Transactions.Add(new Models.Transaction()
                {
                    Content = _contents[i]
                });
            }

            _dataContextNoTracking.ChangeTracker.DetectChanges();
            return await _dataContextNoTracking.SaveChangesAsync();
        }

        [Benchmark]
        public async Task<int> EfCoreInsertSave()
        {
            using var dbContext = new DataContextTracking();
            int c = 0;
            for (int i = 0; i < Length; i++)
            {
                dbContext.Transactions.Add(new Models.Transaction()
                {
                    Content = _contents[i]
                });

                c += await dbContext.SaveChangesAsync();
            }

            return c;
        }

        [Benchmark]
        public int DapperInsert()
        {
            using var dbConnection = new SqlConnection("Data Source=localhost,1434;Database=benchmark;User ID=sa;Password=p@ssword1;MultipleActiveResultSets=true;");
            int c = 0;
            for (int i = 0; i < Length; i++)
            {
                c += dbConnection.Execute($"INSERT INTO Transactions(Content) VALUES('{_contents[i]}')");
            }

            return c;
        }

        [Benchmark]
        public int DapperInsert2()
        {
            int c = 0;
            for (int i = 0; i < Length; i++)
            {
                SaveDapper(ref c, i);
            }

            return c;
        }

        private void SaveDapper(ref int c, int i)
        {
            using var dbConnection = new SqlConnection("Data Source=localhost,1434;Database=benchmark;User ID=sa;Password=p@ssword1;MultipleActiveResultSets=true;");
            c += dbConnection.Execute($"INSERT INTO Transactions(Content) VALUES('{_contents[i]}')");
        }

        [IterationCleanup]
        public void CleanUp()
        {
            using var dbContext = new DataContextTracking();
            if (dbContext.Transactions.Any())
            {
                dbContext.Transactions.RemoveRange(dbContext.Transactions);
                dbContext.SaveChanges();
            }
        }
    }
}
