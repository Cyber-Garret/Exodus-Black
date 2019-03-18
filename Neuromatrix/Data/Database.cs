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

        /// <summary>
        /// Поиск экзотического снаряжения в локальном хранилище по названию вещи.
        /// </summary>
        /// <param name="ItemName">полное или частичное название вещи для поиска в бд.</param>
        /// <returns>Gear class</returns>
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

        /// <summary>
        /// Возвращает данные о гильдии с локального хранилища.
        /// </summary>
        /// <param name="guild">SocketGuild</param>
        /// <returns>Guild class</returns>
        internal static Guild GetGuildAccount(IGuild guild)
        {
            try
            {
                using (var DbContext = new SqliteDbContext())
                {
                    if (DbContext.Guilds.Where(g => g.ID == guild.Id).Count() < 1)
                        return null;
                    return DbContext.Guilds.FirstOrDefault(g => g.ID == guild.Id);
                }
            }
            catch
            {

                return null;
            }
        }

        /// <summary>
        /// Создает запись о гильдии с начальными данными ID гильдии, название гильдии и ID владельца гильдии.
        /// </summary>
        /// <param name="guild">SocketGuild</param>
        /// <returns></returns>
        internal static Task CreateGuildAccount(IGuild guild)
        {
            using (SqliteDbContext context = new SqliteDbContext())
            {
                Guild new_Guild = new Guild
                {
                    ID = guild.Id,
                    Name = guild.Name,
                    OwnerId = guild.OwnerId
                };

                context.Guilds.Add(new_Guild);
                context.SaveChanges();

                Console.WriteLine($"Гильдия {guild.Name} успешно зарегистрирована.");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Обновляет запись в локальном хранилище о информационом канале гильдии.
        /// </summary>
        /// <param name="guild">SocketGuild</param>
        /// <param name="channel">ISocketMessageChannel</param>
        /// <returns></returns>
        internal static Task UpdateGuildNotificationChannel(IGuild guild, IChannel channel)
        {
            using (var Db = new SqliteDbContext())
            {
                var GuildData = Db.Guilds.First(g => g.ID == guild.Id);
                GuildData.NotificationChannel = channel.Id;
                Db.Guilds.Update(GuildData);
                Db.SaveChanges();
                Console.WriteLine($"Гильдия {guild.Name} успешно обновила канал для новостей.");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Обновляет запись в локальном хранилище о лог канале гильдии.
        /// </summary>
        /// <param name="guild">SocketGuild</param>
        /// <param name="channel">ISocketMessageChannel</param>
        /// <returns></returns>
        internal static Task UpdateGuildLoggingChannel(IGuild guild, IChannel channel)
        {
            using (var Db = new SqliteDbContext())
            {
                var GuildData = Db.Guilds.First(g => g.ID == guild.Id);
                GuildData.LoggingChannel = channel.Id;
                Db.Guilds.Update(GuildData);
                Db.SaveChanges();
                Console.WriteLine($"Гильдия {guild.Name} успешно обновила канал для логгирования.");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Обновляет запись в локальном хранилище о включенном или выключенном логировании в гильдии.
        /// </summary>
        /// <param name="guild">SocketGuild</param>
        /// <param name="channel">bool true\false</param>
        /// <returns></returns>
        internal static Task ToggleGuildLogging(IGuild guild, bool value)
        {

            using (var Db = new SqliteDbContext())
            {
                var GuildData = Db.Guilds.First(g => g.ID == guild.Id);
                GuildData.EnableLogging = value;
                Db.Guilds.Update(GuildData);
                Db.SaveChanges();
                Console.WriteLine($"Гильдия {guild.Name} включила или выключила логгирование.");
                return Task.CompletedTask;
            }
        }
        #endregion
    }
}
