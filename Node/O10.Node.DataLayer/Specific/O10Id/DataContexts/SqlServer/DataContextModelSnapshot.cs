﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.DataLayer.Specific.O10Id.DataContexts.SqlServer;

namespace O10.Node.DataLayer.Specific.O10Id.DataContexts.SqlServer
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.AccountIdentity", b =>
                {
                    b.Property<long>("AccountIdentityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("KeyHash")
                        .HasColumnType("decimal(20,0)");

                    b.Property<byte[]>("PublicKey")
                        .HasColumnType("varbinary(64)");

                    b.HasKey("AccountIdentityId");

                    b.HasIndex("KeyHash");

                    b.ToTable("O10AccountIdentity");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10Transaction", b =>
                {
                    b.Property<long>("O10TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("BlockContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<long>("BlockHeight")
                        .HasColumnType("bigint");

                    b.Property<int>("BlockType")
                        .HasColumnType("int");

                    b.Property<long?>("HashKeyO10TransactionHashKeyId")
                        .HasColumnType("bigint");

                    b.Property<long?>("IdentityO10TransactionIdentityId")
                        .HasColumnType("bigint");

                    b.Property<long>("SyncBlockHeight")
                        .HasColumnType("bigint");

                    b.HasKey("O10TransactionId");

                    b.HasIndex("BlockHeight");

                    b.HasIndex("HashKeyO10TransactionHashKeyId");

                    b.HasIndex("IdentityO10TransactionIdentityId");

                    b.HasIndex("SyncBlockHeight");

                    b.ToTable("O10Transactions");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionHashKey", b =>
                {
                    b.Property<long>("O10TransactionHashKeyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("CombinedBlockHeight")
                        .HasColumnType("decimal(20,0)");

                    b.Property<byte[]>("Hash")
                        .HasColumnType("varbinary(64)");

                    b.Property<decimal>("SyncBlockHeight")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("O10TransactionHashKeyId");

                    b.HasIndex("Hash");

                    b.HasIndex("SyncBlockHeight");

                    b.ToTable("O10TransactionHashKeys");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionIdentity", b =>
                {
                    b.Property<long>("O10TransactionIdentityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("IdentityAccountIdentityId")
                        .HasColumnType("bigint");

                    b.HasKey("O10TransactionIdentityId");

                    b.HasIndex("IdentityAccountIdentityId");

                    b.ToTable("O10TransactionIdentities");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10Transaction", b =>
                {
                    b.HasOne("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionHashKey", "HashKey")
                        .WithMany()
                        .HasForeignKey("HashKeyO10TransactionHashKeyId");

                    b.HasOne("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionIdentity", "Identity")
                        .WithMany("Transactions")
                        .HasForeignKey("IdentityO10TransactionIdentityId");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionIdentity", b =>
                {
                    b.HasOne("O10.Node.DataLayer.Specific.O10Id.Model.AccountIdentity", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityAccountIdentityId");
                });
#pragma warning restore 612, 618
        }
    }
}
