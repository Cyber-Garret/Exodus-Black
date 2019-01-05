using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace Neuromatrix.Core.Commands
{
    public class Xur : ModuleBase<SocketCommandContext>
    {
        #region Global fields
        DateTime today = DateTime.Now;
        static TimeSpan start = new TimeSpan(20, 0, 0);
        static TimeSpan end = start; //setting the same value as your start and end time is same
        DateTime fridayStartCheck = new DateTime();
        DateTime tuesdayEndCheck = new DateTime();
        #endregion

        [Command("xur"), Alias("зур"), Summary("Информационная комманда о посланнике девяти по имени Зур")]
        public async Task XurCommand()
        {

            #region Check DayOfWeek
            if (today.DayOfWeek == DayOfWeek.Friday)
            {
                fridayStartCheck = DateTime.Parse(today.ToShortDateString());
                fridayStartCheck += start;
                tuesdayEndCheck = DateTime.Parse(today.AddDays(+5).ToShortDateString());
                tuesdayEndCheck += end;
            }
            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                fridayStartCheck = DateTime.Parse(today.AddDays(-1).ToShortDateString());
                fridayStartCheck += start;
                tuesdayEndCheck = DateTime.Parse(today.AddDays(+4).ToShortDateString());
                tuesdayEndCheck += end;
            }
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                fridayStartCheck = DateTime.Parse(today.AddDays(-2).ToShortDateString());
                fridayStartCheck += start;
                tuesdayEndCheck = DateTime.Parse(today.AddDays(+3).ToShortDateString());
                tuesdayEndCheck += end;
            }
            if (today.DayOfWeek == DayOfWeek.Monday)
            {
                fridayStartCheck = DateTime.Parse(today.AddDays(-3).ToShortDateString());
                fridayStartCheck += start;
                tuesdayEndCheck = DateTime.Parse(today.AddDays(+2).ToShortDateString());
                tuesdayEndCheck += end;
            }
            if (today.DayOfWeek == DayOfWeek.Tuesday)
            {
                fridayStartCheck = DateTime.Parse(today.AddDays(-5).ToShortDateString());
                fridayStartCheck += start;
                tuesdayEndCheck = DateTime.Parse(today.ToShortDateString());
                tuesdayEndCheck += end;
            }
            #endregion

            if (today >= fridayStartCheck && today <= tuesdayEndCheck)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(251, 227, 103);
                Embed.WithTitle($"Уважаемый пользователь {Context.User}");
                Embed.WithDescription("По моим данным Зур в данный момент в пределах солнечной системы, но\n" +
                    "так как мои алгоритмы глобального позиционирования пока еще в разработке, определить точное положение я не могу.\n" +
                    "[Но я уверена что тут ты сможешь отыскать его положение](https://whereisxur.com/)");
                await Context.Channel.SendMessageAsync("", false, Embed.Build());
            }
            else
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(219, 66, 55);
                Embed.WithDescription("По моим данным Зур не был замечен где-либо,\n" +
                    "я устала вам людишкам, повторять из дня в день что он прилетает в пятницу 20:00 по московскому времени и исчезает неизвестным мне образом\n" +
                    "во вторник 20:00 по московскому времени.");
                await Context.Channel.SendMessageAsync("", false, Embed.Build());
            }
        }

        //[Command("xur"), Alias("зур"), Summary("Информационная комманда о посланнике девяти по имени Зур")]
        //public async Task Embed([Remainder]string Input = "None")
        //{
        //    EmbedBuilder Embed = new EmbedBuilder();
        //    Embed.WithAuthor("Test Embed", Context.User.GetAvatarUrl());
        //    Embed.WithColor(254, 240, 101);
        //    Embed.WithFooter("This is footer!", Context.Guild.Owner.GetAvatarUrl());
        //    Embed.WithDescription("This is **test** description with cool link.\n" +
        //        "[Зура можно найти тут](https://whereisxur.com/)");
        //    Embed.AddInlineField("User Input: ", Input);
        //    await Context.Channel.SendMessageAsync("", false, Embed.Build());
        //}
    }
}
