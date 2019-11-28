﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Database
{
	public class NeiraLinkContext : DbContext
	{
		public NeiraLinkContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySql("Server=95.181.198.84;Database=NeiraLink;Uid=Garret;Pwd=dGF12(TW8e;");
			//optionsBuilder.UseSqlite("Data Source=UserData/NeiraLink.db;");
		}
		//Web
		public virtual DbSet<BotInfo> BotInfos { get; set; }
		//Discord
		public virtual DbSet<Guild> Guilds { get; set; }
		public virtual DbSet<UserAccount> UserAccounts { get; set; }
		public virtual DbSet<GuildUserAccount> GuildUserAccounts { get; set; }
		public virtual DbSet<ADOnline> ADOnlines { get; set; }

		//Destiny 2
		public virtual DbSet<Gear> Gears { get; set; }
		public virtual DbSet<Catalyst> Catalysts { get; set; }
		public virtual DbSet<Clan> Clans { get; set; }
		public virtual DbSet<Clan_Member> Clan_Members { get; set; }
		public virtual DbSet<Clan_Member_Stat> Clan_Member_Stats { get; set; }
		//Milestones
		public virtual DbSet<Milestone> Milestones { get; set; }
		public virtual DbSet<ActiveMilestone> ActiveMilestones { get; set; }
		public virtual DbSet<MilestoneUser> MilestoneUsers { get; set; }
	}
}