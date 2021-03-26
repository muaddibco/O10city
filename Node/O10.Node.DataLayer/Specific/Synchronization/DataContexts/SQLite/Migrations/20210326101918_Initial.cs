using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedRegistrationsTransactions",
                columns: table => new
                {
                    AggregatedRegistrationsTransactionId = table.Column<long>(nullable: false),
                    SyncBlockHeight = table.Column<long>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    FullBlockHashes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedRegistrationsTransactions", x => x.AggregatedRegistrationsTransactionId);
                });

            migrationBuilder.CreateTable(
                name: "SynchronizationPackets",
                columns: table => new
                {
                    SynchronizationPacketId = table.Column<long>(nullable: false),
                    ReceiveTime = table.Column<DateTime>(nullable: false),
                    MedianTime = table.Column<DateTime>(nullable: false),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynchronizationPackets", x => x.SynchronizationPacketId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedRegistrationsTransactions_SyncBlockHeight",
                table: "AggregatedRegistrationsTransactions",
                column: "SyncBlockHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedRegistrationsTransactions");

            migrationBuilder.DropTable(
                name: "SynchronizationPackets");
        }
    }
}
