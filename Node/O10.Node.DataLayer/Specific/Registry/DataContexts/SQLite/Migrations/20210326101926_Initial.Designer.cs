﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.DataLayer.Specific.Registry.DataContexts.SQLite;

namespace O10.Node.DataLayer.Specific.Registry.DataContexts.SQLite.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210326101926_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4");

            modelBuilder.Entity("O10.Node.DataLayer.Specific.Registry.Model.RegistryFullBlock", b =>
                {
                    b.Property<long>("RegistryFullBlockId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hash")
                        .HasColumnType("varbinary(64)");

                    b.Property<string>("HashString")
                        .HasColumnType("TEXT");

                    b.Property<long>("Round")
                        .HasColumnType("INTEGER");

                    b.Property<long>("SyncBlockHeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TransactionsCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("RegistryFullBlockId");

                    b.HasIndex("SyncBlockHeight");

                    b.ToTable("RegistryFullBlocks");
                });
#pragma warning restore 612, 618
        }
    }
}