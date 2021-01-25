using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistryCombinedBlocks",
                columns: table => new
                {
                    RegistryCombinedBlockId = table.Column<long>(nullable: false),
                    SyncBlockHeight = table.Column<ulong>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    FullBlockHashes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registry_combined_blocks", x => x.RegistryCombinedBlockId);
                });

            migrationBuilder.CreateTable(
                name: "SynchronizationBlocks",
                columns: table => new
                {
                    SynchronizationBlockId = table.Column<long>(nullable: false),
                    ReceiveTime = table.Column<DateTime>(nullable: false),
                    MedianTime = table.Column<DateTime>(nullable: false),
                    BlockContent = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synchronization_blocks", x => x.SynchronizationBlockId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registry_combined_blocks_SyncBlockHeight",
                table: "RegistryCombinedBlocks",
                column: "SyncBlockHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistryCombinedBlocks");

            migrationBuilder.DropTable(
                name: "SynchronizationBlocks");
        }
    }
}
