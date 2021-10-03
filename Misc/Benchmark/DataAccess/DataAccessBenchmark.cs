using BenchmarkDotNet.Attributes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using Dapper;

namespace Benchmark.DataAccess
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class DataAccessBenchmark
    {
        private DataContextTracking _dataContextNoSave;
        private DataContextTracking _dataContextNoTracking;

        [GlobalSetup]
        public void Setup()
        {
            _dataContextNoSave = new DataContextTracking();
            _dataContextNoSave.ChangeTracker.AutoDetectChangesEnabled = true;
            _dataContextNoSave.Database.Migrate();

            _dataContextNoTracking = new DataContextTracking();
            _dataContextNoTracking.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        [Benchmark]
        public void EfCoreInsertNoSaveWithTracking()
        {
            _dataContextNoSave.Transactions.Add(new Models.Transaction()
            {
                Content = "AAA"
            });
        }

        [Benchmark]
        public void EfCoreInsertNoSaveWithNoTracking()
        {
            _dataContextNoTracking.Transactions.Add(new Models.Transaction()
            {
                Content = "AAA"
            });
        }

        [Benchmark]
        public int EfCoreInsertSave()
        {
            using var dbContext = new DataContextTracking();
            dbContext.Transactions.Add(new Models.Transaction()
            {
                Content = "AAA"
            });

            return dbContext.SaveChanges();
        }

        [Benchmark]
        public int EfCoreInsertSave1000()
        {
            using var dbContext = new DataContextTracking();
            for (int i = 0; i < 1000; i++)
            {
                dbContext.Transactions.Add(new Models.Transaction()
                {
                    Content = "AAA"
                });
            }

            return dbContext.SaveChanges();
        }

        [Benchmark]
        public int DapperInsert()
        {
            using var dbConnection = new SqlConnection("Data Source=localhost,1434;Database=benchmark;User ID=sa;Password=p@ssword1;MultipleActiveResultSets=true;");
            return dbConnection.Execute("INSERT INTO Transactions(Content) VALUES('AAA')");
        }

        [Benchmark(OperationsPerInvoke = 1000)]
        public int DapperInsert1000()
        {
            using var dbConnection = new SqlConnection("Data Source=localhost,1434;Database=benchmark;User ID=sa;Password=p@ssword1;MultipleActiveResultSets=true;");
            return dbConnection.Execute("INSERT INTO Transactions(Content) VALUES('AAA')");
        }

        [Benchmark]
        public void DapperInsert1000_2()
        {
            using var dbConnection = new SqlConnection("Data Source=localhost,1434;Database=benchmark;User ID=sa;Password=p@ssword1;MultipleActiveResultSets=true;");
            for (int i = 0; i < 1000; i++)
            {
                dbConnection.Execute("INSERT INTO Transactions(Content) VALUES('AAA')");
            }
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
