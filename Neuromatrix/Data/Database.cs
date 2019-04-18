using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Discord;

using DiscordBot.Models.Db;

namespace DiscordBot.Data
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
        /// Возвращает список всех гильдий.
        /// </summary>
        /// <returns>IEnumerable<Guild></returns>
        internal static async Task<IEnumerable<Guild>> GetAllGuildsAsync()
        {
            using (SqliteDbContext dbContext = new SqliteDbContext())
            {
                IEnumerable<Guild> guilds = await dbContext.Guilds.ToListAsync();
                return guilds;
            }
        }

        /// <summary>
        /// Возвращает данные о гильдии, если нет данных создает запись.
        /// </summary>
        /// <param name="guild">SocketGuild</param>
        /// <returns>Guild class</returns>
        internal static async Task<Guild> GetGuildAccountAsync(IGuild guild)
        {
            using (var ctx = new SqliteDbContext())
            {
                if (ctx.Guilds.Where(G => G.ID == guild.Id).Count() < 1)
                {
                    var newGuild = new Guild
                    {
                        ID = guild.Id
                    };

                    ctx.Guilds.Add(newGuild);
                    await ctx.SaveChangesAsync();

                    return newGuild;
                }
                else
                {
                    return ctx.Guilds.SingleOrDefault(G => G.ID == guild.Id);
                }
            }
        }


        internal static Task SaveGuildAccountAsync(IGuild guild, Guild guildAccount)
        {
            using (var ctx = new SqliteDbContext())
            {
                if (ctx.Guilds.Where(G => G.ID == guild.Id).Count() < 1)
                {
                    var newGuild = new Guild
                    {
                        ID = guild.Id
                    };

                    ctx.Guilds.Add(newGuild);
                    ctx.SaveChangesAsync();

                    return Task.CompletedTask;
                }
                else
                {
                    //ctx.Guilds.Add(guildAccount);
                    ctx.Entry(guildAccount).State = EntityState.Modified;
                    ctx.SaveChangesAsync();
                    return Task.CompletedTask;
                }
            }
        }

        internal static Task SaveWelcomeMessage(IGuild guild, string value)
        {

            using (var Db = new SqliteDbContext())
            {
                var GuildData = Db.Guilds.First(g => g.ID == guild.Id);
                GuildData.WelcomeMessage = value;
                Db.Guilds.Update(GuildData);
                Db.SaveChanges();
                Console.WriteLine($"Гильдия {guild.Name} сохранила сообщение.");
                return Task.CompletedTask;
            }
        }
        #endregion
    }
}
