using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.IdentityProvider.DataLayer.SQLite.Migrations
{
    public partial class BiometricRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProtectionCommitment",
                table: "user_records",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "biometric_record",
                columns: table => new
                {
                    BiometricRecordId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserRecordId = table.Column<long>(nullable: false),
                    RecordType = table.Column<int>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_record", x => x.BiometricRecordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_records_AssetId",
                table: "user_records",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biometric_record_UserRecordId",
                table: "biometric_record",
                column: "UserRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "biometric_record");

            migrationBuilder.DropIndex(
                name: "IX_user_records_AssetId",
                table: "user_records");

            migrationBuilder.DropColumn(
                name: "ProtectionCommitment",
                table: "user_records");
        }
    }
}
