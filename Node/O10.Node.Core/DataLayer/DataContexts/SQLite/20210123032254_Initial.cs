using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.Core.DataLayer.DataContexts.SQLite
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
                        .Annotation("Sqlite:Autoincrement", true),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicKey = table.Column<string>(type: "varbinary(64)", nullable: true),
                    IPAddress = table.Column<string>(type: "varbinary(32)", nullable: true),
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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NodeRecords_PublicKey_NodeRole",
                table: "NodeRecords",
                columns: new[] { "PublicKey", "NodeRole" },
                unique: true);
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
