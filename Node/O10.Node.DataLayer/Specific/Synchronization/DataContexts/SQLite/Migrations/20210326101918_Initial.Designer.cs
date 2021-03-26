﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite;

namespace O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210326101918_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Synchronization.Model.AggregatedRegistrationsTransaction", b =>
                {
                    b.Property<long>("AggregatedRegistrationsTransactionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<string>("FullBlockHashes")
                        .HasColumnType("TEXT");

                    b.Property<long>("SyncBlockHeight")
                        .HasColumnType("INTEGER");

                    b.HasKey("AggregatedRegistrationsTransactionId");

                    b.HasIndex("SyncBlockHeight");

                    b.ToTable("AggregatedRegistrationsTransactions");
                });

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Synchronization.Model.SynchronizationPacket", b =>
                {
                    b.Property<long>("SynchronizationPacketId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("MedianTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReceiveTime")
                        .HasColumnType("TEXT");

                    b.HasKey("SynchronizationPacketId");

                    b.ToTable("SynchronizationPackets");
                });
#pragma warning restore 612, 618
        }
    }
}
