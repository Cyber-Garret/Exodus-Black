using System;
using System.Linq;
using System.Threading.Tasks;
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

        internal static async Task CreateGuildAccount(IGuild guild)
        {
            using (var DbContext = new SqliteDbContext())
            {
                try
                {
                    DbContext.Guilds.Add(new Guild
                    {
                        GuildID = guild.Id,
                        GuildName = guild.Name,
                        GuildOwnerId = guild.OwnerId
                    });

                    await DbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.UtcNow} Source: {ex.Source}] Message: {ex.Message}");
                }
            }
        }

        internal static async Task UpdateGuildInfo(IGuild guild, DbData dataForUpdate, object value)
        {
            using (var Db = new SqliteDbContext())
            {
                try
                {
                    var GuildData = Db.Guilds.FirstOrDefault(g => g.GuildID == guild.Id);
                    if (dataForUpdate == DbData.EnableLogging)
                    {
                        GuildData.EnableLogging = (bool)value;
                    }
                    if (dataForUpdate == DbData.NotificationChannel)
                    {
                        GuildData.NotificationChannel = (ulong)value;
                    }
                    if (dataForUpdate == DbData.LoggingChannel)
                    {
                        GuildData.LoggingChannel = (ulong)value;
                    }

                    Db.Guilds.Update(GuildData);
                    await Db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.UtcNow} Source: {ex.Source}] Message: {ex.Message}");
                }

            }
        }

        public enum DbData
        {
            NotificationChannel,
            LoggingChannel,
            EnableLogging
        }
    }
}
