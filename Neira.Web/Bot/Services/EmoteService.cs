using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neira.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.Bot.Services
{
	public class EmoteService
	{
		public IEmote Raid;
		public IEmote Arc;
		public IEmote Solar;
		public IEmote Void;
		public IEmote Glimmer;

		public IEmote ExoticEngram;
		public IEmote LegendaryEngram;
		public IEmote RareEngram;
		public IEmote UncommonEngram;
		public IEmote CommonEngram;
		public IEmote ReactSecond => new Emoji("2\u20e3");
		public IEmote ReactThird => new Emoji("3\u20e3");
		public IEmote ReactFourth => new Emoji("4\u20e3");
		public IEmote ReactFifth => new Emoji("5\u20e3");
		public IEmote ReactSixth => new Emoji("6\u20e3");

		private readonly DiscordSocketClient _client;
		private readonly Config _config;

		public EmoteService(IServiceProvider service)
		{
			_client = service.GetRequiredService<DiscordSocketClient>();
			_config = service.GetRequiredService<IOptions<Config>>().Value;
		}

		public void Configure()
		{
			var NeiraHome = _client.GetGuild(_config.NeiraHomeServerId);
			//Raid and milestone
			Raid = NeiraHome.Emotes.First(e => e.Name == "Neira_Raid");
			//Elemens
			Arc = NeiraHome.Emotes.First(e => e.Name == "arc");
			Solar = NeiraHome.Emotes.First(e => e.Name == "solar");
			Void = NeiraHome.Emotes.First(e => e.Name == "void");
			//Global Currency
			Glimmer = NeiraHome.Emotes.First(e => e.Name == "glimmer");
			//Engrams
			ExoticEngram = NeiraHome.Emotes.First(e => e.Name == "Exotic_Engram");
			LegendaryEngram = NeiraHome.Emotes.First(e => e.Name == "Legendary_Engram");
			RareEngram = NeiraHome.Emotes.First(e => e.Name == "Rare_Engram");
			UncommonEngram = NeiraHome.Emotes.First(e => e.Name == "Uncommon_Engram");
			CommonEngram = NeiraHome.Emotes.First(e => e.Name == "Common_Engram");
		}
	}
}
