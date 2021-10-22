using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;


namespace O10.IdentityProvider.DataLayer.SQLite
{
	[RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Transient)]
	public class SQLiteDataContext : DataContext
	{
		public override string DataProvider => "IdpSQLite";
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.UseSqlite(ConnectionString ?? "Filename=idp.dat");
		}
	}
}
