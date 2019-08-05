using Discord;
using Discord.WebSocket;

using Core;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Helpers
{
	class MiscHelpers
	{
		public static EmbedBuilder WelcomeEmbed(SocketGuildUser guildUser)
		{
			string text = FailsafeDbOperations.GetGuildAccountAsync(guildUser.Guild.Id).Result.WelcomeMessage;

			var embed = new EmbedBuilder()
			{
				Color = Color.Orange,
				Title = $"Добро пожаловать на сервер {guildUser.Guild.Name}",
				Description = text
			};
			//if guild have picture add to message.
			if (guildUser.Guild.IconUrl != string.Empty)
				embed.ThumbnailUrl = guildUser.Guild.IconUrl;

			return embed;
		}

		public static async Task Autorole(SocketGuildUser user)
		{
			var guild = await FailsafeDbOperations.GetGuildAccountAsync(user.Guild.Id);
			if (guild.AutoroleID != 0)
			{
				var targetRole = user.Guild.Roles.FirstOrDefault(r => r.Id == guild.AutoroleID);
				if (targetRole != null)
					await user.AddRoleAsync(targetRole);
			}
		}
	}
}
