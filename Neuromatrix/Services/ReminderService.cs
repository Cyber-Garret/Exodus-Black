using System;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Neuromatrix.Models;

namespace Neuromatrix.Services
{
    public class ReminderService
    {
        #region Private fields
        private readonly DiscordSocketClient _discord;
        private Timer _timer;
        #endregion

        public ReminderService(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public void Configure()
        {
            // Initialize timer for 10 sec.
            _timer = new Timer(10000);
            _timer.Elapsed += OnTimedEventAsync;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private async void OnTimedEventAsync(object sender, ElapsedEventArgs e)
        {
            // If signal time equal Friday 20:00 we will send message Xur is arrived in game.
            if (e.SignalTime.DayOfWeek == DayOfWeek.Friday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
                await XurArrived();
            // If signal time equal Tuesday 20:00 we will send message Xur is leave game.
            if (e.SignalTime.DayOfWeek == DayOfWeek.Tuesday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
                await XurLeave();
        }

        private async Task XurArrived()
        {
            SocketGuild Guild = _discord.Guilds.Where(x => x.Id == Settings.Guild).First();
            SocketTextChannel TextChannel = Guild.Channels.Where(x => x.Id == Settings.XurChannel).First() as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Gold)
                .WithTitle("Стражи! Зур прибыл в солнечную систему!")
                .WithThumbnailUrl("http://159.69.21.188/Icon/xur_emblem.png")
                .WithDescription("Нажмите на заголовок сообщения чтобы узнать точное местоположение посланника Зура.")
                .WithUrl("https://whereisxur.com/")
                .WithFooter("Напоминаю! Зур покинет солнечную систему во вторник в 20:00 по МСК.");


            await TextChannel.SendMessageAsync("@everyone", false, embed.Build());
        }
        private async Task XurLeave()
        {
            SocketGuild Guild = _discord.Guilds.Where(x => x.Id == Settings.Guild).First();
            SocketTextChannel TextChannel = Guild.Channels.Where(x => x.Id == Settings.XurChannel).First() as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder()
               .WithColor(Color.Red)
               .WithTitle("Внимание! Зур покинул солнечную систему.")
               .WithThumbnailUrl("http://159.69.21.188/Icon/xur_emblem.png")
               .WithDescription("Он просто испарился! Как только он придёт я сообщу.")
               .WithFooter("Напоминаю! В следующий раз Зур прибудет в пятницу в 20:00 по МСК.");

            await TextChannel.SendMessageAsync("@everyone", false, embed.Build());
        }
    }
}
