using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.IdentityProvider.DataLayer.SqlServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blinding_factor_records",
                columns: table => new
                {
                    BlindingFactorsRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IssuanceBlindingFactor = table.Column<string>(nullable: true),
                    BiometricBlindingFactor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blinding_factor_records", x => x.BlindingFactorsRecordId);
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_settings",
                columns: table => new
                {
                    IdentityProviderSettingsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_provider_settings", x => x.IdentityProviderSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "user_records",
                columns: table => new
                {
                    UserRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssetId = table.Column<string>(nullable: true),
                    IssuanceCommitment = table.Column<string>(nullable: true),
                    IssuanceBiometricCommitment = table.Column<string>(nullable: true),
                    IssuanceBlindingRecordId = table.Column<long>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastUpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_records", x => x.UserRecordId);
                });

            migrationBuilder.CreateTable(
                name: "registration_sessions",
                columns: table => new
                {
                    RegistrationSessionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserRecordId = table.Column<long>(nullable: true),
                    SessionKey = table.Column<string>(nullable: true),
                    SessionCommitment = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_sessions", x => x.RegistrationSessionId);
                    table.ForeignKey(
                        name: "FK_registration_sessions_user_records_UserRecordId",
                        column: x => x.UserRecordId,
                        principalTable: "user_records",
                        principalColumn: "UserRecordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registration_sessions_SessionCommitment",
                table: "registration_sessions",
                column: "SessionCommitment",
                unique: true,
                filter: "[SessionCommitment] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_registration_sessions_SessionKey",
                table: "registration_sessions",
                column: "SessionKey",
                unique: true,
                filter: "[SessionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_registration_sessions_UserRecordId",
                table: "registration_sessions",
                column: "UserRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_user_records_IssuanceCommitment",
                table: "user_records",
                column: "IssuanceCommitment",
                unique: true,
                filter: "[IssuanceCommitment] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blinding_factor_records");

            migrationBuilder.DropTable(
                name: "identity_provider_settings");

            migrationBuilder.DropTable(
                name: "registration_sessions");

            migrationBuilder.DropTable(
                name: "user_records");
        }
    }
}
