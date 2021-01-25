using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.DataLayer.SQLite
{
    [RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Singleton)]
	public class SQLiteDataContext : DataContext
	{
		public override string DataProvider => "SQLite";
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(ConnectionString ?? "Filename=client.dat");
		}
	}
}
