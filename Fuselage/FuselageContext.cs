using Fuselage.Models;

using Microsoft.EntityFrameworkCore;

namespace Fuselage
{
    public class FuselageContext : DbContext
    {
        public FuselageContext(DbContextOptions<FuselageContext> options)
        : base(options)
        { }

        /// <summary>
        /// Localized random welcomes
        /// </summary>
        public DbSet<Welcome> Welcomes { get; set; }

        public DbSet<Catalyst> Catalysts { get; set; }

        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<MilestoneLocale> MilestoneLocales { get; set; }
    }
}
