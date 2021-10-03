using Microsoft.EntityFrameworkCore;
using O10.Core.Architecture;
using O10.Node.DataLayer.DataAccess;

namespace O10.Node.DataLayer.Specific.Stealth.DataContexts.SQLite
{
    [RegisterExtension(typeof(INodeDataContext), Lifetime = LifetimeManagement.Transient)]
    public class DataContext : StealthDataContextBase
    {
        public override string DataProvider => "SQLite";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString ?? "Filename=core.dat");
            ManualResetEventSlim.Set();
        }
    }
}
