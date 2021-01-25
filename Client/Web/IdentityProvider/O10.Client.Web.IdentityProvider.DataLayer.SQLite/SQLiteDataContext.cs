using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;


namespace O10.IdentityProvider.DataLayer.SQLite
{
	[RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Singleton)]
	public class SQLiteDataContext : DataContext
	{
		public override string DataProvider => "IdpSQLite";
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(ConnectionString ?? "Filename=idp.dat");
		}
	}
}
