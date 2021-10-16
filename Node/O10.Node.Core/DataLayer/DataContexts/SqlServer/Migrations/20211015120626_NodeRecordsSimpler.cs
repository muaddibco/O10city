using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.Core.DataLayer.DataContexts.SqlServer.Migrations
{
    public partial class NodeRecordsSimpler : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PublicKey",
                table: "NodeRecords",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(64)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "NodeRecords",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "PublicKey",
                table: "NodeRecords",
                type: "varbinary(64)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "IPAddress",
                table: "NodeRecords",
                type: "varbinary(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
