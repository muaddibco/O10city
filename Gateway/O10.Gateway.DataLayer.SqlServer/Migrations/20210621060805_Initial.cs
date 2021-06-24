using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace O10.Gateway.DataLayer.SqlServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressId);
                });

            migrationBuilder.CreateTable(
                name: "CompromisedKeyImages",
                columns: table => new
                {
                    CompromisedKeyImageId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyImage = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompromisedKeyImages", x => x.CompromisedKeyImageId);
                });

            migrationBuilder.CreateTable(
                name: "RegistryCombinedBlocks",
                columns: table => new
                {
                    RegistryCombinedBlockId = table.Column<long>(nullable: false),
                    Content = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistryCombinedBlocks", x => x.RegistryCombinedBlockId);
                });

            migrationBuilder.CreateTable(
                name: "RegistryFullBlocks",
                columns: table => new
                {
                    RegistryFullBlockDataId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CombinedBlockHeight = table.Column<long>(nullable: false),
                    Content = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistryFullBlocks", x => x.RegistryFullBlockDataId);
                });

            migrationBuilder.CreateTable(
                name: "RelationRecords",
                columns: table => new
                {
                    RelationRecordId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Issuer = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    RegistrationCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    GroupCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    IsRevoked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationRecords", x => x.RelationRecordId);
                });

            migrationBuilder.CreateTable(
                name: "StealthOutputs",
                columns: table => new
                {
                    StealthOutputId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinationKey = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    Commitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    OriginatingCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    IsOverriden = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StealthOutputs", x => x.StealthOutputId);
                });

            migrationBuilder.CreateTable(
                name: "SyncBlocks",
                columns: table => new
                {
                    SyncBlockId = table.Column<long>(nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncBlocks", x => x.SyncBlockId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionHashes",
                columns: table => new
                {
                    TransactionHashId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregatedTransactionsHeight = table.Column<long>(nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHashes", x => x.TransactionHashId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionKeys",
                columns: table => new
                {
                    TransactionKeyId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionKeys", x => x.TransactionKeyId);
                });

            migrationBuilder.CreateTable(
                name: "UtxoKeyImages",
                columns: table => new
                {
                    KeyImageId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtxoKeyImages", x => x.KeyImageId);
                });

            migrationBuilder.CreateTable(
                name: "AssociatedAttributeIssuances",
                columns: table => new
                {
                    AssociatedAttributeIssuanceId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuerAddressId = table.Column<long>(nullable: false),
                    IssuanceCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    RootIssuanceCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociatedAttributeIssuances", x => x.AssociatedAttributeIssuanceId);
                    table.ForeignKey(
                        name: "FK_AssociatedAttributeIssuances_Addresses_IssuerAddressId",
                        column: x => x.IssuerAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RootAttributes",
                columns: table => new
                {
                    RootAttributeIssuanceId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuerAddressId = table.Column<long>(nullable: false),
                    IssuanceCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    RootCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    IsOverriden = table.Column<bool>(nullable: false),
                    IssuanceCombinedBlock = table.Column<long>(nullable: false),
                    RevocationCombinedBlock = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootAttributes", x => x.RootAttributeIssuanceId);
                    table.ForeignKey(
                        name: "FK_RootAttributes_Addresses_IssuerAddressId",
                        column: x => x.IssuerAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WitnessPackets",
                columns: table => new
                {
                    WitnessPacketId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyncBlockHeight = table.Column<long>(nullable: false),
                    Round = table.Column<long>(nullable: false),
                    CombinedBlockHeight = table.Column<long>(nullable: false),
                    ReferencedLedgerType = table.Column<int>(nullable: false),
                    ReferencedPacketType = table.Column<int>(nullable: false),
                    ReferencedBodyHashTransactionHashId = table.Column<long>(nullable: false),
                    ReferencedDestinationKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ReferencedDestinationKey2 = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ReferencedTransactionKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ReferencedKeyImage = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WitnessPackets", x => x.WitnessPacketId);
                    table.ForeignKey(
                        name: "FK_WitnessPackets_TransactionHashes_ReferencedBodyHashTransactionHashId",
                        column: x => x.ReferencedBodyHashTransactionHashId,
                        principalTable: "TransactionHashes",
                        principalColumn: "TransactionHashId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StateTransactions",
                columns: table => new
                {
                    StateTransactionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WitnessId = table.Column<long>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false),
                    SourceAddressId = table.Column<long>(nullable: false),
                    TargetAddressId = table.Column<long>(nullable: true),
                    Content = table.Column<string>(nullable: false),
                    HashTransactionHashId = table.Column<long>(nullable: true),
                    TransactionKeyId = table.Column<long>(nullable: true),
                    OutputStealthOutputId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTransactions", x => x.StateTransactionId);
                    table.ForeignKey(
                        name: "FK_StateTransactions_TransactionHashes_HashTransactionHashId",
                        column: x => x.HashTransactionHashId,
                        principalTable: "TransactionHashes",
                        principalColumn: "TransactionHashId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StateTransactions_StealthOutputs_OutputStealthOutputId",
                        column: x => x.OutputStealthOutputId,
                        principalTable: "StealthOutputs",
                        principalColumn: "StealthOutputId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StateTransactions_Addresses_SourceAddressId",
                        column: x => x.SourceAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StateTransactions_Addresses_TargetAddressId",
                        column: x => x.TargetAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StateTransactions_TransactionKeys_TransactionKeyId",
                        column: x => x.TransactionKeyId,
                        principalTable: "TransactionKeys",
                        principalColumn: "TransactionKeyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StealthTransactions",
                columns: table => new
                {
                    StealthTransactionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WitnessId = table.Column<long>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    TransactionKeyId = table.Column<long>(nullable: false),
                    KeyImageId = table.Column<long>(nullable: false),
                    OutputStealthOutputId = table.Column<long>(nullable: false),
                    HashTransactionHashId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StealthTransactions", x => x.StealthTransactionId);
                    table.ForeignKey(
                        name: "FK_StealthTransactions_TransactionHashes_HashTransactionHashId",
                        column: x => x.HashTransactionHashId,
                        principalTable: "TransactionHashes",
                        principalColumn: "TransactionHashId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StealthTransactions_UtxoKeyImages_KeyImageId",
                        column: x => x.KeyImageId,
                        principalTable: "UtxoKeyImages",
                        principalColumn: "KeyImageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StealthTransactions_StealthOutputs_OutputStealthOutputId",
                        column: x => x.OutputStealthOutputId,
                        principalTable: "StealthOutputs",
                        principalColumn: "StealthOutputId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StealthTransactions_TransactionKeys_TransactionKeyId",
                        column: x => x.TransactionKeyId,
                        principalTable: "TransactionKeys",
                        principalColumn: "TransactionKeyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Key",
                table: "Addresses",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_AssociatedAttributeIssuances_IssuanceCommitment",
                table: "AssociatedAttributeIssuances",
                column: "IssuanceCommitment");

            migrationBuilder.CreateIndex(
                name: "IX_AssociatedAttributeIssuances_IssuerAddressId",
                table: "AssociatedAttributeIssuances",
                column: "IssuerAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AssociatedAttributeIssuances_RootIssuanceCommitment",
                table: "AssociatedAttributeIssuances",
                column: "RootIssuanceCommitment");

            migrationBuilder.CreateIndex(
                name: "IX_CompromisedKeyImages_KeyImage",
                table: "CompromisedKeyImages",
                column: "KeyImage",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistryFullBlocks_CombinedBlockHeight",
                table: "RegistryFullBlocks",
                column: "CombinedBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_RelationRecords_IsRevoked",
                table: "RelationRecords",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_RelationRecords_Issuer",
                table: "RelationRecords",
                column: "Issuer");

            migrationBuilder.CreateIndex(
                name: "IX_RelationRecords_RegistrationCommitment",
                table: "RelationRecords",
                column: "RegistrationCommitment");

            migrationBuilder.CreateIndex(
                name: "IX_RootAttributes_IsOverriden",
                table: "RootAttributes",
                column: "IsOverriden");

            migrationBuilder.CreateIndex(
                name: "IX_RootAttributes_IssuanceCommitment",
                table: "RootAttributes",
                column: "IssuanceCommitment");

            migrationBuilder.CreateIndex(
                name: "IX_RootAttributes_IssuerAddressId",
                table: "RootAttributes",
                column: "IssuerAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_RootAttributes_RootCommitment",
                table: "RootAttributes",
                column: "RootCommitment");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransactions_HashTransactionHashId",
                table: "StateTransactions",
                column: "HashTransactionHashId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransactions_OutputStealthOutputId",
                table: "StateTransactions",
                column: "OutputStealthOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransactions_SourceAddressId",
                table: "StateTransactions",
                column: "SourceAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransactions_TargetAddressId",
                table: "StateTransactions",
                column: "TargetAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransactions_TransactionKeyId",
                table: "StateTransactions",
                column: "TransactionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_StateTransactions_WitnessId",
                table: "StateTransactions",
                column: "WitnessId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthOutputs_Commitment",
                table: "StealthOutputs",
                column: "Commitment");

            migrationBuilder.CreateIndex(
                name: "IX_StealthOutputs_DestinationKey",
                table: "StealthOutputs",
                column: "DestinationKey");

            migrationBuilder.CreateIndex(
                name: "IX_StealthOutputs_IsOverriden",
                table: "StealthOutputs",
                column: "IsOverriden");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_HashTransactionHashId",
                table: "StealthTransactions",
                column: "HashTransactionHashId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_KeyImageId",
                table: "StealthTransactions",
                column: "KeyImageId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_OutputStealthOutputId",
                table: "StealthTransactions",
                column: "OutputStealthOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_TransactionKeyId",
                table: "StealthTransactions",
                column: "TransactionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthTransactions_WitnessId",
                table: "StealthTransactions",
                column: "WitnessId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHashes_AggregatedTransactionsHeight_Hash",
                table: "TransactionHashes",
                columns: new[] { "AggregatedTransactionsHeight", "Hash" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionKeys_Key",
                table: "TransactionKeys",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_UtxoKeyImages_Value",
                table: "UtxoKeyImages",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_CombinedBlockHeight",
                table: "WitnessPackets",
                column: "CombinedBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_ReferencedBodyHashTransactionHashId",
                table: "WitnessPackets",
                column: "ReferencedBodyHashTransactionHashId");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_ReferencedDestinationKey",
                table: "WitnessPackets",
                column: "ReferencedDestinationKey");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_ReferencedDestinationKey2",
                table: "WitnessPackets",
                column: "ReferencedDestinationKey2");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_ReferencedKeyImage",
                table: "WitnessPackets",
                column: "ReferencedKeyImage");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_ReferencedTransactionKey",
                table: "WitnessPackets",
                column: "ReferencedTransactionKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssociatedAttributeIssuances");

            migrationBuilder.DropTable(
                name: "CompromisedKeyImages");

            migrationBuilder.DropTable(
                name: "RegistryCombinedBlocks");

            migrationBuilder.DropTable(
                name: "RegistryFullBlocks");

            migrationBuilder.DropTable(
                name: "RelationRecords");

            migrationBuilder.DropTable(
                name: "RootAttributes");

            migrationBuilder.DropTable(
                name: "StateTransactions");

            migrationBuilder.DropTable(
                name: "StealthTransactions");

            migrationBuilder.DropTable(
                name: "SyncBlocks");

            migrationBuilder.DropTable(
                name: "WitnessPackets");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "UtxoKeyImages");

            migrationBuilder.DropTable(
                name: "StealthOutputs");

            migrationBuilder.DropTable(
                name: "TransactionKeys");

            migrationBuilder.DropTable(
                name: "TransactionHashes");
        }
    }
}
