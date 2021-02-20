using Microsoft.EntityFrameworkCore;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Stealth.Model;

namespace O10.Node.DataLayer.Specific.Stealth.DataContexts
{
    public abstract class StealthDataContextBase : NodeDataContextBase
	{
        public override LedgerType LedgerType => LedgerType.Stealth;

        public DbSet<StealthTransactionHashKey> BlockHashKeys { get; set; }

        public DbSet<KeyImage> StealthKeyImages { get; set; }

        public DbSet<StealthTransaction> StealthBlocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StealthTransactionHashKey>().HasIndex(a => a.SyncBlockHeight);
            modelBuilder.Entity<StealthTransactionHashKey>().HasIndex(a => a.Hash);
            modelBuilder.Entity<StealthTransaction>().HasIndex(a => a.SyncBlockHeight);
            modelBuilder.Entity<KeyImage>().HasIndex(a => a.Value);
        }
    }
}
