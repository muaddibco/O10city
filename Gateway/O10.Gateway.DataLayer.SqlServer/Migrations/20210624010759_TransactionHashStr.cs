using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Gateway.DataLayer.SqlServer.Migrations
{
    public partial class TransactionHashStr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashString",
                table: "TransactionHashes",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashString",
                table: "TransactionHashes");
        }
    }
}
