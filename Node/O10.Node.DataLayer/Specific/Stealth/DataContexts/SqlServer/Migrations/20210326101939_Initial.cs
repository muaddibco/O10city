using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.Stealth.DataContexts.SqlServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyImages",
                columns: table => new
                {
                    KeyImageId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ValueString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyImages", x => x.KeyImageId);
                });

            migrationBuilder.CreateTable(
                name: "StealthTransactionHashKeys",
                columns: table => new
                {
                    StealthTransactionHashKeyId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistryHeight = table.Column<long>(nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    HashString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StealthTransactionHashKeys", x => x.StealthTransactionHashKeyId);
                });

            migrationBuilder.CreateTable(
                name: "StealthTransactions",
                columns: table => new
                {
                    StealthTransactionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HashKeyStealthTransactionHashKeyId = table.Column<long>(nullable: true),
                    RegistryHeight = table.Column<long>(nullable: false),
                    KeyImageId = table.Column<long>(nullable: true),
                    BlockType = table.Column<int>(nullable: false),
                    DestinationKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StealthTransactions", x => x.StealthTransactionId);
                    table.ForeignKey(
                        name: "FK_StealthTransactions_StealthTransactionHashKeys_HashKeyStealthTransactionHashKeyId",
                        column: x => x.HashKeyStealthTransactionHashKeyId,
                        principalTable: "StealthTransactionHashKeys",
                        principalColumn: "StealthTransactionHashKeyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StealthTransactions_KeyImages_KeyImageId",
                        column: x => x.KeyImageId,
                        principalTable: "KeyImages",
                        principalColumn: "KeyImageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyImages_Value",
                table: "KeyImages",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactionHashKeys_Hash",
                table: "StealthTransactionHashKeys",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactionHashKeys_RegistryHeight",
                table: "StealthTransactionHashKeys",
                column: "RegistryHeight");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_HashKeyStealthTransactionHashKeyId",
                table: "StealthTransactions",
                column: "HashKeyStealthTransactionHashKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_KeyImageId",
                table: "StealthTransactions",
                column: "KeyImageId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_RegistryHeight",
                table: "StealthTransactions",
                column: "RegistryHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StealthTransactions");

            migrationBuilder.DropTable(
                name: "StealthTransactionHashKeys");

            migrationBuilder.DropTable(
                name: "KeyImages");
        }
    }
}
