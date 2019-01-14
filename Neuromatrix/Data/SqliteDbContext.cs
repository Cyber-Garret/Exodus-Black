using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

using Neuromatrix.Models.Db;

namespace Neuromatrix.Data
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Gear> Gears { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"/UserData/Neuromatrix.db";
            Options.UseSqlite($"DataSource={DbLocation}");
        }
    }
}
