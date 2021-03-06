﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.DataLayer.Specific.O10Id.DataContexts.SQLite;

namespace O10.Node.DataLayer.Specific.O10Id.DataContexts.SQLite.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210326101935_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.AccountIdentity", b =>
                {
                    b.Property<long>("AccountIdentityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("KeyHash")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PublicKey")
                        .HasColumnType("varbinary(64)");

                    b.HasKey("AccountIdentityId");

                    b.HasIndex("KeyHash");

                    b.ToTable("O10AccountIdentity");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10Transaction", b =>
                {
                    b.Property<long>("O10TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<long?>("HashKeyO10TransactionHashKeyId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("PacketType")
                        .HasColumnType("INTEGER");

                    b.Property<long>("RegistryHeight")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("SourceO10TransactionSourceId")
                        .HasColumnType("INTEGER");

                    b.HasKey("O10TransactionId");

                    b.HasIndex("HashKeyO10TransactionHashKeyId");

                    b.HasIndex("Height");

                    b.HasIndex("RegistryHeight");

                    b.HasIndex("SourceO10TransactionSourceId");

                    b.ToTable("O10Transactions");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionHashKey", b =>
                {
                    b.Property<long>("O10TransactionHashKeyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Hash")
                        .HasColumnType("varbinary(64)");

                    b.Property<long>("RegistryHeight")
                        .HasColumnType("INTEGER");

                    b.HasKey("O10TransactionHashKeyId");

                    b.HasIndex("Hash");

                    b.HasIndex("RegistryHeight");

                    b.ToTable("O10TransactionHashKeys");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionSource", b =>
                {
                    b.Property<long>("O10TransactionSourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long?>("IdentityAccountIdentityId")
                        .HasColumnType("INTEGER");

                    b.HasKey("O10TransactionSourceId");

                    b.HasIndex("IdentityAccountIdentityId");

                    b.ToTable("O10TransactionSources");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10Transaction", b =>
                {
                    b.HasOne("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionHashKey", "HashKey")
                        .WithMany()
                        .HasForeignKey("HashKeyO10TransactionHashKeyId");

                    b.HasOne("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionSource", "Source")
                        .WithMany("Transactions")
                        .HasForeignKey("SourceO10TransactionSourceId");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.O10Id.Model.O10TransactionSource", b =>
                {
                    b.HasOne("O10.Node.DataLayer.Specific.O10Id.Model.AccountIdentity", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityAccountIdentityId");
                });
#pragma warning restore 612, 618
        }
    }
}
