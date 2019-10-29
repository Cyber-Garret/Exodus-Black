using Discord;
using Discord.WebSocket;

using System.Linq;

namespace Neira.Bot.Services
{
	public class EmoteService
	{
		public IEmote Raid;
		public IEmote Arc;
		public IEmote Solar;
		public IEmote Void;
		public IEmote Glimmer;
		private readonly DiscordSocketClient Client;

		public EmoteService(DiscordSocketClient discordSocket)
		{
			Client = discordSocket;
		}

		public void Initialize()
		{
			var NeiraHome = Client.GetGuild(Program.config.HomeDiscordServerId);

			Raid = NeiraHome.Emotes.First(e => e.Name == "Neira_Raid");
			Arc = NeiraHome.Emotes.First(e => e.Name == "arc");
			Solar = NeiraHome.Emotes.First(e => e.Name == "solar");
			Void = NeiraHome.Emotes.First(e => e.Name == "void");
			Glimmer = NeiraHome.Emotes.First(e => e.Name == "glimmer");
		}
	}
}
