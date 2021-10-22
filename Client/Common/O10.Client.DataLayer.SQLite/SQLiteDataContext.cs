using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.DataLayer.SQLite
{
    [RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Transient)]
	public class SQLiteDataContext : DataContext
	{
		public override string DataProvider => "SQLite";
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.UseSqlite(ConnectionString ?? "Filename=client.dat");
		}
	}
}
