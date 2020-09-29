using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;

namespace Bot.Services
{
	public class EmoteService
	{
		public IEmote Raid;
		public IEmote Arc;
		public IEmote Solar;
		public IEmote Void;

		private readonly DiscordSocketClient discord;
		private readonly IConfiguration config;
		private readonly ILogger logger;

		public EmoteService(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<EmoteService>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			config = service.GetRequiredService<IConfiguration>();
		}

		public void Configure()
		{
			try
			{
				var HomeGuild = discord.GetGuild(config.GetValue<ulong>("Bot:HomeGuild"));
				//Raid and milestone
				Raid = HomeGuild.Emotes.First(e => e.Name == "Neira_Raid");
				//Elemens
				Arc = HomeGuild.Emotes.First(e => e.Name == "arc");
				Solar = HomeGuild.Emotes.First(e => e.Name == "solar");
				Void = HomeGuild.Emotes.First(e => e.Name == "void");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Configure Emote");
				throw;
			}
		}
	}
}
