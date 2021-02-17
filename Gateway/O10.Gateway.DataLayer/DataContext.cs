using Microsoft.EntityFrameworkCore;
using O10.Gateway.DataLayer.Model;
using O10.Core.DataLayer;

namespace O10.Gateway.DataLayer
{
	public abstract class DataContext : DataContextBase
	{
		public DbSet<SyncBlock> SyncBlocks { get; set; }
        public DbSet<RegistryCombinedBlock> RegistryCombinedBlocks { get; set; }
        public DbSet<RegistryFullBlockData> RegistryFullBlocks { get; set; }
        public DbSet<PacketHash> PacketHashes { get; set; }
        public DbSet<Address> Identities { get; set; }
        public DbSet<StatePacket> TransactionalPackets { get; set; }
        public DbSet<StealthPacket> StealthPackets { get; set; }
        public DbSet<KeyImage> UtxoKeyImages { get; set; }
        public DbSet<StealthOutput> UtxoOutputs { get; set; }
        public DbSet<TransactionKey> UtxoTransactionKeys { get; set; }
        public DbSet<RootAttribute> RootAttributeIssuances { get; set; }
        public DbSet<AssociatedAttributeIssuance> AssociatedAttributeIssuances { get; set; }
        public DbSet<WitnessPacket> WitnessPackets { get; set; }
		public DbSet<RelationRecord> EmployeeRecords { get; set; }

        public DbSet<CompromisedKeyImage> CompromisedKeyImages { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<AssociatedAttributeIssuance>().HasIndex(s => s.IssuanceCommitment);
			modelBuilder.Entity<AssociatedAttributeIssuance>().HasIndex(s => s.RootIssuanceCommitment);

			modelBuilder.Entity<PacketHash>().HasIndex(s => new { s.SyncBlockHeight, s.CombinedRegistryBlockHeight, s.Hash });

			modelBuilder.Entity<RelationRecord>().HasIndex(s => s.Issuer);
			modelBuilder.Entity<RelationRecord>().HasIndex(s => s.RegistrationCommitment);
			modelBuilder.Entity<RelationRecord>().HasIndex(s => s.IsRevoked);

			modelBuilder.Entity<RootAttribute>().HasIndex(s => s.IssuanceCommitment);
			modelBuilder.Entity<RootAttribute>().HasIndex(s => s.RootCommitment);
			modelBuilder.Entity<RootAttribute>().HasIndex(s => s.IsOverriden);

			modelBuilder.Entity<KeyImage>().HasIndex(s => s.Value);

			modelBuilder.Entity<StealthOutput>().HasIndex(s => s.IsOverriden);
			modelBuilder.Entity<StealthOutput>().HasIndex(s => s.DestinationKey);
			modelBuilder.Entity<StealthOutput>().HasIndex(s => s.Commitment);

			modelBuilder.Entity<TransactionKey>().HasIndex(s => s.Key);

			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.CombinedBlockHeight);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedDestinationKey);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedDestinationKey2);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedTransactionKey);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedKeyImage);

			modelBuilder.Entity<RegistryFullBlockData>().HasIndex(s => s.CombinedBlockHeight);
            modelBuilder.Entity<StatePacket>().HasIndex(s => s.WitnessId);
            modelBuilder.Entity<StatePacket>().HasIndex(s => s.Height);
			modelBuilder.Entity<StealthPacket>().HasIndex(s => s.WitnessId);
            modelBuilder.Entity<RootAttribute>().HasIndex(s => s.IsOverriden);
            modelBuilder.Entity<Address>().HasIndex(s => s.Key);
            modelBuilder.Entity<CompromisedKeyImage>().HasIndex(s => s.KeyImage).IsUnique();
		}
	}
}
