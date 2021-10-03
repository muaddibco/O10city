using Microsoft.EntityFrameworkCore;
using O10.Core.Persistency;

namespace O10.Node.Core.DataLayer.DataContexts
{
    public abstract class InternalDataContextBase : DataContextBase
	{
        public DbSet<NodeRecord> NodeRecords { get; set; }

        public DbSet<Gateway> Gateways { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<NodeRecord>().HasIndex("PublicKey", "NodeRole").IsUnique();
			modelBuilder.Entity<Gateway>().HasIndex(g => g.BaseUri).IsUnique();
        }
	}
}
