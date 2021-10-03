using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Gateway.DataLayer.SQLite
{
	[RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Transient)]
	public class SQLiteDataContext : DataContext
	{
		public override string DataProvider => "SQLite";
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(ConnectionString ?? "Filename=gateway.dat");
		}
	}
}
