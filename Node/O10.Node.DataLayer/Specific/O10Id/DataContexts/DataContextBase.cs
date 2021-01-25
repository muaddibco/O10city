using Microsoft.EntityFrameworkCore;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.O10Id.Model;

namespace O10.Node.DataLayer.Specific.O10Id.DataContexts
{
    public abstract class O10IdDataContextBase : NodeDataContextBase
	{
        public override PacketType PacketType => PacketType.Transactional;

        public DbSet<AccountIdentity> AccountIdentities { get; set; }

        public DbSet<O10TransactionHashKey> BlockHashKeys { get; set; }

        public DbSet<O10Transaction> TransactionalBlocks { get; set; }

        public DbSet<O10TransactionIdentity> TransactionalIdentities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountIdentity>().HasIndex(a => a.KeyHash);
            modelBuilder.Entity<O10TransactionHashKey>().HasIndex(a => a.SyncBlockHeight);
            modelBuilder.Entity<O10TransactionHashKey>().HasIndex(a => a.Hash);
            modelBuilder.Entity<O10Transaction>().HasIndex(a => a.SyncBlockHeight);
            modelBuilder.Entity<O10Transaction>().HasIndex(a => a.BlockHeight);
        }
    }
}
