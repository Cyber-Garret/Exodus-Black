﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Neuromatrix.Resources.Database;

namespace Neuromatrix.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20190110165525_Migration")]
    partial class Migration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028");

            modelBuilder.Entity("Neuromatrix.Resources.Database.Gear", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Catalyst");

                    b.Property<string>("CatalystBonus");

                    b.Property<string>("CatalystQuest");

                    b.Property<string>("Description");

                    b.Property<string>("DropLocation");

                    b.Property<string>("IconUrl");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<string>("PerkDescription");

                    b.Property<string>("PerkName");

                    b.Property<string>("SecondPerkDescription");

                    b.Property<string>("SecondPerkName");

                    b.Property<string>("Type");

                    b.Property<string>("WhereCatalystDrop");

                    b.HasKey("Id");

                    b.ToTable("Gears");
                });
#pragma warning restore 612, 618
        }
    }
}
