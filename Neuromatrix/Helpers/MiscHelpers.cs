using Discord;
using Discord.WebSocket;

using Neuromatrix.Data;

namespace Neuromatrix.Helpers
{
    class MiscHelpers
    {
        public static EmbedBuilder WelcomeEmbed(SocketGuildUser guildUser)
        {
            string text = Database.GetGuildAccountAsync(guildUser.Guild).Result.WelcomeMessage;
            
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
