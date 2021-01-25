﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using O10.Node.Core.DataLayer.DataContexts.SqlServer;

namespace O10.Node.Core.DataLayer.DataContexts.SqlServer
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

            modelBuilder.Entity("O10.Node.Core.DataLayer.DataContexts.Gateway", b =>
                {
                    b.Property<int>("GatewayId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Alias")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BaseUri")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("GatewayId");

                    b.HasIndex("BaseUri")
                        .IsUnique()
                        .HasFilter("[BaseUri] IS NOT NULL");

                    b.ToTable("gateways");
                });

            modelBuilder.Entity("O10.Node.Core.DataLayer.DataContexts.NodeRecord", b =>
                {
                    b.Property<long>("NodeRecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("IPAddress")
                        .HasColumnType("varbinary(32)");

                    b.Property<byte>("NodeRole")
                        .HasColumnType("tinyint");

                    b.Property<byte[]>("PublicKey")
                        .HasColumnType("varbinary(64)");

                    b.HasKey("NodeRecordId");

                    b.HasIndex("PublicKey", "NodeRole")
                        .IsUnique()
                        .HasFilter("[PublicKey] IS NOT NULL");

                    b.ToTable("NodeRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
