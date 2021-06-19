using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Client.DataLayer.SQLite.Migrations
{
    public partial class Refactoring1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_synchronization_statuses_accounts_AccountId",
                table: "synchronization_statuses");

            migrationBuilder.DropTable(
                name: "sp_employees");

            migrationBuilder.DropTable(
                name: "sp_employee_groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_root_attributes",
                table: "user_root_attributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_synchronization_statuses",
                table: "synchronization_statuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_registration_commitments",
                table: "registration_commitments");

            migrationBuilder.DropColumn(
                name: "BlindingFactor",
                table: "user_transaction_secrets");

            migrationBuilder.DropColumn(
                name: "OriginalCommitment",
                table: "user_root_attributes");

            migrationBuilder.RenameTable(
                name: "user_root_attributes",
                newName: "UserRootAttributes");

            migrationBuilder.RenameTable(
                name: "synchronization_statuses",
                newName: "SynchronizationStatuses");

            migrationBuilder.RenameTable(
                name: "registration_commitments",
                newName: "RegistrationCommitments");

            migrationBuilder.RenameIndex(
                name: "IX_synchronization_statuses_AccountId",
                table: "SynchronizationStatuses",
                newName: "IX_SynchronizationStatuses_AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_registration_commitments_Issuer",
                table: "RegistrationCommitments",
                newName: "IX_RegistrationCommitments_Issuer");

            migrationBuilder.RenameIndex(
                name: "IX_registration_commitments_AssetId",
                table: "RegistrationCommitments",
                newName: "IX_RegistrationCommitments_AssetId");

            migrationBuilder.AlterColumn<string>(
                name: "Commitment",
                table: "attributes",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AnchoringOriginationCommitment",
                table: "UserRootAttributes",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddColumn<byte[]>(
                name: "IssuanceTransactionKey",
                table: "UserRootAttributes",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRootAttributes",
                table: "UserRootAttributes",
                column: "UserAttributeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SynchronizationStatuses",
                table: "SynchronizationStatuses",
                column: "SynchronizationStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistrationCommitments",
                table: "RegistrationCommitments",
                column: "RegistrationCommitmentId");

            migrationBuilder.CreateTable(
                name: "RelationGroups",
                columns: table => new
                {
                    RelationGroupId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    GroupName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationGroups", x => x.RelationGroupId);
                    table.ForeignKey(
                        name: "FK_RelationGroups_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelationRecords",
                columns: table => new
                {
                    RelationRecordId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    RootAttributeValue = table.Column<string>(nullable: false),
                    RegistrationCommitmentId = table.Column<long>(nullable: true),
                    RelationGroupId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationRecords", x => x.RelationRecordId);
                    table.ForeignKey(
                        name: "FK_RelationRecords_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
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

            migrationBuilder.CreateIndex(
                name: "IX_attributes_Commitment",
                table: "attributes",
                column: "Commitment");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCommitments_Commitment",
                table: "RegistrationCommitments",
                column: "Commitment",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_SynchronizationStatuses_accounts_AccountId",
                table: "SynchronizationStatuses",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SynchronizationStatuses_accounts_AccountId",
                table: "SynchronizationStatuses");

            migrationBuilder.DropTable(
                name: "RelationRecords");

            migrationBuilder.DropTable(
                name: "RelationGroups");

            migrationBuilder.DropIndex(
                name: "IX_attributes_Commitment",
                table: "attributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRootAttributes",
                table: "UserRootAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SynchronizationStatuses",
                table: "SynchronizationStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistrationCommitments",
                table: "RegistrationCommitments");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationCommitments_Commitment",
                table: "RegistrationCommitments");

            migrationBuilder.DropColumn(
                name: "AnchoringOriginationCommitment",
                table: "UserRootAttributes");

            migrationBuilder.DropColumn(
                name: "IssuanceTransactionKey",
                table: "UserRootAttributes");

            migrationBuilder.RenameTable(
                name: "UserRootAttributes",
                newName: "user_root_attributes");

            migrationBuilder.RenameTable(
                name: "SynchronizationStatuses",
                newName: "synchronization_statuses");

            migrationBuilder.RenameTable(
                name: "RegistrationCommitments",
                newName: "registration_commitments");

            migrationBuilder.RenameIndex(
                name: "IX_SynchronizationStatuses_AccountId",
                table: "synchronization_statuses",
                newName: "IX_synchronization_statuses_AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationCommitments_Issuer",
                table: "registration_commitments",
                newName: "IX_registration_commitments_Issuer");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrationCommitments_AssetId",
                table: "registration_commitments",
                newName: "IX_registration_commitments_AssetId");

            migrationBuilder.AddColumn<string>(
                name: "BlindingFactor",
                table: "user_transaction_secrets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Commitment",
                table: "attributes",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "OriginalCommitment",
                table: "user_root_attributes",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_root_attributes",
                table: "user_root_attributes",
                column: "UserAttributeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_synchronization_statuses",
                table: "synchronization_statuses",
                column: "SynchronizationStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_registration_commitments",
                table: "registration_commitments",
                column: "RegistrationCommitmentId");

            migrationBuilder.CreateTable(
                name: "sp_employee_groups",
                columns: table => new
                {
                    SpEmployeeGroupId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: true),
                    GroupName = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "sp_employees",
                columns: table => new
                {
                    SpEmployeeId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    RegistrationCommitment = table.Column<string>(type: "TEXT", nullable: true),
                    RootAttributeRaw = table.Column<string>(type: "TEXT", nullable: false),
                    SpEmployeeGroupId = table.Column<long>(type: "INTEGER", nullable: true)
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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sp_employees_SpEmployeeGroupId",
                table: "sp_employees",
                column: "SpEmployeeGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_synchronization_statuses_accounts_AccountId",
                table: "synchronization_statuses",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
