using Discord;
using Discord.WebSocket;
using Neuromatrix.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Neuromatrix
{
    public class ProgramOld
    {
        #region Private fields
        private Timer timer;
        #endregion

        /// <summary>
        /// Таймер для Зура.
        /// </summary>
        /// <returns></returns>
        private Task XurReminder()
        {
            // Создаем таймер с интервалом в 10 секунд.
            timer = new Timer(10000);
            // Привязываем Elapsed event к таймеру. 
            timer.Elapsed += OnTimedEventAsync;
            timer.AutoReset = true;
            timer.Enabled = true;

            return Task.CompletedTask;
        }

        private async void OnTimedEventAsync(object sender, ElapsedEventArgs e)
        {
            // Если пятница 20:00 отправляем сообщение что Зур пришёл. DateTime.Now.Second < 10 помогает вызвать 1 раз, а не каждые 10 секунд в течении минуты.
            if (e.SignalTime.DayOfWeek == DayOfWeek.Friday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
                //await XurArrived();
                // Если вторник 20:00 отправляем сообщение что Зур ушёл. DateTime.Now.Second < 10 помогает вызвать 1 раз, а не каждые 10 секунд в течении минуты.
                if (e.SignalTime.DayOfWeek == DayOfWeek.Tuesday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10) ;
                //await XurLeave();
        }

        //private async Task XurArrived()
        //{
        //    SocketGuild Guild = Client.Guilds.Where(x => x.Id == StaticSettings.Guild).First();
        //    SocketTextChannel TextChannel = Guild.Channels.Where(x => x.Id == StaticSettings.XurChannel).First() as SocketTextChannel;

        //    EmbedBuilder embed = new EmbedBuilder()
        //        .WithColor(251, 227, 103)
        //        .WithTitle("Стражи! Зур прибыл в солнечную систему!")
        //        .WithThumbnailUrl("http://159.69.21.188/Icon/xur_emblem.png")
        //        .WithDescription("Нажмите на заголовок сообщения чтобы узнать точное местоположение посланника Зура.")
        //        .WithUrl("https://whereisxur.com/")
        //        .WithFooter("Напоминаю! Зур покинет солнечную систему во вторник в 20:00 по МСК.");


        //    await TextChannel.SendMessageAsync("@everyone", false, embed.Build());
        //}
        //private async Task XurLeave()
        //{
        //    SocketGuild Guild = Client.Guilds.Where(x => x.Id == StaticSettings.Guild).First();
        //    SocketTextChannel TextChannel = Guild.Channels.Where(x => x.Id == StaticSettings.XurChannel).First() as SocketTextChannel;

        //    EmbedBuilder embed = new EmbedBuilder()
        //       .WithColor(219, 66, 55)
        //       .WithTitle("Внимание! Зур покинул солнечную систему!")
        //       .WithThumbnailUrl("http://159.69.21.188/Icon/xur_emblem.png")
        //       .WithDescription("Он просто испарился! Как только он прийдет я сообщу.")
        //       .WithFooter("Напоминаю! В следующий раз Зур прибудет в пятницу в 20:00 по МСК.");

        //    await TextChannel.SendMessageAsync("@everyone", false, embed.Build());
        //}
    }
}
