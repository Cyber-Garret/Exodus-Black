using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DiscordBot.Data;
using DiscordBot.Helpers;
using DiscordBot.Models.Db;
using DiscordBot.Extensions;
using DiscordBot.Preconditions;

namespace DiscordBot.Modules.Administration
{
    public class ModerationModule : BotModuleBase
    {
        #region Functions
        public static string ConvertBoolean(bool? boolean)
        {
            return boolean == true ? "**Да**" : "**Нет**";
        }
        #endregion

        [Command("clan")]
        [Summary("Информационная справка о доступных командах администраторам клана.")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task GuildInfo()
        {
            // Get some bot info
            var app = await Context.Client.GetApplicationInfoAsync();

            #region Message
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle($"Приветствую страж {Context.User.Username}")
                .WithDescription("Краткий ликбез о том какие команды доступны избранным стражам и капитану.")
                .AddField("Команда: **!clan info**", "Эта команда выводит мои настройки для текущей гильдии, так же содержит некоторую полезную и не очень информацию.")
                .AddField("Команда: **!news**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда информационные сообщения например о Зуре.")
                .AddField("Команда: **!switch news**", "Эта команда позволяет включить или выключить оповещения о Зур-е")
                .AddField("Команда: **!logs**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда сервисные сообщения например о том что кто-то покинул сервер.")
                .AddField("Команда: **!switch logs**", "Эта команда позволяет включить или выключить Тех. сообщения.")
                .AddField("Команда: **!preview welcome**", "Позволяет посмотреть, как будет выглядеть сообщение-приветствие новоприбывшему на сервер.")
                .AddField("Команда: **!save welcome <Message>**", "Сохраняет сообщение-приветствие и включает механизм отправки сообщения.\nПоддерживает синтаксис MarkDown для красивого оформления.")
                .WithFooter($"Любые предложения по улучшению или исправлении ошибок пожалуйста сообщи моему создателю {app.Owner.Username}#{app.Owner.Discriminator}", app.Owner.GetAvatarUrl());
            #endregion

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("clan info")]
        [Summary("Отображает все настройки бота в гильдии где была вызвана комманда.")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task GetGuildConfig()
        {
            // Get or create personal guild settings
            Guild guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            #region Data
            var OwnerName = Context.Guild.Owner.Nickname ?? Context.Guild.Owner.Username;
            string NotificationChannel = "Не указан";
            string LogChannel = "Не указан";
            string FormattedCreatedAt = Context.Guild.CreatedAt.ToString("dd-MM-yyyy");
            string logs = ConvertBoolean(guild.EnableLogging);
            string news = ConvertBoolean(guild.EnableNotification);
            #endregion

            #region Checks for Data
            if (guild.NotificationChannel != 0)
                NotificationChannel = Context.Guild.GetChannel(guild.NotificationChannel).Name;

            if (guild.LoggingChannel != 0)
                LogChannel = Context.Guild.GetChannel(guild.LoggingChannel).Name;
            #endregion

            #region Message
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle($"Мои настройки на этом корабле.")
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription($"Клан **{Context.Guild.Name}** имеет свой корабль с **{FormattedCreatedAt}**, капитаном корабля в данный момент является **{OwnerName}**")
                .AddField("Сейчас на корабле:",
                $"Всего каналов: **{Context.Guild.Channels.Count}**\n" +
                $"Стражей на корабле: **{Context.Guild.Users.Count}**")
                .AddField("Новостной канал", $"В данный момент используется **{NotificationChannel}** для сообщений о Зур-е.")
                .AddField("Оповещения о Зур-е включены?", news)
                .AddField("Технический канал", $"В данный момент используется **{LogChannel}** для сервисных сообщений клана")
                .AddField("Тех. сообщения включены?", logs);
            #endregion

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
        }

        [Command("news")]
        [Summary("Сохраняет ID канала для использования в новостных сообщениях.")]
        [Cooldown(5)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task SetNotificationChannel()
        {
            // Get or create personal guild settings
            Guild guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            string NotificationChannel = "Не указан";
            string news = ConvertBoolean(guild.EnableNotification);

            //Get notification channel name
            if (guild.NotificationChannel != 0)
                NotificationChannel = Context.Guild.GetChannel(guild.NotificationChannel).Name;

            #region Message
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("Новостной канал");
            if (guild.NotificationChannel == 0)
            {
                embed.Description = $"Я заглянула в свою базу данных и оказывается у меня не записанно куда мне отправлять новости о Зур-е. :frowning: ";
            }
            else
            {
                embed.Description = $"В данный момент у меня записанно что все новости о Зур-е я должна отправлять в **{NotificationChannel}**.";
            }
            embed.AddField("Оповещения о Зур-е включены?", news);
            embed.WithFooter($"Хотите я запишу этот канал как новостной? Если да - нажмите {HeavyCheckMark}, если нет - нажмите {X}.");
            #endregion

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());

            //Если true обновляем id новостного канала.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
            {
                guild.NotificationChannel = Context.Channel.Id;
                await Database.SaveGuildAccountAsync(Context.Guild, guild);
            }
        }

        [Command("logs")]
        [Summary("Сохраняет ID канала для использования в тех сообщениях.")]
        [Cooldown(5)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task SetLogChannel()
        {
            // Get or create personal guild settings
            Guild guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            string LogChannel = "Не указан";
            string logs = ConvertBoolean(guild.EnableLogging);

            //Get logging channel name
            if (guild.LoggingChannel != 0)
                LogChannel = Context.Guild.GetChannel(guild.LoggingChannel).Name;

            #region Message
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("Технический канал");
            if (guild.LoggingChannel == 0)
            {
                embed.Description = $"Я заглянула в свою базу данных и оказывается у меня не записанно куда мне отправлять сервисные сообщения. :frowning: ";
            }
            else
            {
                embed.Description = $"В данный момент у меня записанно что все сервисные сообщения я должна отправлять в **#{LogChannel}**.";
            }
            embed.AddField("Cервисные сообщения включены?", logs);
            embed.WithFooter($"Хотите я запишу этот канал как технический? Если да - нажмите {HeavyCheckMark}, если нет - нажмите {X}.");
            #endregion

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());

            //Если true обновляем id лог канала.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
            {
                guild.LoggingChannel = Context.Channel.Id;
                await Database.SaveGuildAccountAsync(Context.Guild, guild);
            }
        }

        [Command("switch logs")]
        [Summary("Вкл. или Выкл. тех. сообщения.")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task ToggleLogging()
        {
            // Get or create personal guild settings
            Guild guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            string LogChannel = "Не указан";
            string logs = ConvertBoolean(guild.EnableLogging);

            //Get logging channel name
            if (guild.LoggingChannel != 0)
                LogChannel = Context.Guild.GetChannel(guild.LoggingChannel).Name;

            #region Message
            EmbedBuilder embed = new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithTitle("Технические сообщения")
            .WithDescription($"В данный момент все технические сообщения я отправляю в канал **{LogChannel}**")
            .AddField("Оповещения включены?", logs, true)
            .WithFooter($" Для включения - нажми {HeavyCheckMark}, для отключения - нажми {X}, или ничего не нажимай.");
            #endregion

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());

            //Если true или false обновляем включено или выключено логирование для гильдии, в противном случае ничего не делаем.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
            {
                guild.EnableLogging = true;
                await Database.SaveGuildAccountAsync(Context.Guild, guild);
            }
            else if (choice == false)
            {
                guild.EnableLogging = false;
                await Database.SaveGuildAccountAsync(Context.Guild, guild);
            }

        }

        [Command("switch news")]
        [Summary("Включает или выключает новости о зуре")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task ToggleNews()
        {
            // Get or create personal guild settings
            Guild guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            string NewsChannel = "Не указан";
            string news = ConvertBoolean(guild.EnableNotification);

            //Get notification channel name
            if (guild.NotificationChannel != 0)
                NewsChannel = Context.Guild.GetChannel(guild.NotificationChannel).Name;

            #region Message
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("Новостные сообщения")
                .WithDescription($"В данный момент все новостные сообщения о Зур-е я отправляю в канал **{NewsChannel}**")
                .AddField("Оповещения включены?", news, true)
                .WithFooter($" Для включения - нажми {HeavyCheckMark}, для отключения - нажми {X}, или ничего не нажимай.");
            #endregion

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());

            //Если true или false обновляем включено или выключено логирование для гильдии, в противном случае ничего не делаем.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
            {
                guild.EnableNotification = true;
                await Database.SaveGuildAccountAsync(Context.Guild, guild);
            }
            else if (choice == false)
            {
                guild.EnableNotification = false;
                await Database.SaveGuildAccountAsync(Context.Guild, guild);
            }
        }

        [Command("preview welcome")]
        [Summary("Команда предпросмотра приветственного сообщения")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task WelcomeMessagePreview()
        {
            // Get or create personal guild settings
            var guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            if (string.IsNullOrWhiteSpace(guild.WelcomeMessage))
            {
                await Context.Channel.SendMessageAsync($":x: | В данный момент я не отправляю какое либо сообщение новоприбывшим. Для добавления или редактирования сообщения отправь команду **!сохранить приветствие <текст сообщения>**");
                return;
            }

            await Context.Channel.SendMessageAsync($"{Context.User.Mention} вот так выглядит сообщение для новоприбывших в Discord.", embed: MiscHelpers.WelcomeEmbed(Context.Guild.CurrentUser).Build());
        }

        [Command("save welcome")]
        [Summary("Сохраняет сообщение для отправки всем кто пришел в гильдию")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator,
            ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
            NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
        public async Task SaveWelcomeMessage([Remainder]string message)
        {
            // Get or create personal guild settings
            var guild = Database.GetGuildAccountAsync(Context.Guild).Result;

            //Dont save empty welcome message.
            if (string.IsNullOrWhiteSpace(message)) return;
            
            guild.WelcomeMessage = message;

            await Database.SaveGuildAccountAsync(Context.Guild, guild);

            await Context.Channel.SendMessageAsync(":smiley: Приветственное сообщение сохранено.");
        }
    }
}
