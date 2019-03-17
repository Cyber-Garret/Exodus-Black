using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;

using Neuromatrix.Models.Db;

namespace Neuromatrix.Data
{
    public static class Database
    {
        #region Exotics
        internal static Gear GetGears(string ItemName)
        {
            using (var DbContext = new SqliteDbContext())
            {
                Gear gear = DbContext.Gears.Where(g => g.Name.IndexOf(ItemName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
                if (gear == null)
                    return null;
                else
                    return gear;
            }
        }
        #endregion

        #region Guilds
        internal static Guild GetGuildAccount(IGuild guild)
        {
            return GetGuildAccount(guild.Id);
        }

        internal static Guild GetGuildAccount(ulong id)
        {
            try
            {
                using (var DbContext = new SqliteDbContext())
                {
                    if (DbContext.Guilds.Where(g => g.GuildID == id).Count() < 1)
                        return null;
                    return DbContext.Guilds.FirstOrDefault(g => g.GuildID == id);
                }
            }
            catch
            {

                return null;
            }
        }

        internal static Task CreateGuildAccount(IGuild guild)
        {
            using (SqliteDbContext context = new SqliteDbContext())
            {
                Guild new_Guild = new Guild
                {
                    GuildID = guild.Id,
                    GuildName = guild.Name,
                    GuildOwnerId = guild.OwnerId
                };

                context.Guilds.Add(new_Guild);

                context.SaveChanges();
                Console.WriteLine("Гильдия успешно сохранена.");
                return Task.CompletedTask;
            }
        }

        internal static Task UpdateGuildInfo(IGuild guild, DbData dataForUpdate, object value)
        {
            using (var Db = new SqliteDbContext())
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
                Db.SaveChanges();
                Console.WriteLine("Гильдия успешно сохранена.");
                return Task.CompletedTask;

            }
        }

        #endregion

        public enum DbData
        {
            NotificationChannel,
            LoggingChannel,
            EnableLogging
        }
    }
}
