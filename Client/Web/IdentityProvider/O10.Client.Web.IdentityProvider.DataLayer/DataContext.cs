using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.DataLayer;
using O10.IdentityProvider.DataLayer.Model;

namespace O10.IdentityProvider.DataLayer
{
	public abstract class DataContext : DataContextBase
	{
		public DbSet<UserRecord> UserRecords { get; set; }
		public DbSet<BlindingFactorsRecord> BlindingFactorsRecords { get; set; }
		public DbSet<RegistrationSession> RegistrationSessions { get; set; }
		public DbSet<IdentityProviderSettings> IdentityProviderSettings { get; set; }
        public DbSet<BiometricRecord> BiometricRecords { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserRecord>().HasIndex(r => r.IssuanceCommitment).IsUnique();
			modelBuilder.Entity<UserRecord>().HasIndex(r => r.AssetId).IsUnique();
			modelBuilder.Entity<RegistrationSession>().HasIndex(r => r.SessionKey).IsUnique();
			modelBuilder.Entity<RegistrationSession>().HasIndex(r => r.SessionCommitment).IsUnique();
            modelBuilder.Entity<BiometricRecord>().HasIndex(r => r.UserRecordId);

            base.OnModelCreating(modelBuilder);
		}
	}
}
