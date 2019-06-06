using Discord;
using Discord.WebSocket;

using Core;

namespace DiscordBot.Helpers
{
	class MiscHelpers
	{
		public static EmbedBuilder WelcomeEmbed(SocketGuildUser guildUser)
		{
			string text = FailsafeDbOperations.GetGuildAccountAsync(guildUser.Guild.Id).Result.WelcomeMessage;

			var embed = new EmbedBuilder()
			{
				Color = Color.Orange,
				Title = $"Добро пожаловать на сервер клана {guildUser.Guild.Name} в Destiny 2",
				Description = text
			};
			//if guild have picture add to message.
			if (guildUser.Guild.IconUrl != string.Empty)
				embed.ThumbnailUrl = guildUser.Guild.IconUrl;

			return embed;
		}
	}
}
