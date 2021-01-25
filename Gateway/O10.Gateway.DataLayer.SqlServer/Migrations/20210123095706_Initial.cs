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
                    Key = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
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
                    KeyImage = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompromisedKeyImages", x => x.CompromisedKeyImageId);
                });

            migrationBuilder.CreateTable(
                name: "PacketHashes",
                columns: table => new
                {
                    PacketHashId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyncBlockHeight = table.Column<long>(nullable: false),
                    CombinedRegistryBlockHeight = table.Column<long>(nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketHashes", x => x.PacketHashId);
                });

            migrationBuilder.CreateTable(
                name: "RegistryCombinedBlocks",
                columns: table => new
                {
                    RegistryCombinedBlockId = table.Column<long>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true)
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
                    Content = table.Column<byte[]>(nullable: true)
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
                    Issuer = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    RegistrationCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    GroupCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    IsRevoked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationRecords", x => x.RelationRecordId);
                });

            migrationBuilder.CreateTable(
                name: "SyncBlocks",
                columns: table => new
                {
                    SyncBlockId = table.Column<long>(nullable: false),
                    Hash = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncBlocks", x => x.SyncBlockId);
                });

            migrationBuilder.CreateTable(
                name: "UtxoKeyImages",
                columns: table => new
                {
                    UtxoKeyImageId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyImage = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtxoKeyImages", x => x.UtxoKeyImageId);
                });

            migrationBuilder.CreateTable(
                name: "UtxoOutputs",
                columns: table => new
                {
                    UtxoOutputId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinationKey = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    Commitment = table.Column<byte[]>(type: "varbinary(64)", nullable: false),
                    OriginatingCommitment = table.Column<byte[]>(nullable: true),
                    IsOverriden = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtxoOutputs", x => x.UtxoOutputId);
                });

            migrationBuilder.CreateTable(
                name: "UtxoTransactionKeys",
                columns: table => new
                {
                    UtxoTransactionKeyId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtxoTransactionKeys", x => x.UtxoTransactionKeyId);
                });

            migrationBuilder.CreateTable(
                name: "AssociatedAttributeIssuances",
                columns: table => new
                {
                    AssociatedAttributeIssuanceId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuerAddressId = table.Column<long>(nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RootAttributes",
                columns: table => new
                {
                    RootAttributeIssuanceId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuerAddressId = table.Column<long>(nullable: true),
                    IssuanceCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    RootCommitment = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
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
                    ReferencedPacketType = table.Column<int>(nullable: false),
                    ReferencedBlockType = table.Column<int>(nullable: false),
                    ReferencedBodyHashPacketHashId = table.Column<long>(nullable: true),
                    ReferencedDestinationKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ReferencedDestinationKey2 = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ReferencedTransactionKey = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    ReferencedKeyImage = table.Column<byte[]>(type: "varbinary(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WitnessPackets", x => x.WitnessPacketId);
                    table.ForeignKey(
                        name: "FK_WitnessPackets_PacketHashes_ReferencedBodyHashPacketHashId",
                        column: x => x.ReferencedBodyHashPacketHashId,
                        principalTable: "PacketHashes",
                        principalColumn: "PacketHashId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StealthPackets",
                columns: table => new
                {
                    StealthPacketId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WitnessId = table.Column<long>(nullable: false),
                    BlockType = table.Column<int>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    TransactionKeyUtxoTransactionKeyId = table.Column<long>(nullable: true),
                    KeyImageUtxoKeyImageId = table.Column<long>(nullable: true),
                    OutputUtxoOutputId = table.Column<long>(nullable: true),
                    ThisBlockHashPacketHashId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StealthPackets", x => x.StealthPacketId);
                    table.ForeignKey(
                        name: "FK_StealthPackets_UtxoKeyImages_KeyImageUtxoKeyImageId",
                        column: x => x.KeyImageUtxoKeyImageId,
                        principalTable: "UtxoKeyImages",
                        principalColumn: "UtxoKeyImageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StealthPackets_UtxoOutputs_OutputUtxoOutputId",
                        column: x => x.OutputUtxoOutputId,
                        principalTable: "UtxoOutputs",
                        principalColumn: "UtxoOutputId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StealthPackets_PacketHashes_ThisBlockHashPacketHashId",
                        column: x => x.ThisBlockHashPacketHashId,
                        principalTable: "PacketHashes",
                        principalColumn: "PacketHashId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StealthPackets_UtxoTransactionKeys_TransactionKeyUtxoTransactionKeyId",
                        column: x => x.TransactionKeyUtxoTransactionKeyId,
                        principalTable: "UtxoTransactionKeys",
                        principalColumn: "UtxoTransactionKeyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionalPackets",
                columns: table => new
                {
                    TransactionalPacketId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WitnessId = table.Column<long>(nullable: false),
                    Height = table.Column<long>(nullable: false),
                    BlockType = table.Column<int>(nullable: false),
                    SourceAddressId = table.Column<long>(nullable: true),
                    TargetAddressId = table.Column<long>(nullable: true),
                    GroupId = table.Column<byte[]>(nullable: true),
                    Content = table.Column<byte[]>(nullable: true),
                    IsTransition = table.Column<bool>(nullable: false),
                    TransactionKeyUtxoTransactionKeyId = table.Column<long>(nullable: true),
                    OutputUtxoOutputId = table.Column<long>(nullable: true),
                    IsVerified = table.Column<bool>(nullable: false),
                    IsValid = table.Column<bool>(nullable: false),
                    ThisBlockHashPacketHashId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionalPackets", x => x.TransactionalPacketId);
                    table.ForeignKey(
                        name: "FK_TransactionalPackets_UtxoOutputs_OutputUtxoOutputId",
                        column: x => x.OutputUtxoOutputId,
                        principalTable: "UtxoOutputs",
                        principalColumn: "UtxoOutputId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionalPackets_Addresses_SourceAddressId",
                        column: x => x.SourceAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionalPackets_Addresses_TargetAddressId",
                        column: x => x.TargetAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionalPackets_PacketHashes_ThisBlockHashPacketHashId",
                        column: x => x.ThisBlockHashPacketHashId,
                        principalTable: "PacketHashes",
                        principalColumn: "PacketHashId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionalPackets_UtxoTransactionKeys_TransactionKeyUtxoTransactionKeyId",
                        column: x => x.TransactionKeyUtxoTransactionKeyId,
                        principalTable: "UtxoTransactionKeys",
                        principalColumn: "UtxoTransactionKeyId",
                        onDelete: ReferentialAction.Restrict);
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
                unique: true,
                filter: "[KeyImage] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PacketHashes_SyncBlockHeight_CombinedRegistryBlockHeight_Hash",
                table: "PacketHashes",
                columns: new[] { "SyncBlockHeight", "CombinedRegistryBlockHeight", "Hash" });

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
                name: "IX_StealthPackets_KeyImageUtxoKeyImageId",
                table: "StealthPackets",
                column: "KeyImageUtxoKeyImageId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthPackets_OutputUtxoOutputId",
                table: "StealthPackets",
                column: "OutputUtxoOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthPackets_ThisBlockHashPacketHashId",
                table: "StealthPackets",
                column: "ThisBlockHashPacketHashId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthPackets_TransactionKeyUtxoTransactionKeyId",
                table: "StealthPackets",
                column: "TransactionKeyUtxoTransactionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_StealthPackets_WitnessId",
                table: "StealthPackets",
                column: "WitnessId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionalPackets_OutputUtxoOutputId",
                table: "TransactionalPackets",
                column: "OutputUtxoOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionalPackets_SourceAddressId",
                table: "TransactionalPackets",
                column: "SourceAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionalPackets_TargetAddressId",
                table: "TransactionalPackets",
                column: "TargetAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionalPackets_ThisBlockHashPacketHashId",
                table: "TransactionalPackets",
                column: "ThisBlockHashPacketHashId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionalPackets_TransactionKeyUtxoTransactionKeyId",
                table: "TransactionalPackets",
                column: "TransactionKeyUtxoTransactionKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionalPackets_WitnessId",
                table: "TransactionalPackets",
                column: "WitnessId");

            migrationBuilder.CreateIndex(
                name: "IX_UtxoKeyImages_KeyImage",
                table: "UtxoKeyImages",
                column: "KeyImage");

            migrationBuilder.CreateIndex(
                name: "IX_UtxoOutputs_Commitment",
                table: "UtxoOutputs",
                column: "Commitment");

            migrationBuilder.CreateIndex(
                name: "IX_UtxoOutputs_DestinationKey",
                table: "UtxoOutputs",
                column: "DestinationKey");

            migrationBuilder.CreateIndex(
                name: "IX_UtxoOutputs_IsOverriden",
                table: "UtxoOutputs",
                column: "IsOverriden");

            migrationBuilder.CreateIndex(
                name: "IX_UtxoTransactionKeys_Key",
                table: "UtxoTransactionKeys",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_CombinedBlockHeight",
                table: "WitnessPackets",
                column: "CombinedBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_WitnessPackets_ReferencedBodyHashPacketHashId",
                table: "WitnessPackets",
                column: "ReferencedBodyHashPacketHashId");

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
                name: "StealthPackets");

            migrationBuilder.DropTable(
                name: "SyncBlocks");

            migrationBuilder.DropTable(
                name: "TransactionalPackets");

            migrationBuilder.DropTable(
                name: "WitnessPackets");

            migrationBuilder.DropTable(
                name: "UtxoKeyImages");

            migrationBuilder.DropTable(
                name: "UtxoOutputs");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "UtxoTransactionKeys");

            migrationBuilder.DropTable(
                name: "PacketHashes");
        }
    }
}
