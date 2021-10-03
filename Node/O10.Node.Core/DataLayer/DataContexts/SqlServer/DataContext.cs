using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Node.Core.DataLayer.DataContexts.SqlServer
{
    [RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Transient)]
    public class DataContext : InternalDataContextBase
    {
        public override string DataProvider => "SqlServer";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString ?? @"Server=localhost\SQLEXPRESS;Database=core;Trusted_Connection=True;");
            ManualResetEventSlim.Set();
        }
    }
}
