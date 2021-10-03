using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;


namespace O10.IdentityProvider.DataLayer.SqlServer
{
	[RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Transient)]
	public class SqlServerDataContext : DataContext
	{
		public override string DataProvider => "IdpSqlServer";

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(ConnectionString ?? @"Server=localhost\SQLEXPRESS;Database=idp;Trusted_Connection=True;");
		}
	}
}
