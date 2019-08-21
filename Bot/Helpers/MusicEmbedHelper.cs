using Discord;
using Discord.WebSocket;

using System.Threading.Tasks;

namespace Bot.Helpers
{
	internal static class MusicEmbedHelper
	{
		public static Embed CreateEmbed(string title, string body, EmbedMessageType type, SocketUser target)
		{
			var embed = new EmbedBuilder();
			var thumbnailUrl = target.GetAvatarUrl();
			var auth = new EmbedAuthorBuilder()
			{
				Name = target.Username,
				IconUrl = thumbnailUrl,
			};
			embed.WithAuthor(auth);
			embed.WithTitle(title);
			embed.WithDescription(body);

			switch (type)
			{
				case EmbedMessageType.Info:
					embed.WithColor(Color.Blue);
					break;
				case EmbedMessageType.Success:
					embed.WithColor(Color.Green);
					break;
				case EmbedMessageType.Error:
					embed.WithColor(Color.Red);
					break;
				case EmbedMessageType.Exception:
					embed.WithColor(Color.DarkRed);
					break;
				default:
					embed.WithColor(Color.Gold);
					break;
			}

			embed.WithCurrentTimestamp();

			return embed.Build();
		}


		public static async Task<Embed> CreateBasicEmbed(string title = null, string description = null, string footer = null)
		{
			var embed = await Task.Run(() => (new EmbedBuilder()
				.WithTitle(title)
				.WithDescription(description)
				.WithFooter(footer)
				.WithColor(Color.Gold).Build()));
			return embed;
		}

		public static async Task<Embed> CreateMusicEmbed(string title, string description, string footer = null)
		{
			var embed = await Task.Run(() => (new EmbedBuilder()
				.WithTitle(title)
				.WithDescription(description)
				.WithFooter(footer)
				.WithColor(Color.Blue)
				.WithCurrentTimestamp().Build()));
			return embed;
		}

		public static async Task<Embed> CreateErrorEmbed(string source, string error, string footer = null)
		{
			var embed = await Task.Run(() => new EmbedBuilder()
				.WithTitle(source)
				.WithDescription($"**Ошибка:** {error}")
				.WithFooter(footer)
				.WithColor(Color.Red).Build());
			return embed;
		}

		public enum EmbedMessageType
		{
			Success = 0,
			Info = 10,
			Error = 20,
			Exception = 30
		}
	}
}
