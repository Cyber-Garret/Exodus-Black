using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Neuromatrix.Preconditions;

namespace Neuromatrix.Modules.Commands
{
    public class HelpModule : BotModuleBase
    {
        [Command("справка")]
        [Summary("Основная справочная команда.")]
        [Cooldown(10)]
        public async Task MainHelp()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Gold);
            embedBuilder.WithDescription(
                $"Доброго времени суток. Я Нейроматрица версии {Settings.Version}.\n" +
                "Моя основная цель своевременно сообщать когда прибывает или улетает посланник девяти Зур.\n" +
                "Так же я могу предоставить информацию о экзотическом вооружении.\n" +
                "В текущий момент в моей базе данных зарегистрированны такие команды:");
            embedBuilder.AddField(
                "Команда: **!инфо [название снаряжения]**",
                "Эта команда несет полную информацию о экзотическом вооружении представленном в игре.\n" +
                "Синтаксис довольно простой, можно искать как по полному названию так и частичному.\n" +
                "Например: **!инфо дело** предоставит информацию об автомате Милое Дело.");
            embedBuilder.AddField(
                "Команда: **!зур**",
                "Команда отображает находится ли в данный момент Зур в игре или нет.");
            embedBuilder.WithFooter("Для дальнейшей работы введите одну из команд представленых выше.");

            await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }
    }
}
