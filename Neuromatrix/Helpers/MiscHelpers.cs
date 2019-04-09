using Discord;
using Discord.WebSocket;

using Neuromatrix.Data;

namespace Neuromatrix.Helpers
{
    class MiscHelpers
    {
        public static EmbedBuilder WelcomeEmbed(SocketGuildUser guildUser)
        {
            var bot = Program._client.CurrentUser;
            var text = Database.GetGuildAccount(guildUser.Guild).WelcomeMessage;

            var auth = new EmbedAuthorBuilder()
            {
                IconUrl = bot.GetAvatarUrl(),
                Name = bot.Username
            };

            var embed = new EmbedBuilder()
            {
                Author = auth,
                Color = Color.Orange,
                Title = $"Добро пожаловать в гильдию {guildUser.Guild.Name}",
                Description = text
            };
            return embed;
        }
    }
}
