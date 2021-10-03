using Microsoft.EntityFrameworkCore;
using O10.Core.Architecture;
using O10.Node.DataLayer.DataAccess;

namespace O10.Node.DataLayer.Specific.Registry.DataContexts.SqlServer
{
    [RegisterExtension(typeof(INodeDataContext), Lifetime = LifetimeManagement.Transient)]
    public class DataContext : RegistryDataContextBase
    {
        public override string DataProvider => "SqlServer";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(optionsBuilder));
            }

            optionsBuilder.UseSqlServer(ConnectionString ?? @"Server=localhost\SQLEXPRESS;Database=core;Trusted_Connection=True;");
            ManualResetEventSlim.Set();
        }
    }
}
