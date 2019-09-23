﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Neira.Db;

namespace Neira.Db.Migrations
{
    [DbContext(typeof(NeiraContext))]
    partial class NeiraContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Neira.Db.Models.ActiveMilestone", b =>
                {
                    b.Property<decimal>("MessageId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<DateTime>("DateExpire");

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("Leader")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Memo")
                        .HasMaxLength(1000);

                    b.Property<int>("MilestoneId");

                    b.Property<decimal>("TextChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("MessageId");

                    b.HasIndex("MilestoneId");

                    b.ToTable("ActiveMilestones");
                });

            modelBuilder.Entity("Neira.Db.Models.Catalyst", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Bonus")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000);

                    b.Property<string>("DropLocation")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("Masterwork")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("WeaponName")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.ToTable("Catalysts");
                });

            modelBuilder.Entity("Neira.Db.Models.Clan", b =>
                {
                    b.Property<long>("Id");

                    b.Property<string>("About");

                    b.Property<DateTimeOffset>("CreateDate");

                    b.Property<long>("MemberCount");

                    b.Property<string>("Motto");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("Neira.Db.Models.Clan_Member", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BungieMembershipId");

                    b.Property<long?>("BungieMembershipType");

                    b.Property<long>("ClanId");

                    b.Property<DateTimeOffset?>("ClanJoinDate");

                    b.Property<DateTimeOffset?>("DateLastPlayed");

                    b.Property<string>("DestinyMembershipId");

                    b.Property<long>("DestinyMembershipType");

                    b.Property<string>("IconPath");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("ClanId");

                    b.ToTable("Clan_Members");
                });

            modelBuilder.Entity("Neira.Db.Models.Gear", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasMaxLength(1024);

                    b.Property<string>("DropLocation")
                        .HasMaxLength(300);

                    b.Property<string>("IconUrl")
                        .HasMaxLength(1000);

                    b.Property<string>("ImageUrl")
                        .HasMaxLength(1000);

                    b.Property<string>("Name")
                        .HasMaxLength(100);

                    b.Property<string>("Perk")
                        .HasMaxLength(100);

                    b.Property<string>("PerkDescription")
                        .HasMaxLength(1024);

                    b.Property<string>("SecondPerk")
                        .HasMaxLength(100);

                    b.Property<string>("SecondPerkDescription")
                        .HasMaxLength(1000);

                    b.Property<string>("Type")
                        .HasMaxLength(100);

                    b.Property<bool>("isHaveCatalyst");

                    b.Property<bool>("isWeapon");

                    b.HasKey("Id");

                    b.ToTable("Gears");
                });

            modelBuilder.Entity("Neira.Db.Models.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("AutoroleID")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("CommandPrefix");

                    b.Property<string>("GlobalMention");

                    b.Property<string>("LeaveMessage");

                    b.Property<decimal>("LoggingChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("NotificationChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("WelcomeChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("WelcomeMessage");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Neira.Db.Models.Milestone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Alias")
                        .HasMaxLength(50);

                    b.Property<string>("Icon")
                        .HasMaxLength(1000);

                    b.Property<string>("Name")
                        .HasMaxLength(100);

                    b.Property<string>("PreviewDesc")
                        .HasMaxLength(1024);

                    b.Property<string>("Type")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("Milestones");
                });

            modelBuilder.Entity("Neira.Db.Models.MilestoneUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("MessageId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.ToTable("MilestoneUsers");
                });

            modelBuilder.Entity("Neira.Db.Models.ActiveMilestone", b =>
                {
                    b.HasOne("Neira.Db.Models.Milestone", "Milestone")
                        .WithMany("ActiveMilestones")
                        .HasForeignKey("MilestoneId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Neira.Db.Models.Clan_Member", b =>
                {
                    b.HasOne("Neira.Db.Models.Clan", "Clan")
                        .WithMany("Members")
                        .HasForeignKey("ClanId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Neira.Db.Models.MilestoneUser", b =>
                {
                    b.HasOne("Neira.Db.Models.ActiveMilestone", "ActiveMilestone")
                        .WithMany("MilestoneUsers")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
