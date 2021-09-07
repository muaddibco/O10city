using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Client.DataLayer.SQLite.Migrations
{
    public partial class FixRootAssetIdOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RootAssetId",
                table: "UserAssociatedAttributes",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RootAssetId",
                table: "UserAssociatedAttributes",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);
        }
    }
}
