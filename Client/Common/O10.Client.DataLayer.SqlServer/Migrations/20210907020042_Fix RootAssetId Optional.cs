using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Client.DataLayer.SqlServer.Migrations
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
                oldType: "varbinary(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RootAssetId",
                table: "UserAssociatedAttributes",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);
        }
    }
}
