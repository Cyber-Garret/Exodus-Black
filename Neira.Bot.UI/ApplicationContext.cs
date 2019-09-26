using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Neira.Bot.UI.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Neira.Bot.UI
{
	public class ApplicationContext : DbContext
	{
		public DbSet<Clan> Clans { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			var connection = new SqlConnectionStringBuilder
			{
				DataSource = "ip",
				InitialCatalog = "db",
				UserID = "login",
				Password = "password",
				MultipleActiveResultSets = true
			};

			builder.UseSqlServer(connection.ConnectionString);
		}
	}
}
