﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Node.DataLayer.Specific.Registry.DataContexts.SqlServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistryFullBlocks",
                columns: table => new
                {
                    RegistryFullBlockId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyncBlockHeight = table.Column<long>(nullable: false),
                    Round = table.Column<long>(nullable: false),
                    TransactionsCount = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistryFullBlocks", x => x.RegistryFullBlockId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistryFullBlocks_SyncBlockHeight",
                table: "RegistryFullBlocks",
                column: "SyncBlockHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistryFullBlocks");
        }
    }
}
