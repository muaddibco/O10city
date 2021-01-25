using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.Stealth.DataContexts.SQLite
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
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "varbinary(64)", nullable: true)
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
                        .Annotation("Sqlite:Autoincrement", true),
                    CombinedBlockHeight = table.Column<ulong>(nullable: false),
                    SyncBlockHeight = table.Column<ulong>(nullable: false),
                    Hash = table.Column<string>(type: "varbinary(64)", nullable: true)
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
                        .Annotation("Sqlite:Autoincrement", true),
                    HashKeyStealthTransactionHashKeyId = table.Column<long>(nullable: true),
                    SyncBlockHeight = table.Column<ulong>(nullable: false),
                    KeyImageId = table.Column<long>(nullable: true),
                    BlockType = table.Column<ushort>(nullable: false),
                    DestinationKey = table.Column<string>(type: "varbinary(64)", nullable: true),
                    Content = table.Column<byte[]>(nullable: true)
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
                name: "IX_StealthTransactionHashKeys_SyncBlockHeight",
                table: "StealthTransactionHashKeys",
                column: "SyncBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_HashKeyStealthTransactionHashKeyId",
                table: "StealthTransactions",
                column: "HashKeyStealthTransactionHashKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_KeyImageId",
                table: "StealthTransactions",
                column: "KeyImageId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_SyncBlockHeight",
                table: "StealthTransactions",
                column: "SyncBlockHeight");
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
