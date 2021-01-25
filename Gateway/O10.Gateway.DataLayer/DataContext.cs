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
        public DbSet<TransactionalPacket> TransactionalPackets { get; set; }
        public DbSet<StealthPacket> StealthPackets { get; set; }
        public DbSet<UtxoKeyImage> UtxoKeyImages { get; set; }
        public DbSet<UtxoOutput> UtxoOutputs { get; set; }
        public DbSet<UtxoTransactionKey> UtxoTransactionKeys { get; set; }
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

			modelBuilder.Entity<UtxoKeyImage>().HasIndex(s => s.KeyImage);

			modelBuilder.Entity<UtxoOutput>().HasIndex(s => s.IsOverriden);
			modelBuilder.Entity<UtxoOutput>().HasIndex(s => s.DestinationKey);
			modelBuilder.Entity<UtxoOutput>().HasIndex(s => s.Commitment);

			modelBuilder.Entity<UtxoTransactionKey>().HasIndex(s => s.Key);

			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.CombinedBlockHeight);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedDestinationKey);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedDestinationKey2);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedTransactionKey);
			modelBuilder.Entity<WitnessPacket>().HasIndex(s => s.ReferencedKeyImage);

			modelBuilder.Entity<RegistryFullBlockData>().HasIndex(s => s.CombinedBlockHeight);
            modelBuilder.Entity<TransactionalPacket>().HasIndex(s => s.WitnessId);
            modelBuilder.Entity<StealthPacket>().HasIndex(s => s.WitnessId);
            modelBuilder.Entity<RootAttribute>().HasIndex(s => s.IsOverriden);
            modelBuilder.Entity<Address>().HasIndex(s => s.Key);
            modelBuilder.Entity<CompromisedKeyImage>().HasIndex(s => s.KeyImage).IsUnique();
		}
	}
}
