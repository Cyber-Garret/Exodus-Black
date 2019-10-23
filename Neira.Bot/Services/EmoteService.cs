using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neira.Bot.Services
{
	public class EmoteService
	{
		public IEmote Arc;
		public IEmote Solar;
		public IEmote Void;
		private readonly DiscordSocketClient Client;

		public EmoteService(DiscordSocketClient discordSocket)
		{
			Client = discordSocket;
		}

		public void Initialize()
		{
			var NeiraHome = Client.GetGuild(521689023962415104);
			Arc = NeiraHome.Emotes.First(e => e.Name == "arc");
			Solar = NeiraHome.Emotes.First(e => e.Name == "solar");
			Void = NeiraHome.Emotes.First(e => e.Name == "void");
		}
	}
}
