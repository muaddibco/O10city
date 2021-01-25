﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite;

namespace O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite
{
    [DbContext(typeof(DataContext))]
    [Migration("20210123032148_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Synchronization.Model.RegistryCombinedBlock", b =>
                {
                    b.Property<long>("RegistryCombinedBlockId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<string>("FullBlockHashes")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("SyncBlockHeight")
                        .HasColumnType("INTEGER");

                    b.HasKey("RegistryCombinedBlockId");

                    b.HasIndex("SyncBlockHeight");

                    b.ToTable("registry_combined_blocks");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Synchronization.Model.SynchronizationBlock", b =>
                {
                    b.Property<long>("SynchronizationBlockId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("BlockContent")
                        .HasColumnType("BLOB");

                    b.Property<DateTime>("MedianTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReceiveTime")
                        .HasColumnType("TEXT");

                    b.HasKey("SynchronizationBlockId");

                    b.ToTable("synchronization_blocks");
                });
#pragma warning restore 612, 618
        }
    }
}
