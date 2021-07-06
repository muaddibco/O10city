using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Client.DataLayer.SQLite.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountKeyValues",
                columns: table => new
                {
                    AccountKeyValueId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    Key = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountKeyValues", x => x.AccountKeyValueId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecretViewKey = table.Column<byte[]>(nullable: true),
                    PublicViewKey = table.Column<byte[]>(nullable: true),
                    SecretSpendKey = table.Column<byte[]>(nullable: false),
                    PublicSpendKey = table.Column<byte[]>(nullable: false),
                    AccountType = table.Column<byte>(nullable: false),
                    AccountInfo = table.Column<string>(nullable: false),
                    IsCompromised = table.Column<bool>(nullable: false),
                    LastAggregatedRegistrations = table.Column<long>(nullable: false),
                    IsPrivate = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "AssociatedAttributeBackups",
                columns: table => new
                {
                    AssociatedAttributeBackupId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootIssuer = table.Column<string>(nullable: false),
                    RootAssetId = table.Column<string>(nullable: false),
                    AssociatedIssuer = table.Column<string>(nullable: false),
                    SchemeName = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociatedAttributeBackups", x => x.AssociatedAttributeBackupId);
                });

            migrationBuilder.CreateTable(
                name: "BiometricRecords",
                columns: table => new
                {
                    BiometricRecordId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserData = table.Column<string>(nullable: false),
                    PersonGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricRecords", x => x.BiometricRecordId);
                });

            migrationBuilder.CreateTable(
                name: "ConsentManagementSettings",
                columns: table => new
                {
                    ConsentManagementSettingsId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentManagementSettings", x => x.ConsentManagementSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "EcPollRecords",
                columns: table => new
                {
                    EcPollRecordId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcPollRecords", x => x.EcPollRecordId);
                });

            migrationBuilder.CreateTable(
                name: "ExternalIdentityProviders",
                columns: table => new
                {
                    ExternalIdentityProviderId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Alias = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIdentityProviders", x => x.ExternalIdentityProviderId);
                });

            migrationBuilder.CreateTable(
                name: "GroupRelations",
                columns: table => new
                {
                    GroupRelationId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupOwnerKey = table.Column<string>(nullable: false),
                    GroupName = table.Column<string>(nullable: false),
                    AssetId = table.Column<string>(nullable: false),
                    Issuer = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRelations", x => x.GroupRelationId);
                });

            migrationBuilder.CreateTable(
                name: "Identities",
                columns: table => new
                {
                    IdentityId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identities", x => x.IdentityId);
                });

            migrationBuilder.CreateTable(
                name: "IdentitiesSchemes",
                columns: table => new
                {
                    IdentitiesSchemeId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttributeName = table.Column<string>(nullable: false),
                    AttributeSchemeName = table.Column<string>(nullable: false),
                    Alias = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CanBeRoot = table.Column<bool>(nullable: false),
                    Issuer = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentitiesSchemes", x => x.IdentitiesSchemeId);
                });

            migrationBuilder.CreateTable(
                name: "IdentityTargets",
                columns: table => new
                {
                    IdentityTargetId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityId = table.Column<long>(nullable: false),
                    PublicSpendKey = table.Column<string>(nullable: false),
                    PublicViewKey = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityTargets", x => x.IdentityTargetId);
                });

            migrationBuilder.CreateTable(
                name: "InherenceSettings",
                columns: table => new
                {
                    InherenceSettingId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InherenceSettings", x => x.InherenceSettingId);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedWitnesss",
                columns: table => new
                {
                    ProcessedWitnessId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    WitnessId = table.Column<long>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedWitnesss", x => x.ProcessedWitnessId);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationCommitments",
                columns: table => new
                {
                    RegistrationCommitmentId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Commitment = table.Column<string>(nullable: false),
                    ServiceProviderInfo = table.Column<string>(nullable: false),
                    AssetId = table.Column<string>(nullable: false),
                    Issuer = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationCommitments", x => x.RegistrationCommitmentId);
                });

            migrationBuilder.CreateTable(
                name: "SamlIdentityProviders",
                columns: table => new
                {
                    SamlIdentityProviderId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityId = table.Column<string>(nullable: false),
                    SecretViewKey = table.Column<string>(nullable: false),
                    PublicSpendKey = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlIdentityProviders", x => x.SamlIdentityProviderId);
                });

            migrationBuilder.CreateTable(
                name: "SamlServiceProviders",
                columns: table => new
                {
                    SamlServiceProviderId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityId = table.Column<string>(nullable: false),
                    SingleLogoutUrl = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlServiceProviders", x => x.SamlServiceProviderId);
                });

            migrationBuilder.CreateTable(
                name: "SamlSettings",
                columns: table => new
                {
                    SamlSettingsId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DefaultSamlIdpId = table.Column<long>(nullable: false),
                    DefaultSamlIdpAccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlSettings", x => x.SamlSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioSessions",
                columns: table => new
                {
                    ScenarioSessionId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserSubject = table.Column<string>(nullable: false),
                    ScenarioId = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    CurrentStep = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioSessions", x => x.ScenarioSessionId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderRegistrations",
                columns: table => new
                {
                    ServiceProviderRegistrationId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    Commitment = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderRegistrations", x => x.ServiceProviderRegistrationId);
                });

            migrationBuilder.CreateTable(
                name: "SpAttributes",
                columns: table => new
                {
                    SpAttributeId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    AttributeSchemeName = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    AssetId = table.Column<byte[]>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    OriginalBlindingFactor = table.Column<byte[]>(nullable: false),
                    OriginalCommitment = table.Column<byte[]>(nullable: false),
                    IssuingCommitment = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpAttributes", x => x.SpAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "SpIdenitityValidations",
                columns: table => new
                {
                    SpIdenitityValidationId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    SchemeName = table.Column<string>(nullable: false),
                    ValidationType = table.Column<ushort>(nullable: false),
                    NumericCriterion = table.Column<ushort>(nullable: true),
                    GroupIdCriterion = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpIdenitityValidations", x => x.SpIdenitityValidationId);
                });

            migrationBuilder.CreateTable(
                name: "SpUserTransactions",
                columns: table => new
                {
                    SpUserTransactionId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    ServiceProviderRegistrationId = table.Column<long>(nullable: false),
                    TransactionId = table.Column<string>(nullable: false),
                    TransactionDescription = table.Column<string>(nullable: false),
                    IsProcessed = table.Column<bool>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    IsCompromised = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpUserTransactions", x => x.SpUserTransactionId);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SystemSettingsId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InitializationVector = table.Column<byte[]>(nullable: false),
                    BiometricSecretKey = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SystemSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "UserAssociatedAttributes",
                columns: table => new
                {
                    UserAssociatedAttributeId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    AttributeSchemeName = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: true),
                    LastUpdateTime = table.Column<DateTime>(nullable: true),
                    RootAssetId = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssociatedAttributes", x => x.UserAssociatedAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "UserIdentityIssuers",
                columns: table => new
                {
                    UserIdentityIssuerId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: false),
                    Alias = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdentityIssuers", x => x.UserIdentityIssuerId);
                });

            migrationBuilder.CreateTable(
                name: "UserRootAttributes",
                columns: table => new
                {
                    UserAttributeId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    SchemeName = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    AssetId = table.Column<byte[]>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    AnchoringOriginationCommitment = table.Column<byte[]>(nullable: false),
                    OriginalBlindingFactor = table.Column<byte[]>(nullable: false),
                    IssuanceTransactionKey = table.Column<byte[]>(nullable: false),
                    IssuanceCommitment = table.Column<byte[]>(nullable: false),
                    LastBlindingFactor = table.Column<byte[]>(nullable: false),
                    LastCommitment = table.Column<byte[]>(nullable: false),
                    LastTransactionKey = table.Column<byte[]>(nullable: false),
                    LastDestinationKey = table.Column<byte[]>(nullable: false),
                    NextKeyImage = table.Column<string>(nullable: false),
                    IsOverriden = table.Column<bool>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: true),
                    ConfirmationTime = table.Column<DateTime>(nullable: true),
                    LastUpdateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRootAttributes", x => x.UserAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "UserTransactionSecrets",
                columns: table => new
                {
                    UserTransactionSecretId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    KeyImage = table.Column<string>(nullable: false),
                    Issuer = table.Column<string>(nullable: false),
                    AssetId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTransactionSecrets", x => x.UserTransactionSecretId);
                });

            migrationBuilder.CreateTable(
                name: "AutoLogins",
                columns: table => new
                {
                    AutoLoginId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecretKey = table.Column<byte[]>(nullable: false),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoLogins", x => x.AutoLoginId);
                    table.ForeignKey(
                        name: "FK_AutoLogins_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationGroups",
                columns: table => new
                {
                    RelationGroupId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    GroupName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationGroups", x => x.RelationGroupId);
                    table.ForeignKey(
                        name: "FK_RelationGroups_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpDocuments",
                columns: table => new
                {
                    SpDocumentId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    DocumentName = table.Column<string>(nullable: false),
                    Hash = table.Column<string>(nullable: false),
                    LastChangeRecordHeight = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpDocuments", x => x.SpDocumentId);
                    table.ForeignKey(
                        name: "FK_SpDocuments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SynchronizationStatuses",
                columns: table => new
                {
                    SynchronizationStatusId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: false),
                    LastUpdatedCombinedBlockHeight = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynchronizationStatuses", x => x.SynchronizationStatusId);
                    table.ForeignKey(
                        name: "FK_SynchronizationStatuses_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupRelations",
                columns: table => new
                {
                    UserGroupRelationId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    GroupOwnerName = table.Column<string>(nullable: false),
                    GroupOwnerKey = table.Column<string>(nullable: false),
                    GroupName = table.Column<string>(nullable: false),
                    AssetId = table.Column<string>(nullable: false),
                    Issuer = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupRelations", x => x.UserGroupRelationId);
                    table.ForeignKey(
                        name: "FK_UserGroupRelations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRegistrations",
                columns: table => new
                {
                    UserRegistrationId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    Commitment = table.Column<string>(nullable: false),
                    ServiceProviderInfo = table.Column<string>(nullable: false),
                    AssetId = table.Column<string>(nullable: false),
                    Issuer = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegistrations", x => x.UserRegistrationId);
                    table.ForeignKey(
                        name: "FK_UserRegistrations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserSettingsId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    IsAutoTheftProtection = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserSettingsId);
                    table.ForeignKey(
                        name: "FK_UserSettings_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EcCandidateRecords",
                columns: table => new
                {
                    EcCandidateRecordId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    AssetId = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    EcPollRecordId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcCandidateRecords", x => x.EcCandidateRecordId);
                    table.ForeignKey(
                        name: "FK_EcCandidateRecords_EcPollRecords_EcPollRecordId",
                        column: x => x.EcPollRecordId,
                        principalTable: "EcPollRecords",
                        principalColumn: "EcPollRecordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EcPollSelections",
                columns: table => new
                {
                    EcPollSelectionId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EcPollRecordId = table.Column<long>(nullable: false),
                    EcCommitment = table.Column<string>(nullable: false),
                    EcBlindingFactor = table.Column<string>(nullable: false),
                    VoterBlindingFactor = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcPollSelections", x => x.EcPollSelectionId);
                    table.ForeignKey(
                        name: "FK_EcPollSelections_EcPollRecords_EcPollRecordId",
                        column: x => x.EcPollRecordId,
                        principalTable: "EcPollRecords",
                        principalColumn: "EcPollRecordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityAttributes",
                columns: table => new
                {
                    AttributeId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityId = table.Column<long>(nullable: true),
                    AttributeName = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Subject = table.Column<int>(nullable: false),
                    Commitment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityAttributes", x => x.AttributeId);
                    table.ForeignKey(
                        name: "FK_IdentityAttributes_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "IdentityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioAccounts",
                columns: table => new
                {
                    ScenarioAccountId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScenarioSessionId = table.Column<long>(nullable: true),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioAccounts", x => x.ScenarioAccountId);
                    table.ForeignKey(
                        name: "FK_ScenarioAccounts_ScenarioSessions_ScenarioSessionId",
                        column: x => x.ScenarioSessionId,
                        principalTable: "ScenarioSessions",
                        principalColumn: "ScenarioSessionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelationRecords",
                columns: table => new
                {
                    RelationRecordId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    Description = table.Column<string>(nullable: false),
                    RootAttributeValue = table.Column<string>(nullable: false),
                    RegistrationCommitmentId = table.Column<long>(nullable: true),
                    RelationGroupId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationRecords", x => x.RelationRecordId);
                    table.ForeignKey(
                        name: "FK_RelationRecords_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelationRecords_RegistrationCommitments_RegistrationCommitmentId",
                        column: x => x.RegistrationCommitmentId,
                        principalTable: "RegistrationCommitments",
                        principalColumn: "RegistrationCommitmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelationRecords_RelationGroups_RelationGroupId",
                        column: x => x.RelationGroupId,
                        principalTable: "RelationGroups",
                        principalColumn: "RelationGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpDocumentAllowedSigners",
                columns: table => new
                {
                    SpDocumentAllowedSignerId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    DocumentSpDocumentId = table.Column<long>(nullable: true),
                    GroupIssuer = table.Column<string>(nullable: false),
                    GroupName = table.Column<string>(nullable: false),
                    GroupCommitment = table.Column<string>(nullable: false),
                    BlindingFactor = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpDocumentAllowedSigners", x => x.SpDocumentAllowedSignerId);
                    table.ForeignKey(
                        name: "FK_SpDocumentAllowedSigners_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpDocumentAllowedSigners_SpDocuments_DocumentSpDocumentId",
                        column: x => x.DocumentSpDocumentId,
                        principalTable: "SpDocuments",
                        principalColumn: "SpDocumentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpDocumentSignatures",
                columns: table => new
                {
                    SpDocumentSignatureId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentSpDocumentId = table.Column<long>(nullable: true),
                    SignatureRecordHeight = table.Column<ulong>(nullable: false),
                    DocumentRecordHeight = table.Column<ulong>(nullable: false),
                    DocumentSignRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpDocumentSignatures", x => x.SpDocumentSignatureId);
                    table.ForeignKey(
                        name: "FK_SpDocumentSignatures_SpDocuments_DocumentSpDocumentId",
                        column: x => x.DocumentSpDocumentId,
                        principalTable: "SpDocuments",
                        principalColumn: "SpDocumentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountKeyValues_AccountId",
                table: "AccountKeyValues",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountKeyValues_Key",
                table: "AccountKeyValues",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_AssociatedAttributeBackups_RootAssetId",
                table: "AssociatedAttributeBackups",
                column: "RootAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssociatedAttributeBackups_RootIssuer",
                table: "AssociatedAttributeBackups",
                column: "RootIssuer");

            migrationBuilder.CreateIndex(
                name: "IX_AutoLogins_AccountId",
                table: "AutoLogins",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricRecords_UserData",
                table: "BiometricRecords",
                column: "UserData",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcCandidateRecords_EcPollRecordId",
                table: "EcCandidateRecords",
                column: "EcPollRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EcCandidateRecords_Name",
                table: "EcCandidateRecords",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EcPollRecords_Name",
                table: "EcPollRecords",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EcPollSelections_EcCommitment",
                table: "EcPollSelections",
                column: "EcCommitment");

            migrationBuilder.CreateIndex(
                name: "IX_EcPollSelections_EcPollRecordId",
                table: "EcPollSelections",
                column: "EcPollRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIdentityProviders_Name",
                table: "ExternalIdentityProviders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupRelations_AssetId",
                table: "GroupRelations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRelations_Issuer",
                table: "GroupRelations",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_IdentitiesSchemes_AttributeName",
                table: "IdentitiesSchemes",
                column: "AttributeName");

            migrationBuilder.CreateIndex(
                name: "IX_IdentitiesSchemes_Issuer",
                table: "IdentitiesSchemes",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityAttributes_Commitment",
                table: "IdentityAttributes",
                column: "Commitment");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityAttributes_IdentityId",
                table: "IdentityAttributes",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityTargets_IdentityId",
                table: "IdentityTargets",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_InherenceSettings_Name",
                table: "InherenceSettings",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWitnesss_AccountId_WitnessId",
                table: "ProcessedWitnesss",
                columns: new[] { "AccountId", "WitnessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCommitments_AssetId",
                table: "RegistrationCommitments",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCommitments_Commitment",
                table: "RegistrationCommitments",
                column: "Commitment",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCommitments_Issuer",
                table: "RegistrationCommitments",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_RelationGroups_AccountId",
                table: "RelationGroups",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationRecords_AccountId",
                table: "RelationRecords",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationRecords_RegistrationCommitmentId",
                table: "RelationRecords",
                column: "RegistrationCommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationRecords_RelationGroupId",
                table: "RelationRecords",
                column: "RelationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlIdentityProviders_EntityId",
                table: "SamlIdentityProviders",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlServiceProviders_EntityId",
                table: "SamlServiceProviders",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioAccounts_ScenarioSessionId",
                table: "ScenarioAccounts",
                column: "ScenarioSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioSessions_UserSubject",
                table: "ScenarioSessions",
                column: "UserSubject");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocumentAllowedSigners_AccountId",
                table: "SpDocumentAllowedSigners",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocumentAllowedSigners_DocumentSpDocumentId",
                table: "SpDocumentAllowedSigners",
                column: "DocumentSpDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocuments_AccountId",
                table: "SpDocuments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocuments_DocumentName",
                table: "SpDocuments",
                column: "DocumentName");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocumentSignatures_DocumentRecordHeight",
                table: "SpDocumentSignatures",
                column: "DocumentRecordHeight");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocumentSignatures_DocumentSpDocumentId",
                table: "SpDocumentSignatures",
                column: "DocumentSpDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SpDocumentSignatures_SignatureRecordHeight",
                table: "SpDocumentSignatures",
                column: "SignatureRecordHeight");

            migrationBuilder.CreateIndex(
                name: "IX_SpUserTransactions_AccountId",
                table: "SpUserTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SpUserTransactions_TransactionId",
                table: "SpUserTransactions",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationStatuses_AccountId",
                table: "SynchronizationStatuses",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupRelations_AccountId",
                table: "UserGroupRelations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupRelations_AssetId",
                table: "UserGroupRelations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupRelations_Issuer",
                table: "UserGroupRelations",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentityIssuers_Key",
                table: "UserIdentityIssuers",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrations_AccountId",
                table: "UserRegistrations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrations_AssetId",
                table: "UserRegistrations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegistrations_Issuer",
                table: "UserRegistrations",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_AccountId",
                table: "UserSettings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactionSecrets_AccountId",
                table: "UserTransactionSecrets",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactionSecrets_KeyImage",
                table: "UserTransactionSecrets",
                column: "KeyImage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountKeyValues");

            migrationBuilder.DropTable(
                name: "AssociatedAttributeBackups");

            migrationBuilder.DropTable(
                name: "AutoLogins");

            migrationBuilder.DropTable(
                name: "BiometricRecords");

            migrationBuilder.DropTable(
                name: "ConsentManagementSettings");

            migrationBuilder.DropTable(
                name: "EcCandidateRecords");

            migrationBuilder.DropTable(
                name: "EcPollSelections");

            migrationBuilder.DropTable(
                name: "ExternalIdentityProviders");

            migrationBuilder.DropTable(
                name: "GroupRelations");

            migrationBuilder.DropTable(
                name: "IdentitiesSchemes");

            migrationBuilder.DropTable(
                name: "IdentityAttributes");

            migrationBuilder.DropTable(
                name: "IdentityTargets");

            migrationBuilder.DropTable(
                name: "InherenceSettings");

            migrationBuilder.DropTable(
                name: "ProcessedWitnesss");

            migrationBuilder.DropTable(
                name: "RelationRecords");

            migrationBuilder.DropTable(
                name: "SamlIdentityProviders");

            migrationBuilder.DropTable(
                name: "SamlServiceProviders");

            migrationBuilder.DropTable(
                name: "SamlSettings");

            migrationBuilder.DropTable(
                name: "ScenarioAccounts");

            migrationBuilder.DropTable(
                name: "ServiceProviderRegistrations");

            migrationBuilder.DropTable(
                name: "SpAttributes");

            migrationBuilder.DropTable(
                name: "SpDocumentAllowedSigners");

            migrationBuilder.DropTable(
                name: "SpDocumentSignatures");

            migrationBuilder.DropTable(
                name: "SpIdenitityValidations");

            migrationBuilder.DropTable(
                name: "SpUserTransactions");

            migrationBuilder.DropTable(
                name: "SynchronizationStatuses");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserAssociatedAttributes");

            migrationBuilder.DropTable(
                name: "UserGroupRelations");

            migrationBuilder.DropTable(
                name: "UserIdentityIssuers");

            migrationBuilder.DropTable(
                name: "UserRegistrations");

            migrationBuilder.DropTable(
                name: "UserRootAttributes");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "UserTransactionSecrets");

            migrationBuilder.DropTable(
                name: "EcPollRecords");

            migrationBuilder.DropTable(
                name: "Identities");

            migrationBuilder.DropTable(
                name: "RegistrationCommitments");

            migrationBuilder.DropTable(
                name: "RelationGroups");

            migrationBuilder.DropTable(
                name: "ScenarioSessions");

            migrationBuilder.DropTable(
                name: "SpDocuments");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
