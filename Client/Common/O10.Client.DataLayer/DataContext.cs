using Microsoft.EntityFrameworkCore;
using O10.Client.DataLayer.ElectionCommittee;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Model.ConsentManagement;
using O10.Client.DataLayer.Model.Inherence;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Client.DataLayer.Model.ServiceProviders;
using O10.Client.DataLayer.Model.Users;
using O10.Core.DataLayer;

namespace O10.Client.DataLayer
{
    public abstract class DataContext : DataContextBase
	{
		public DbSet<SystemSettings> SystemSettings { get; set; }
		public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountKeyValue> AccountKeyValues { get; set; }
        public DbSet<Identity> Identities { get; set; }
        public DbSet<IdentityTarget> IdentityTargets { get; set; }
        public DbSet<IdentityAttribute> Attributes { get; set; }
		public DbSet<UserRootAttribute> UserRootAttributes { get; set; }
		public DbSet<UserAssociatedAttribute> UserAssociatedAttributes { get; set; }
		public DbSet<SpAttribute> SpAttributes { get; set; }
		public DbSet<ServiceProviderRegistration> ServiceProviderRegistrations { get; set; }
		public DbSet<SpIdenitityValidation> SpIdenitityValidations { get; set; }
		public DbSet<SynchronizationStatus> SynchronizationStatuses { get; set; }
		public DbSet<IdentitiesScheme> IdentitiesSchemes { get; set; }
        public DbSet<BiometricRecord> BiometricRecords { get; set; }
		public DbSet<SpEmployee> SpEmployees { get; set; }
		public DbSet<SpEmployeeGroup> SpEmployeeGroups { get; set; }
        public DbSet<SpDocument> SpDocuments { get; set; }
        public DbSet<SpDocumentAllowedSigner> SpDocumentAllowedSigners { get; set; }
        public DbSet<SpDocumentSignature> SpDocumentSignatures { get; set; }
        public DbSet<UserGroupRelation> UserGroupRelations { get; set; }
		public DbSet<UserRegistration> UserRegistrations { get; set; }
		public DbSet<AutoLogin> AutoLogins { get; set; }
		public DbSet<SamlIdentityProvider> SamlIdentityProviders { get; set; }
        public DbSet<SamlServiceProvider> SamlServiceProviders { get; set; }
        public DbSet<SamlSettings> SamlSettings { get; set; }

        public DbSet<GroupRelation> GroupRelations { get; set; }
		public DbSet<RegistrationCommitment> RegistrationCommitments { get; set; }

		public DbSet<UserSettings> UserSettings { get; set; }
		public DbSet<ConsentManagementSettings> ConsentManagementSettings { get; set; }

		public DbSet<ScenarioSession> ScenarioSessions { get; set; }
		public DbSet<ScenarioAccount> ScenarioAccounts { get; set; }
		public DbSet<SpUserTransaction> SpUserTransactions { get; set; }

		public DbSet<UserIdentityIssuer> UserIdentityIssuers { get; set; }

		public DbSet<ProcessedWitness> ProcessedWitnesses { get; set; }

		public DbSet<InherenceSetting> InherenceSettings { get; set; }

		public DbSet<ExternalIdentityProvider> ExternalIdentityProviders { get; set; }
		public DbSet<AssociatedAttributeBackup> AssociatedAttributeBackups { get; set; }
		public DbSet<UserTransactionSecrets> UserTransactionSecrets { get; set; }

        public DbSet<EcPollRecord> PollRecords { get; set; }
        public DbSet<EcCandidateRecord> CandidateRecords { get; set; }
        public DbSet<EcPollSelection> PollSelections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BiometricRecord>().HasIndex(p => p.UserData).IsUnique();
			modelBuilder.Entity<SpEmployee>().HasIndex(p => p.RegistrationCommitment).IsUnique();
			modelBuilder.Entity<SpDocument>().HasIndex(p => p.DocumentName);
            modelBuilder.Entity<SpDocumentSignature>().HasIndex(p => p.DocumentRecordHeight);
            modelBuilder.Entity<SpDocumentSignature>().HasIndex(p => p.SignatureRecordHeight);
            modelBuilder.Entity<SamlIdentityProvider>().HasIndex(p => p.EntityId);
            modelBuilder.Entity<SamlServiceProvider>().HasIndex(p => p.EntityId);
			modelBuilder.Entity<IdentitiesScheme>().HasIndex(p => p.Issuer);
			modelBuilder.Entity<IdentitiesScheme>().HasIndex(p => p.AttributeName);
            modelBuilder.Entity<GroupRelation>().HasIndex(p => p.Issuer);
            modelBuilder.Entity<GroupRelation>().HasIndex(p => p.AssetId);
			modelBuilder.Entity<RegistrationCommitment>().HasIndex(p => p.Issuer);
			modelBuilder.Entity<RegistrationCommitment>().HasIndex(p => p.AssetId);
			modelBuilder.Entity<ScenarioSession>().HasIndex(p => p.UserSubject);
			modelBuilder.Entity<SpUserTransaction>().HasIndex(p => p.AccountId);
			modelBuilder.Entity<SpUserTransaction>().HasIndex(p => p.TransactionId).IsUnique();
			modelBuilder.Entity<UserGroupRelation>().HasIndex(p => p.Issuer);
			modelBuilder.Entity<UserGroupRelation>().HasIndex(p => p.AssetId);
			modelBuilder.Entity<UserRegistration>().HasIndex(p => p.Issuer);
			modelBuilder.Entity<UserRegistration>().HasIndex(p => p.AssetId);
			modelBuilder.Entity<UserIdentityIssuer>().HasIndex(p => p.Key).IsUnique();
			modelBuilder.Entity<ProcessedWitness>().HasIndex("AccountId", "WitnessId").IsUnique();
			modelBuilder.Entity<InherenceSetting>().HasIndex("Name").IsUnique();
			modelBuilder.Entity<ExternalIdentityProvider>().HasIndex("Name").IsUnique();
			modelBuilder.Entity<AssociatedAttributeBackup>().HasIndex(p => p.RootIssuer);
			modelBuilder.Entity<AssociatedAttributeBackup>().HasIndex(p => p.RootAssetId);
			modelBuilder.Entity<UserTransactionSecrets>().HasIndex(p => p.AccountId);
			modelBuilder.Entity<UserTransactionSecrets>().HasIndex(p => p.KeyImage);
			modelBuilder.Entity<AccountKeyValue>().HasIndex(p => p.AccountId);
			modelBuilder.Entity<AccountKeyValue>().HasIndex(p => p.Key);
			modelBuilder.Entity<EcPollRecord>().HasIndex(p => p.Name);
			modelBuilder.Entity<EcCandidateRecord>().HasIndex(p => p.Name);
			modelBuilder.Entity<EcPollSelection>().HasIndex(p => p.EcCommitment);
			modelBuilder.Entity<IdentityTarget>().HasIndex(p => p.IdentityId);
			base.OnModelCreating(modelBuilder);
        }
    }
}
