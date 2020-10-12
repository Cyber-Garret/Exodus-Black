using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Properties;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Failsafe.Modules.Moderation
{
	public class GuildOwnerModule : RootModule
	{
		private readonly ulong botOwner;
		private readonly ILogger<GuildOwnerModule> _logger;
		private readonly DiscordSocketClient _discord;

		public GuildOwnerModule(IConfiguration configuration, ILogger<GuildOwnerModule> logger, DiscordSocketClient discord)
		{
			botOwner = configuration.GetValue<ulong>("Bot:Owner");
			_logger = logger;
			_discord = discord;
		}

		[Command("guilds"), Alias("гильдии", "гільдії")]
		public async Task Guilds()
		{
			if (Context.User.Id != botOwner)
				return;

			var guilds = _discord.Guilds.OrderBy(o => o.Users.Count).Take(10);
			var message = guilds.Aggregate("**Smallest guilds:**\n", (current, guild) => current + $"{guild.Name}({guild.Id}) - Users: {guild.Users.Count}\n");
			await ReplyAsync(message);
		}

		[Command("leave"), Alias("выйти", "покинути")]
		public async Task Leave(ulong guildId)
		{
			if (Context.User.Id != botOwner)
				return;

			var guild = _discord.GetGuild(guildId);
			await guild.LeaveAsync();
			await ReplyAsync($"{guild.Name} will be abandoned. Now im on {_discord.Guilds.Count} guilds");
		}
	}
}
