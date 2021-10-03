using Microsoft.EntityFrameworkCore;
using O10.Core.Architecture;
using O10.Node.DataLayer.DataAccess;

namespace O10.Node.DataLayer.Specific.O10Id.DataContexts.SQLite
{
    [RegisterExtension(typeof(INodeDataContext), Lifetime = LifetimeManagement.Transient)]
    public class DataContext : O10IdDataContextBase
    {
        public override string DataProvider => "SQLite";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString ?? "Filename=core.dat");
            ManualResetEventSlim.Set();
        }
    }
}
