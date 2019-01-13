using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Reflection;

namespace Neuromatrix.Resources.Database
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Gear> Gears { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"/Data/Neuromatrix.db";
            Options.UseSqlite($"DataSource={DbLocation}");
        }
    }
}