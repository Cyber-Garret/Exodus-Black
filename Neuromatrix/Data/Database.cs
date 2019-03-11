using System;
using System.Linq;

using Discord;

using Neuromatrix.Models.Db;

namespace Neuromatrix.Data
{
    public static class Database
    {
        internal static Gear GetGears(string ItemName)
        {
            using (var DbContext = new SqliteDbContext())
            {
                try
                {
                    Gear gear = DbContext.Gears.Where(g => g.Name.IndexOf(ItemName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
                    if (gear == null)
                        return null;
                    else
                        return gear;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.UtcNow} Source: {ex.Source}] Message: {ex.Message}");
                    return null;
                }
            }
        }

        internal static Guild GetGuildAccount(ulong id)
        {
            using (var DbContext = new SqliteDbContext())
            {
                try
                {
                    if (DbContext.Guilds.Where(g => g.GuildID == id).Count() < 1)
                        return null;
                    return DbContext.Guilds.FirstOrDefault(g => g.GuildID == id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.UtcNow} Source: {ex.Source}] Message: {ex.Message}");
                    return null;
                }
            }
        }

        internal static Guild GetGuildAccount(IGuild guild)
        {
            return GetGuildAccount(guild.Id);
        }

    }
}
