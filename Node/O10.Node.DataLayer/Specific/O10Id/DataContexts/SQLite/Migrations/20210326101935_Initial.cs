using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.O10Id.DataContexts.SQLite.Migrations
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
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyHash = table.Column<ulong>(nullable: false),
                    PublicKey = table.Column<string>(type: "varbinary(64)", nullable: true)
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
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistryHeight = table.Column<long>(nullable: false),
                    Hash = table.Column<string>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O10TransactionHashKeys", x => x.O10TransactionHashKeyId);
                });

            migrationBuilder.CreateTable(
                name: "O10TransactionSources",
                columns: table => new
                {
                    O10TransactionSourceId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityAccountIdentityId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_O10TransactionSources", x => x.O10TransactionSourceId);
                    table.ForeignKey(
                        name: "FK_O10TransactionSources_O10AccountIdentity_IdentityAccountIdentityId",
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
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceO10TransactionSourceId = table.Column<long>(nullable: true),
                    HashKeyO10TransactionHashKeyId = table.Column<long>(nullable: true),
                    RegistryHeight = table.Column<long>(nullable: false),
                    Height = table.Column<long>(nullable: false),
                    PacketType = table.Column<ushort>(nullable: false),
                    Content = table.Column<string>(nullable: true)
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
                        name: "FK_O10Transactions_O10TransactionSources_SourceO10TransactionSourceId",
                        column: x => x.SourceO10TransactionSourceId,
                        principalTable: "O10TransactionSources",
                        principalColumn: "O10TransactionSourceId",
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
                name: "IX_O10TransactionHashKeys_RegistryHeight",
                table: "O10TransactionHashKeys",
                column: "RegistryHeight");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_HashKeyO10TransactionHashKeyId",
                table: "O10Transactions",
                column: "HashKeyO10TransactionHashKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_Height",
                table: "O10Transactions",
                column: "Height");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_RegistryHeight",
                table: "O10Transactions",
                column: "RegistryHeight");

            migrationBuilder.CreateIndex(
                name: "IX_O10Transactions_SourceO10TransactionSourceId",
                table: "O10Transactions",
                column: "SourceO10TransactionSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_O10TransactionSources_IdentityAccountIdentityId",
                table: "O10TransactionSources",
                column: "IdentityAccountIdentityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "O10Transactions");

            migrationBuilder.DropTable(
                name: "O10TransactionHashKeys");

            migrationBuilder.DropTable(
                name: "O10TransactionSources");

            migrationBuilder.DropTable(
                name: "O10AccountIdentity");
        }
    }
}
