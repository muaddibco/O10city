using Benchmark.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.DataAccess
{
    public class DataContextTracking : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .UseSqlServer("Data Source=localhost,1434;Database=benchmark;User ID=sa;Password=p@ssword1;MultipleActiveResultSets=true;");
        }
    }
}
