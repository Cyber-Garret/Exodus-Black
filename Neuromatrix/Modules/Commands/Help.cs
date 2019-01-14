using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Specialized;

using Discord;
using Discord.Commands;
using Neuromatrix.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace Neuromatrix.Modules.Commands
{
    [Group("справка"), Summary("Справочная команда.")]
    public class Help : ModuleBase<SocketCommandContext>
    {
        ServiceProvider services { get; set; }

        [Command("")]
        public async Task MainHelp()
        {
            string description = $"Доброго времени суток.\n Я Нейроматрица версии {services.GetRequiredService<Settings>().Version}.\n" +
                "Моя основная цель быстро помочь вам и дать информацию о том что есть в моей базе данных.\n" +
                "Чтобы я знала чем именно вам помочь вы можете более конкретно задать мне команду.\n" +
                "В данный момент в моей базе данных зарегистрированны такие команды:\n**!справка кинетическое**\n**!справка энергетическое**\n**!справка силовое**\n**!зур**\n";

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(251, 227, 103)
                .WithDescription(description)
                .WithFooter("Для продолжения диалога введите одну из команд представленых выше.");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("кинетическое")]
        public async Task KineticHelpCommand()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(251, 227, 103)
                .WithTitle("Кинетическое вооружение")
                .WithDescription(ExotInfo(0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("энергетическое")]
        public async Task EnergyHelpCommand()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(251, 227, 103)
                .WithTitle("Энергетическое вооружение")
                .WithDescription(ExotInfo(1));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("силовое")]
        public async Task PowerHelpCommand()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(251, 227, 103)
                .WithTitle("Силовое вооружение")
                .WithDescription(ExotInfo(2));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        private string ExotInfo(int id_class)
        {
            StringCollection exotCol = new StringCollection();

            string ClassName()
            {
                string typeName = string.Empty;

                if (id_class == 0)
                    typeName = "Neuromatrix.Core.Commands.Exotic.Kinetic";
                if (id_class == 1)
                    typeName = "Neuromatrix.Core.Commands.Exotic.Energy";
                if (id_class == 2)
                    typeName = "Neuromatrix.Core.Commands.Exotic.Power";
                return typeName;

            }

            Type type = Type.GetType(ClassName());
            MethodInfo[] methods = type.GetMethods();

            foreach (MethodInfo method in methods)
            {

                object[] attrs = method.GetCustomAttributes(true);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (object attr in attrs)
                {

                    if (attr is CommandAttribute command)
                    {
                        stringBuilder.Append($"\nКомманда: **!{command.Text}**");
                    }
                    if (attr is AliasAttribute alias)
                    {
                        if (alias != null)
                        {
                            string joined = string.Join(",!", alias.Aliases);
                            stringBuilder.Append($", **!{joined}**");
                        }
                    }
                    if (attr is SummaryAttribute summary)
                    {
                        stringBuilder.Append($"\nОписание: {summary.Text}\n");
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
            return allcommands;

        }
    }
}
