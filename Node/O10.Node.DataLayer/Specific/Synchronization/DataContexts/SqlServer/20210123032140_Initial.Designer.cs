﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.DataLayer.Specific.Synchronization.DataContexts.SqlServer;

namespace O10.Node.DataLayer.Specific.Synchronization.DataContexts.SqlServer
{
    [DbContext(typeof(DataContext))]
    [Migration("20210123032140_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Synchronization.Model.RegistryCombinedBlock", b =>
                {
                    b.Property<long>("RegistryCombinedBlockId")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Content")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("FullBlockHashes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SyncBlockHeight")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("RegistryCombinedBlockId");

                    b.HasIndex("SyncBlockHeight");

                    b.ToTable("registry_combined_blocks");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Synchronization.Model.SynchronizationBlock", b =>
                {
                    b.Property<long>("SynchronizationBlockId")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("BlockContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime>("MedianTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ReceiveTime")
                        .HasColumnType("datetime2");

                    b.HasKey("SynchronizationBlockId");

                    b.ToTable("synchronization_blocks");
                });
#pragma warning restore 612, 618
        }
    }
}
