using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.Core.DataLayer.DataContexts.SqlServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gateways",
                columns: table => new
                {
                    GatewayId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alias = table.Column<string>(nullable: true),
                    BaseUri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gateways", x => x.GatewayId);
                });

            migrationBuilder.CreateTable(
                name: "NodeRecords",
                columns: table => new
                {
                    NodeRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    IPAddress = table.Column<byte[]>(type: "varbinary(32)", nullable: true),
                    NodeRole = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeRecords", x => x.NodeRecordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gateways_BaseUri",
                table: "gateways",
                column: "BaseUri",
                unique: true,
                filter: "[BaseUri] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRecords_PublicKey_NodeRole",
                table: "NodeRecords",
                columns: new[] { "PublicKey", "NodeRole" },
                unique: true,
                filter: "[PublicKey] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gateways");

            migrationBuilder.DropTable(
                name: "NodeRecords");
        }
    }
}
