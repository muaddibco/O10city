using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.DataLayer.SqlServer
{
	[RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Singleton)]
	public class SqlServerDataContext : DataContext
	{
		public override string DataProvider => "SqlServer";

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(ConnectionString ?? @"Server=localhost\SQLEXPRESS;Database=client;Trusted_Connection=True;");
		}
	}
}
