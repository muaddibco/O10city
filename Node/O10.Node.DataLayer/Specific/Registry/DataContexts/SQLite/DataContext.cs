using Microsoft.EntityFrameworkCore;
using O10.Core.Architecture;
using O10.Node.DataLayer.DataAccess;

namespace O10.Node.DataLayer.Specific.Registry.DataContexts.SQLite
{
    [RegisterExtension(typeof(INodeDataContext), Lifetime = LifetimeManagement.Singleton)]
    public class DataContext : RegistryDataContextBase
    {
        public override string DataProvider => "SQLite";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(optionsBuilder));
            }

            optionsBuilder.UseSqlite(ConnectionString ?? "Filename=core.dat");
            ManualResetEventSlim.Set();
        }
    }
}
