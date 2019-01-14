using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Neuromatrix.Models;

namespace Neuromatrix.Modules.Commands
{
    [Group("справка"), Summary("Справочная группа команд.")]
    public class HelpModule : NeiraModuleBase
    {

        [Command("")]
        public async Task MainHelp(Optional<CommandInfo> command)
        {
            string description = $"Доброго времени суток.\n Я Нейроматрица версии {Configuration.Version}.\n" +
                "Моя основная цель быстро помочь вам и дать информацию о том что есть в моей базе данных.\n" +
                "Чтобы я знала чем именно вам помочь вы можете более конкретно задать мне команду.\n" +
                "В данный момент в моей базе данных зарегистрированны такие команды:\n**!справка кинетическое**\n**!справка энергетическое**\n**!справка силовое**\n**!зур**\n";
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(251, 227, 103)
                .WithDescription(description)
                .WithFooter("Для продолжения диалога введите одну из команд представленых выше.");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
