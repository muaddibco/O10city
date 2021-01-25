using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.O10Id.DataContexts.SqlServer
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "O10AccountIdentity",
                columns: table => new
                {
                    AccountIdentityId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyHash = table.Column<decimal>(nullable: false),
                    PublicKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O10AccountIdentity", x => x.AccountIdentityId);
                });

            migrationBuilder.CreateTable(
                name: "O10TransactionHashKeys",
                columns: table => new
                {
                    O10TransactionHashKeyId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CombinedBlockHeight = table.Column<decimal>(nullable: false),
                    SyncBlockHeight = table.Column<decimal>(nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O10TransactionHashKeys", x => x.O10TransactionHashKeyId);
                });

            migrationBuilder.CreateTable(
                name: "O10TransactionIdentities",
                columns: table => new
                {
                    O10TransactionIdentityId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityAccountIdentityId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O10TransactionIdentities", x => x.O10TransactionIdentityId);
                    table.ForeignKey(
                        name: "FK_O10TransactionIdentities_O10AccountIdentity_IdentityAccountIdentityId",
                        column: x => x.IdentityAccountIdentityId,
                        principalTable: "O10AccountIdentity",
                        principalColumn: "AccountIdentityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "O10Transactions",
                columns: table => new
                {
                    O10TransactionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityO10TransactionIdentityId = table.Column<long>(nullable: true),
                    HashKeyO10TransactionHashKeyId = table.Column<long>(nullable: true),
                    SyncBlockHeight = table.Column<long>(nullable: false),
                    BlockHeight = table.Column<long>(nullable: false),
                    BlockType = table.Column<int>(nullable: false),
                    BlockContent = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O10Transactions", x => x.O10TransactionId);
                    table.ForeignKey(
                        name: "FK_O10Transactions_O10TransactionHashKeys_HashKeyO10TransactionHashKeyId",
                        column: x => x.HashKeyO10TransactionHashKeyId,
                        principalTable: "O10TransactionHashKeys",
                        principalColumn: "O10TransactionHashKeyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_O10Transactions_O10TransactionIdentities_IdentityO10TransactionIdentityId",
                        column: x => x.IdentityO10TransactionIdentityId,
                        principalTable: "O10TransactionIdentities",
                        principalColumn: "O10TransactionIdentityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_O10AccountIdentity_KeyHash",
                table: "O10AccountIdentity",
                column: "KeyHash");

            migrationBuilder.CreateIndex(
                name: "IX_O10TransactionHashKeys_Hash",
                table: "O10TransactionHashKeys",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_O10TransactionHashKeys_SyncBlockHeight",
                table: "O10TransactionHashKeys",
                column: "SyncBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_O10TransactionIdentities_IdentityAccountIdentityId",
                table: "O10TransactionIdentities",
                column: "IdentityAccountIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_BlockHeight",
                table: "O10Transactions",
                column: "BlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_HashKeyO10TransactionHashKeyId",
                table: "O10Transactions",
                column: "HashKeyO10TransactionHashKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_IdentityO10TransactionIdentityId",
                table: "O10Transactions",
                column: "IdentityO10TransactionIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_SyncBlockHeight",
                table: "O10Transactions",
                column: "SyncBlockHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "O10Transactions");

            migrationBuilder.DropTable(
                name: "O10TransactionHashKeys");

            migrationBuilder.DropTable(
                name: "O10TransactionIdentities");

            migrationBuilder.DropTable(
                name: "O10AccountIdentity");
        }
    }
}
