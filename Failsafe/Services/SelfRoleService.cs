using Bot.Core.Data;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class SelfRoleService
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		public SelfRoleService(IServiceProvider service)
		{
			discord = service.GetRequiredService<DiscordSocketClient>();
			logger = service.GetRequiredService<ILogger<SelfRoleService>>();
		}
		public async Task SelfRoleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				var guild = GuildData.FindGuildBySelfRoleMessage(msg.Id);

				if (guild == null || guild.GuildSelfRoles.Count < 0) return;

				foreach (var selfRole in guild.GuildSelfRoles)
				{
					var emote = await discord.GetGuild(guild.Id).GetEmoteAsync(selfRole.EmoteID);
					if (reaction.Emote.Equals(emote))
					{
						var user = discord.GetGuild(guild.Id).GetUser(reaction.UserId);
						var role = discord.GetGuild(guild.Id).GetRole(selfRole.RoleID);
						await user.AddRoleAsync(role);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "SelfRoleReactionAdded");
			}
		}
		public async Task SelfRoleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				var guild = GuildData.FindGuildBySelfRoleMessage(msg.Id);

				if (guild == null || guild.GuildSelfRoles.Count < 0) return;

				foreach (var selfRole in guild.GuildSelfRoles)
				{
					var emote = await discord.GetGuild(guild.Id).GetEmoteAsync(selfRole.EmoteID);
					if (reaction.Emote.Equals(emote))
					{
						var user = discord.GetGuild(guild.Id).GetUser(reaction.UserId);
						var role = discord.GetGuild(guild.Id).GetRole(selfRole.RoleID);
						await user.RemoveRoleAsync(role);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "SelfRoleReactionRemoved");
			}
		}
	}
}
