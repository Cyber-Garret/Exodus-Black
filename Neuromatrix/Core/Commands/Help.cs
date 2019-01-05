using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Specialized;

using Discord;
using Discord.Commands;

namespace Neuromatrix.Core.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {

        [Group("справка"), Summary("Справочная команда.")]
        public class FAQ : ModuleBase<SocketCommandContext>
        {

            [Command("")]
            public async Task MainHelp()
            {
                string mainInfo = "Доброго времени суток.\n Я Нейроматрица гильдии Адские Гончие.\n Можете звать меня просто Нейра." +
                    "Моя основная цель быстро помочь вам и дать информацию о том что есть в моей базе данных.\n" +
                    "Чтобы я знала чем именно вам помочь вы можете более конкретно задать мне команду.\n" +
                    "В данный момент в моей базе данных зарегистрированны такие команды:\n**!справка экзот**\n**!справка зур**\n**!справка валюта**\n" +
                    "Для продолжения диалога введите одну из команд представленых выше.";
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(251, 227, 103)
                    .WithDescription(mainInfo);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            [Command("экзот")]
            public async Task ExoticHelp()
            {
                StringCollection exotCol = new StringCollection();

                MethodInfo[] methods = typeof(ExoticWeapon).GetMethods();
                foreach (MethodInfo method in methods)
                {

                    object[] attrs = method.GetCustomAttributes(true);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (object attr in attrs)
                    {

                        if (attr is CommandAttribute command)
                        {
                            stringBuilder.Append($"Комманда:\n**!{command.Text}**\n");
                        }
                        if (attr is AliasAttribute alias)
                        {
                            if (alias != null)
                            {
                                string joined = string.Join(",!", alias.Aliases);
                                stringBuilder.Append($"Дополнительная комманды:\n**!{joined}**\n");
                            }
                        }
                        if (attr is SummaryAttribute summary)
                        {
                            //stringBuilder.Append($"Описание: {summary.Text}");
                            exotCol.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                    }
                }

                StringBuilder builder = new StringBuilder();
                foreach (var item in exotCol)
                {
                    builder.Append(item);
                }
                string allcommands = builder.ToString();
                try
                {
                    EmbedBuilder embed = new EmbedBuilder();

                    embed.WithColor(251, 227, 103);
                    embed.WithDescription(allcommands);
                    embed.Build();

                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            [Command("зур")]
            public async Task XurHelp()
            {

            }

            [Command("валюта")]
            public async Task DataLatticeHelp()
            {

            }
        }
    }
}
