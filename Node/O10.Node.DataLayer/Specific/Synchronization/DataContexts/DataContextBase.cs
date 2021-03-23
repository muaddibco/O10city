using Microsoft.EntityFrameworkCore;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Synchronization.Model;

namespace O10.Node.DataLayer.Specific.Synchronization.DataContexts
{
    public abstract class SynchronizationDataContextBase : NodeDataContextBase
	{
        public override LedgerType LedgerType => LedgerType.Synchronization;

        public DbSet<SynchronizationPacket> SynchronizationBlocks { get; set; }

        public DbSet<AggregatedRegistrationsTransaction> RegistryCombinedBlocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<AggregatedRegistrationsTransaction>().HasIndex(a => a.SyncBlockHeight);
        }
	}
}
