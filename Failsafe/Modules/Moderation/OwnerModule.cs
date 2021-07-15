using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Preconditions;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Failsafe.Modules.Moderation
{
    [TeamMember]
    public class GuildOwnerModule : RootModule
    {
        private readonly DiscordSocketClient _discord;

        public GuildOwnerModule(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        [Command("guilds")]
        [Alias("гильдии")]
        public async Task Guilds()
        {
            var guilds = _discord.Guilds.OrderBy(o => o.Users.Count).Take(10);
            var message = guilds.Aggregate("**Smallest guilds:**\n", (current, guild) => current + $"{guild.Name}({guild.Id}) - Users: {guild.Users.Count}\n");
            await ReplyAsync(message);
        }

        [Command("info")]
        [Alias("инфо")]
        public async Task UpTime()
        {
            var proc = Process.GetCurrentProcess();

            var mem = proc.WorkingSet64 / 1000000;
            var threads = proc.Threads;
            var time = DateTime.Now - proc.StartTime;
            var cpu = proc.TotalProcessorTime.TotalMilliseconds / proc.PrivilegedProcessorTime.TotalMilliseconds;


            var sw = Stopwatch.StartNew();
            sw.Stop();

            var embed = new EmbedBuilder();
            embed.WithColor(37, 152, 255);
            embed.WithDescription(
                $"Your ping: {(int)sw.Elapsed.TotalMilliseconds}ms\n" +
                $"Runtime: {time.Hours}h:{time.Minutes}m\n" +
                $"CPU usage: {cpu:n0}\n" +
                $"Memory: {mem:n0}Mb\n" +
                $"Heap size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}\n" +
                $"Threads using: {threads.Count}\n");

            embed.AddField("Bot Statistics:",
                $"Серверов: {Context.Client.Guilds.Count}\n" +
                $"Пользователей: {Context.Client.Guilds.Sum(x => x.Users.Count)}");
            await ReplyAsync(embed: embed.Build());


        }

        [Command("leave")]
        [Alias("выйти")]
        public async Task Leave(ulong guildId)
        {
            var guild = _discord.GetGuild(guildId);
            await guild.LeaveAsync();
            await ReplyAsync($"{guild.Name} will be abandoned. Now im on {_discord.Guilds.Count} guilds");
        }
    }
}
