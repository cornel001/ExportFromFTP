﻿// <auto-generated />
using System;
using ExportFromFTP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExportFromFTP.Migrations
{
    [DbContext(typeof(FileInfoContext))]
    partial class FileContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ExportFromFTP.FileInfo", b =>
                {
                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("LastWriteTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("WriteTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Path");

                    b.ToTable("FilesInfo");
                });
#pragma warning restore 612, 618
        }
    }
}
