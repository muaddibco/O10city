using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Client.DataLayer.SqlServer.Migrations
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    Key = table.Column<string>(maxLength: 255, nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountKeyValues", x => x.AccountKeyValueId);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    AccountId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SecretViewKey = table.Column<byte[]>(nullable: true),
                    PublicViewKey = table.Column<byte[]>(nullable: true),
                    SecretSpendKey = table.Column<byte[]>(nullable: true),
                    PublicSpendKey = table.Column<byte[]>(nullable: true),
                    AccountType = table.Column<byte>(nullable: false),
                    AccountInfo = table.Column<string>(nullable: true),
                    IsCompromised = table.Column<bool>(nullable: false),
                    LastAggregatedRegistrations = table.Column<decimal>(nullable: false),
                    IsPrivate = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "associated_attribute_backup",
                columns: table => new
                {
                    AssociatedAttributeBackupId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RootIssuer = table.Column<string>(nullable: true),
                    RootAssetId = table.Column<string>(nullable: true),
                    AssociatedIssuer = table.Column<string>(nullable: true),
                    SchemeName = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_associated_attribute_backup", x => x.AssociatedAttributeBackupId);
                });

            migrationBuilder.CreateTable(
                name: "biometric_records",
                columns: table => new
                {
                    BiometricRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserData = table.Column<string>(nullable: true),
                    PersonGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_records", x => x.BiometricRecordId);
                });

            migrationBuilder.CreateTable(
                name: "consent_management_settings",
                columns: table => new
                {
                    ConsentManagementSettingsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consent_management_settings", x => x.ConsentManagementSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "EcPollRecords",
                columns: table => new
                {
                    EcPollRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcPollRecords", x => x.EcPollRecordId);
                });

            migrationBuilder.CreateTable(
                name: "external_identity_providers",
                columns: table => new
                {
                    ExternalIdentityProviderId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_identity_providers", x => x.ExternalIdentityProviderId);
                });

            migrationBuilder.CreateTable(
                name: "group_relations",
                columns: table => new
                {
                    GroupRelationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupOwnerKey = table.Column<string>(nullable: true),
                    GroupName = table.Column<string>(nullable: true),
                    AssetId = table.Column<string>(nullable: true),
                    Issuer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_relations", x => x.GroupRelationId);
                });

            migrationBuilder.CreateTable(
                name: "identities_schemes",
                columns: table => new
                {
                    IdentitiesSchemeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeName = table.Column<string>(nullable: false),
                    AttributeSchemeName = table.Column<string>(nullable: false),
                    Alias = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CanBeRoot = table.Column<bool>(nullable: false),
                    Issuer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identities_schemes", x => x.IdentitiesSchemeId);
                });

            migrationBuilder.CreateTable(
                name: "identity",
                columns: table => new
                {
                    IdentityId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity", x => x.IdentityId);
                });

            migrationBuilder.CreateTable(
                name: "IdentityTargets",
                columns: table => new
                {
                    IdentityTargetId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityId = table.Column<long>(nullable: false),
                    PublicSpendKey = table.Column<string>(nullable: true),
                    PublicViewKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityTargets", x => x.IdentityTargetId);
                });

            migrationBuilder.CreateTable(
                name: "inherence_settings",
                columns: table => new
                {
                    InherenceSettingId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inherence_settings", x => x.InherenceSettingId);
                });

            migrationBuilder.CreateTable(
                name: "processed_witnesses",
                columns: table => new
                {
                    ProcessedWitnessId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    WitnessId = table.Column<long>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_witnesses", x => x.ProcessedWitnessId);
                });

            migrationBuilder.CreateTable(
                name: "registration_commitments",
                columns: table => new
                {
                    RegistrationCommitmentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Commitment = table.Column<string>(nullable: true),
                    ServiceProviderInfo = table.Column<string>(nullable: true),
                    AssetId = table.Column<string>(nullable: true),
                    Issuer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_commitments", x => x.RegistrationCommitmentId);
                });

            migrationBuilder.CreateTable(
                name: "saml_identity_providers",
                columns: table => new
                {
                    SamlIdentityProviderId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<string>(nullable: true),
                    SecretViewKey = table.Column<string>(nullable: true),
                    PublicSpendKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saml_identity_providers", x => x.SamlIdentityProviderId);
                });

            migrationBuilder.CreateTable(
                name: "saml_service_providers",
                columns: table => new
                {
                    SamlServiceProviderId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<string>(nullable: true),
                    SingleLogoutUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saml_service_providers", x => x.SamlServiceProviderId);
                });

            migrationBuilder.CreateTable(
                name: "saml_settings",
                columns: table => new
                {
                    SamlSettingsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultSamlIdpId = table.Column<long>(nullable: false),
                    DefaultSamlIdpAccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saml_settings", x => x.SamlSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "scenario_sessions",
                columns: table => new
                {
                    ScenarioSessionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSubject = table.Column<string>(nullable: true),
                    ScenarioId = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    CurrentStep = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenario_sessions", x => x.ScenarioSessionId);
                });

            migrationBuilder.CreateTable(
                name: "service_provider_registrations",
                columns: table => new
                {
                    ServiceProviderRegistrationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    Commitment = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_provider_registrations", x => x.ServiceProviderRegistrationId);
                });

            migrationBuilder.CreateTable(
                name: "sp_attributes",
                columns: table => new
                {
                    SpAttributeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    AttributeSchemeName = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    AssetId = table.Column<byte[]>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    OriginalBlindingFactor = table.Column<byte[]>(nullable: false),
                    OriginalCommitment = table.Column<byte[]>(nullable: false),
                    IssuingCommitment = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_attributes", x => x.SpAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "sp_identity_validations",
                columns: table => new
                {
                    SpIdenitityValidationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    SchemeName = table.Column<string>(nullable: true),
                    ValidationType = table.Column<int>(nullable: false),
                    NumericCriterion = table.Column<int>(nullable: true),
                    GroupIdCriterion = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_identity_validations", x => x.SpIdenitityValidationId);
                });

            migrationBuilder.CreateTable(
                name: "sp_user_transactions",
                columns: table => new
                {
                    SpUserTransactionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    ServiceProviderRegistrationId = table.Column<long>(nullable: false),
                    TransactionId = table.Column<string>(nullable: true),
                    TransactionDescription = table.Column<string>(nullable: true),
                    IsProcessed = table.Column<bool>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    IsCompromised = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_user_transactions", x => x.SpUserTransactionId);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    SystemSettingsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitializationVector = table.Column<byte[]>(nullable: true),
                    BiometricSecretKey = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.SystemSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "user_associated_attribute",
                columns: table => new
                {
                    UserAssociatedAttributeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    AttributeSchemeName = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: true),
                    LastUpdateTime = table.Column<DateTime>(nullable: true),
                    RootAssetId = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_associated_attribute", x => x.UserAssociatedAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "user_identity_issuers",
                columns: table => new
                {
                    UserIdentityIssuerId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_identity_issuers", x => x.UserIdentityIssuerId);
                });

            migrationBuilder.CreateTable(
                name: "user_root_attributes",
                columns: table => new
                {
                    UserAttributeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    SchemeName = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    AssetId = table.Column<byte[]>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    IssuanceCommitment = table.Column<byte[]>(nullable: false),
                    OriginalBlindingFactor = table.Column<byte[]>(nullable: false),
                    OriginalCommitment = table.Column<byte[]>(nullable: false),
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
                    table.PrimaryKey("PK_user_root_attributes", x => x.UserAttributeId);
                });

            migrationBuilder.CreateTable(
                name: "user_transaction_secrets",
                columns: table => new
                {
                    UserTransactionSecretsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: false),
                    KeyImage = table.Column<string>(nullable: true),
                    Issuer = table.Column<string>(nullable: true),
                    AssetId = table.Column<string>(nullable: true),
                    BlindingFactor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_transaction_secrets", x => x.UserTransactionSecretsId);
                });

            migrationBuilder.CreateTable(
                name: "auto_logins",
                columns: table => new
                {
                    AutoLoginId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SecretKey = table.Column<byte[]>(nullable: true),
                    AccountId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auto_logins", x => x.AutoLoginId);
                    table.ForeignKey(
                        name: "FK_auto_logins_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sp_documents",
                columns: table => new
                {
                    SpDocumentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    DocumentName = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    LastChangeRecordHeight = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_documents", x => x.SpDocumentId);
                    table.ForeignKey(
                        name: "FK_sp_documents_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sp_employee_groups",
                columns: table => new
                {
                    SpEmployeeGroupId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    GroupName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_employee_groups", x => x.SpEmployeeGroupId);
                    table.ForeignKey(
                        name: "FK_sp_employee_groups_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "synchronization_statuses",
                columns: table => new
                {
                    SynchronizationStatusId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    LastUpdatedCombinedBlockHeight = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synchronization_statuses", x => x.SynchronizationStatusId);
                    table.ForeignKey(
                        name: "FK_synchronization_statuses_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_registrations",
                columns: table => new
                {
                    UserRegistrationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    Commitment = table.Column<string>(nullable: true),
                    ServiceProviderInfo = table.Column<string>(nullable: true),
                    AssetId = table.Column<string>(nullable: true),
                    Issuer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_registrations", x => x.UserRegistrationId);
                    table.ForeignKey(
                        name: "FK_user_registrations_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    UserSettingsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    IsAutoTheftProtection = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.UserSettingsId);
                    table.ForeignKey(
                        name: "FK_user_settings_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_to_group_relations",
                columns: table => new
                {
                    UserGroupRelationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    GroupOwnerName = table.Column<string>(nullable: true),
                    GroupOwnerKey = table.Column<string>(nullable: true),
                    GroupName = table.Column<string>(nullable: true),
                    AssetId = table.Column<string>(nullable: true),
                    Issuer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_to_group_relations", x => x.UserGroupRelationId);
                    table.ForeignKey(
                        name: "FK_user_to_group_relations_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EcCandidateRecords",
                columns: table => new
                {
                    EcCandidateRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    AssetId = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    EcPollRecordId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcCandidateRecords", x => x.EcCandidateRecordId);
                    table.ForeignKey(
                        name: "FK_EcCandidateRecords_EcPollRecords_EcPollRecordId",
                        column: x => x.EcPollRecordId,
                        principalTable: "EcPollRecords",
                        principalColumn: "EcPollRecordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EcPollSelections",
                columns: table => new
                {
                    EcPollSelectionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EcPollRecordId = table.Column<long>(nullable: true),
                    EcCommitment = table.Column<string>(nullable: true),
                    EcBlindingFactor = table.Column<string>(nullable: true),
                    VoterBlindingFactor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcPollSelections", x => x.EcPollSelectionId);
                    table.ForeignKey(
                        name: "FK_EcPollSelections_EcPollRecords_EcPollRecordId",
                        column: x => x.EcPollRecordId,
                        principalTable: "EcPollRecords",
                        principalColumn: "EcPollRecordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attributes",
                columns: table => new
                {
                    AttributeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityId = table.Column<long>(nullable: true),
                    AttributeName = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Subject = table.Column<int>(nullable: false),
                    Commitment = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attributes", x => x.AttributeId);
                    table.ForeignKey(
                        name: "FK_attributes_identity_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "identity",
                        principalColumn: "IdentityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "scenario_accounts",
                columns: table => new
                {
                    ScenarioAccountId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioSessionId = table.Column<long>(nullable: true),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenario_accounts", x => x.ScenarioAccountId);
                    table.ForeignKey(
                        name: "FK_scenario_accounts_scenario_sessions_ScenarioSessionId",
                        column: x => x.ScenarioSessionId,
                        principalTable: "scenario_sessions",
                        principalColumn: "ScenarioSessionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sp_document_allowed_signers",
                columns: table => new
                {
                    SpDocumentAllowedSignerId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    DocumentSpDocumentId = table.Column<long>(nullable: true),
                    GroupIssuer = table.Column<string>(nullable: true),
                    GroupName = table.Column<string>(nullable: true),
                    GroupCommitment = table.Column<string>(nullable: true),
                    BlindingFactor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_document_allowed_signers", x => x.SpDocumentAllowedSignerId);
                    table.ForeignKey(
                        name: "FK_sp_document_allowed_signers_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sp_document_allowed_signers_sp_documents_DocumentSpDocumentId",
                        column: x => x.DocumentSpDocumentId,
                        principalTable: "sp_documents",
                        principalColumn: "SpDocumentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sp_document_signatures",
                columns: table => new
                {
                    SpDocumentSignatureId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentSpDocumentId = table.Column<long>(nullable: true),
                    SignatureRecordHeight = table.Column<decimal>(nullable: false),
                    DocumentRecordHeight = table.Column<decimal>(nullable: false),
                    DocumentSignRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_document_signatures", x => x.SpDocumentSignatureId);
                    table.ForeignKey(
                        name: "FK_sp_document_signatures_sp_documents_DocumentSpDocumentId",
                        column: x => x.DocumentSpDocumentId,
                        principalTable: "sp_documents",
                        principalColumn: "SpDocumentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sp_employees",
                columns: table => new
                {
                    SpEmployeeId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    RootAttributeRaw = table.Column<string>(nullable: false),
                    RegistrationCommitment = table.Column<string>(nullable: true),
                    SpEmployeeGroupId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sp_employees", x => x.SpEmployeeId);
                    table.ForeignKey(
                        name: "FK_sp_employees_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sp_employees_sp_employee_groups_SpEmployeeGroupId",
                        column: x => x.SpEmployeeGroupId,
                        principalTable: "sp_employee_groups",
                        principalColumn: "SpEmployeeGroupId",
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
                name: "IX_associated_attribute_backup_RootAssetId",
                table: "associated_attribute_backup",
                column: "RootAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_associated_attribute_backup_RootIssuer",
                table: "associated_attribute_backup",
                column: "RootIssuer");

            migrationBuilder.CreateIndex(
                name: "IX_attributes_IdentityId",
                table: "attributes",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_auto_logins_AccountId",
                table: "auto_logins",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_records_UserData",
                table: "biometric_records",
                column: "UserData",
                unique: true,
                filter: "[UserData] IS NOT NULL");

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
                name: "IX_external_identity_providers_Name",
                table: "external_identity_providers",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_group_relations_AssetId",
                table: "group_relations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_group_relations_Issuer",
                table: "group_relations",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_identities_schemes_AttributeName",
                table: "identities_schemes",
                column: "AttributeName");

            migrationBuilder.CreateIndex(
                name: "IX_identities_schemes_Issuer",
                table: "identities_schemes",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityTargets_IdentityId",
                table: "IdentityTargets",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_inherence_settings_Name",
                table: "inherence_settings",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_witnesses_AccountId_WitnessId",
                table: "processed_witnesses",
                columns: new[] { "AccountId", "WitnessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registration_commitments_AssetId",
                table: "registration_commitments",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_registration_commitments_Issuer",
                table: "registration_commitments",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_saml_identity_providers_EntityId",
                table: "saml_identity_providers",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_saml_service_providers_EntityId",
                table: "saml_service_providers",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_scenario_accounts_ScenarioSessionId",
                table: "scenario_accounts",
                column: "ScenarioSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_scenario_sessions_UserSubject",
                table: "scenario_sessions",
                column: "UserSubject");

            migrationBuilder.CreateIndex(
                name: "IX_sp_document_allowed_signers_AccountId",
                table: "sp_document_allowed_signers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_document_allowed_signers_DocumentSpDocumentId",
                table: "sp_document_allowed_signers",
                column: "DocumentSpDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_document_signatures_DocumentRecordHeight",
                table: "sp_document_signatures",
                column: "DocumentRecordHeight");

            migrationBuilder.CreateIndex(
                name: "IX_sp_document_signatures_DocumentSpDocumentId",
                table: "sp_document_signatures",
                column: "DocumentSpDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_document_signatures_SignatureRecordHeight",
                table: "sp_document_signatures",
                column: "SignatureRecordHeight");

            migrationBuilder.CreateIndex(
                name: "IX_sp_documents_AccountId",
                table: "sp_documents",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_documents_DocumentName",
                table: "sp_documents",
                column: "DocumentName");

            migrationBuilder.CreateIndex(
                name: "IX_sp_employee_groups_AccountId",
                table: "sp_employee_groups",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_employees_AccountId",
                table: "sp_employees",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_employees_RegistrationCommitment",
                table: "sp_employees",
                column: "RegistrationCommitment",
                unique: true,
                filter: "[RegistrationCommitment] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sp_employees_SpEmployeeGroupId",
                table: "sp_employees",
                column: "SpEmployeeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_user_transactions_AccountId",
                table: "sp_user_transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sp_user_transactions_TransactionId",
                table: "sp_user_transactions",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_synchronization_statuses_AccountId",
                table: "synchronization_statuses",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_user_identity_issuers_Key",
                table: "user_identity_issuers",
                column: "Key",
                unique: true,
                filter: "[Key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_registrations_AccountId",
                table: "user_registrations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_user_registrations_AssetId",
                table: "user_registrations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_user_registrations_Issuer",
                table: "user_registrations",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_AccountId",
                table: "user_settings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_user_to_group_relations_AccountId",
                table: "user_to_group_relations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_user_to_group_relations_AssetId",
                table: "user_to_group_relations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_user_to_group_relations_Issuer",
                table: "user_to_group_relations",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_user_transaction_secrets_AccountId",
                table: "user_transaction_secrets",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_user_transaction_secrets_KeyImage",
                table: "user_transaction_secrets",
                column: "KeyImage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountKeyValues");

            migrationBuilder.DropTable(
                name: "associated_attribute_backup");

            migrationBuilder.DropTable(
                name: "attributes");

            migrationBuilder.DropTable(
                name: "auto_logins");

            migrationBuilder.DropTable(
                name: "biometric_records");

            migrationBuilder.DropTable(
                name: "consent_management_settings");

            migrationBuilder.DropTable(
                name: "EcCandidateRecords");

            migrationBuilder.DropTable(
                name: "EcPollSelections");

            migrationBuilder.DropTable(
                name: "external_identity_providers");

            migrationBuilder.DropTable(
                name: "group_relations");

            migrationBuilder.DropTable(
                name: "identities_schemes");

            migrationBuilder.DropTable(
                name: "IdentityTargets");

            migrationBuilder.DropTable(
                name: "inherence_settings");

            migrationBuilder.DropTable(
                name: "processed_witnesses");

            migrationBuilder.DropTable(
                name: "registration_commitments");

            migrationBuilder.DropTable(
                name: "saml_identity_providers");

            migrationBuilder.DropTable(
                name: "saml_service_providers");

            migrationBuilder.DropTable(
                name: "saml_settings");

            migrationBuilder.DropTable(
                name: "scenario_accounts");

            migrationBuilder.DropTable(
                name: "service_provider_registrations");

            migrationBuilder.DropTable(
                name: "sp_attributes");

            migrationBuilder.DropTable(
                name: "sp_document_allowed_signers");

            migrationBuilder.DropTable(
                name: "sp_document_signatures");

            migrationBuilder.DropTable(
                name: "sp_employees");

            migrationBuilder.DropTable(
                name: "sp_identity_validations");

            migrationBuilder.DropTable(
                name: "sp_user_transactions");

            migrationBuilder.DropTable(
                name: "synchronization_statuses");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "user_associated_attribute");

            migrationBuilder.DropTable(
                name: "user_identity_issuers");

            migrationBuilder.DropTable(
                name: "user_registrations");

            migrationBuilder.DropTable(
                name: "user_root_attributes");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "user_to_group_relations");

            migrationBuilder.DropTable(
                name: "user_transaction_secrets");

            migrationBuilder.DropTable(
                name: "identity");

            migrationBuilder.DropTable(
                name: "EcPollRecords");

            migrationBuilder.DropTable(
                name: "scenario_sessions");

            migrationBuilder.DropTable(
                name: "sp_documents");

            migrationBuilder.DropTable(
                name: "sp_employee_groups");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
