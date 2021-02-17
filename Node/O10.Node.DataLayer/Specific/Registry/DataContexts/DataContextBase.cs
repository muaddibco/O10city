using Microsoft.EntityFrameworkCore;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Registry.Model;

namespace O10.Node.DataLayer.Specific.Registry.DataContexts
{
    public abstract class RegistryDataContextBase : NodeDataContextBase
	{
        public override LedgerType PacketType => LedgerType.Registry;

        public DbSet<RegistryFullBlock> RegistryFullBlocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RegistryFullBlock>().HasIndex(a => a.SyncBlockHeight);
        }
    }
}
