﻿// <auto-generated />
using System;
using EStoreAPI.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    [DbContext(typeof(EStoreDbContext))]
    [Migration("20241118023019_AddNavigationProperty")]
    partial class AddNavigationProperty
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EStoreAPI.Server.Models.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CustomerId"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string[]>("PhoneNumbers")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Device", b =>
                {
                    b.Property<int>("DeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DeviceId"));

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DeviceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("DeviceId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Job", b =>
                {
                    b.Property<int>("JobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("JobId"));

                    b.Property<decimal?>("CollectedPrice")
                        .HasColumnType("numeric");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<int>("DeviceId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("EstimatedPickupTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("EstimatedPrice")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsFinished")
                        .HasColumnType("boolean");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PickupTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ReceiveTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("JobId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("DeviceId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Problem", b =>
                {
                    b.Property<int>("ProblemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ProblemId"));

                    b.Property<int>("DeviceId")
                        .HasColumnType("integer");

                    b.Property<int?>("JobId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("ProblemName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ProblemId");

                    b.HasIndex("DeviceId");

                    b.HasIndex("JobId");

                    b.ToTable("Problems");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Job", b =>
                {
                    b.HasOne("EStoreAPI.Server.Models.Customer", "Customer")
                        .WithMany("Jobs")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EStoreAPI.Server.Models.Device", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("Device");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Problem", b =>
                {
                    b.HasOne("EStoreAPI.Server.Models.Device", "Device")
                        .WithMany("Problems")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EStoreAPI.Server.Models.Job", null)
                        .WithMany("Problems")
                        .HasForeignKey("JobId");

                    b.Navigation("Device");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Customer", b =>
                {
                    b.Navigation("Jobs");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Device", b =>
                {
                    b.Navigation("Problems");
                });

            modelBuilder.Entity("EStoreAPI.Server.Models.Job", b =>
                {
                    b.Navigation("Problems");
                });
#pragma warning restore 612, 618
        }
    }
}
