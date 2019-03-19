using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Neuromatrix.Data;
using Neuromatrix.Extensions;
using Neuromatrix.Preconditions;

namespace Neuromatrix.Modules.Administration
{
    [Group("клан")]
    [Summary("Группа команд для управление дискорд гильдией администраторами.")]
    [Cooldown(10)]
    public class ModerationModule : BotModuleBase
    {
        #region Functions
        public static string ConvertBoolean(bool? boolean)
        {
            return boolean == true ? "**Да**" : "**Нет**";
        }
        #endregion

        [Command("")]
        [Summary("Главная команда группы клан")]
        public async Task GuildInfo()
        {
            #region Checks
            if (Context.User.IsBot) return; //Ignore bots
            if (Context.IsPrivate)
            {
                await Context.Channel.SendMessageAsync(":x: | Эта команда не доступна в личных сообщениях.");
                return;
            }

            SocketGuildUser user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder();

            if (!user.GuildPermissions.Administrator)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Прошу прощения страж {Context.User.Username}, но эта команда доступна только капитану корабля и его избранным стражам.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }
            #endregion

            #region Add guild in local storage if null
            var guild = Database.GetGuildAccount(Context.Guild);
            if (guild == null)
                await Database.CreateGuildAccount(Context.Guild);
            #endregion

            #region Data
            var app = await Context.Client.GetApplicationInfoAsync();
            #endregion

            #region Message
            embed.WithColor(Color.Orange);
            embed.WithTitle($"Приветствую капитан {Context.User.Username}");
            embed.WithDescription("Краткий ликбез о том какие команды доступны избранным стражам и капитану.");
            embed.AddField("Команда: **!клан инфо**", "Эта команда выводит мои настройки для текущей гильдии, так же соержит некоторую полезную и не очень информацию.");
            embed.AddField("Команда: **!клан новости**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда информационные сообщения например о Зуре.");
            embed.AddField("Команда: **!клан логи**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда тех. сообщения например о том что кто-то покинул сервер.");
            embed.AddField("Команда: **!клан логирование**", "Эта команда позволяет включить или выключить Тех. сообщения.");
            embed.WithFooter($"Любые предложения по улучшению или исправлении ошибок пожалуйста сообщи моему создателю {app.Owner.Username}", app.Owner.GetAvatarUrl());

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
            #endregion


        }

        [Command("инфо")]
        [Summary("Отображает все настройки бота в гильдии где была вызвана комманда.")]
        public async Task GetGuildConfig()
        {
            #region Base Checks
            if (Context.User.IsBot) return; //Ignore bots
            if (Context.IsPrivate)
            {
                await Context.Channel.SendMessageAsync(":x: | Эта команда не доступна в личных сообщениях.");
                return;
            }

            SocketGuildUser user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder();

            if (!user.GuildPermissions.Administrator)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Прошу прощения страж {Context.User.Username}, но эта команда доступна только капитану корабля и его избранным стражам.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }

            var guild = Database.GetGuildAccount(Context.Guild);

            if (guild == null)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Гильдия не найдена в базе данных, для начал введите команду !клан, для авто регистрации.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }
            #endregion

            #region Data
            var OwnerName = Context.Guild.Owner.Nickname ?? Context.Guild.Owner.Username;
            string NotificationChannel = "Не указан";
            string LogChannel = "Не указан";
            string FormattedCreatedAt = Context.Guild.CreatedAt.ToString("dd-MM-yyyy");
            string logs;
            #endregion

            #region Checks for Data
            if (guild.NotificationChannel != 0)
                NotificationChannel = Context.Guild.GetChannel(guild.NotificationChannel).Name;

            if (guild.LoggingChannel != 0)
                LogChannel = Context.Guild.GetChannel(guild.LoggingChannel).Name;

            logs = ConvertBoolean(guild.EnableLogging);
            #endregion

            #region Message
            embed.WithColor(Color.LightOrange);
            embed.WithTitle($"Мои настройки на этом корабле.");
            embed.WithThumbnailUrl(Context.Guild.IconUrl);
            embed.WithDescription($"Клан **{Context.Guild.Name}** имеет свой корабль с **{FormattedCreatedAt}**, капитан корабля в данный момент является **{OwnerName}**");
            embed.AddField("Сейчас на корабле:",
                $"Всего каналов: **{Context.Guild.Channels.Count}**\n" +
                $"Стражей на корабле: **{Context.Guild.Users.Count}**");
            embed.AddField("Новостной канал", $"В данный момент используется **{NotificationChannel}** для сообщений о Зур-е.");
            embed.AddField("Технический канал", $"В данный момент используется **{LogChannel}** для сервисных сообщений корабля");
            embed.AddField("Тех. сообщения включены?", logs);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());
            #endregion
        }

        [Command("новости")]
        [Summary("Сохраняет ID канала для использования в новостных сообщениях.")]
        public async Task SetNotificationChannel()
        {
            #region Checks
            if (Context.User.IsBot) return; //Ignore bots
            if (Context.IsPrivate)
            {
                await Context.Channel.SendMessageAsync(":x: | Эта команда не доступна в личных сообщениях.");
                return;
            }

            SocketGuildUser user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder();

            if (!user.GuildPermissions.Administrator)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Прошу прощения страж {Context.User.Username}, но эта команда доступна только капитану корабля и его избранным стражам.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }

            var guild = Database.GetGuildAccount(Context.Guild);

            if (guild == null)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Гильдия не найдена в базе данных, для начал введите команду !клан, для авто регистрации.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }
            #endregion

            #region Data
            string NotificationChannel = "Не указан";
            #endregion

            #region Checks for Data
            if (guild.NotificationChannel != 0)
                NotificationChannel = Context.Guild.GetChannel(guild.NotificationChannel).Name;
            #endregion

            #region Message
            embed.WithColor(Color.Orange);
            embed.WithTitle("Новостной канал");
            if (guild.NotificationChannel == 0)
            {
                embed.Description = $"Я заглянула в свою базу данных и оказывается у меня не записанно куда мне отправлять новости о Зур-е. :frowning: ";
            }
            else
            {
                embed.Description = $"В данный момент у меня записанно что все новости о Зур-е я должна отправлять в **{NotificationChannel}**.";
            }

            embed.WithFooter($"Хотите я запишу этот канал как новостной? Если да - нажмите {HeavyCheckMark}, если нет - нажмите {X}.");

            var message = await Context.Channel.SendMessageAsync("", embed: embed.Build());
            #endregion

            //Если true обновляем id новостного канала.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
                await Database.UpdateGuildNotificationChannel(Context.Guild, Context.Channel);
        }

        [Command("логи")]
        [Summary("Сохраняет ID канала для использования в тех сообщениях.")]
        public async Task SetLogChannel()
        {
            #region Checks
            if (Context.User.IsBot) return; //Ignore bots
            if (Context.IsPrivate)
            {
                await Context.Channel.SendMessageAsync(":x: | Эта команда не доступна в личных сообщениях.");
                return;
            }

            SocketGuildUser user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder();

            if (!user.GuildPermissions.Administrator)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Прошу прощения страж {Context.User.Username}, но эта команда доступна только капитану корабля и его избранным стражам.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }

            var guild = Database.GetGuildAccount(Context.Guild);
            if (guild == null)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Гильдия не найдена в базе данных, для начал введите команду !клан, для авто регистрации.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }
            #endregion

            #region Data
            string LogChannel = "Не указан";
            #endregion

            #region Checks for Data
            if (guild.LoggingChannel != 0)
                LogChannel = Context.Guild.GetChannel(guild.LoggingChannel).Name;
            #endregion

            #region Message
            embed.WithColor(Color.Orange);
            embed.WithTitle("Технический канал");
            if (guild.LoggingChannel == 0)
            {
                embed.Description = $"Я заглянула в свою базу данных и оказывается у меня не записанно куда мне отправлять сообщения о том когда-то вышел или кого либо обновили. :frowning: ";
            }
            else
            {
                embed.Description = $"В данный момент у меня записанно что все технические сообщения я должна отправлять в **#{LogChannel}**.";
            }
            embed.WithFooter($"Хотите я запишу этот канал как технический? Если да - нажмите {HeavyCheckMark}, если нет - нажмите {X}.");

            var message = await Context.Channel.SendMessageAsync("", embed: embed.Build());
            #endregion


            //Если true обновляем id лог канала.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
                await Database.UpdateGuildLoggingChannel(Context.Guild, Context.Channel);
        }

        [Command("логирование")]
        [Summary("Сохраняет ID канала для использования в тех. сообщениях.")]
        public async Task ToggleLogging()
        {
            #region Checks
            if (Context.User.IsBot) return; //Ignore bots
            if (Context.IsPrivate)
            {
                await Context.Channel.SendMessageAsync(":x: | Эта команда не доступна в личных сообщениях.");
                return;
            }

            SocketGuildUser user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder();

            if (!user.GuildPermissions.Administrator)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Прошу прощения страж {Context.User.Username}, но эта команда доступна только капитану корабля и его избранным стражам.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }

            var guild = Database.GetGuildAccount(Context.Guild);
            if (guild == null)
            {
                embed.WithColor(Color.Red);
                embed.Title = $":x: | Гильдия не найдена в базе данных, для начал введите команду !клан, для авто регистрации.";
                await Context.Channel.SendMessageAsync(null, false, embed.Build());
                return;
            }
            #endregion

            #region Data
            string LogChannel = "Не указан";
            string LogState;
            #endregion

            #region Checks for Data
            if (guild.LoggingChannel != 0)
                LogChannel = Context.Guild.GetChannel(guild.LoggingChannel).Name;
            LogState = ConvertBoolean(guild.EnableLogging);
            #endregion

            #region Message
            embed.Color = Color.Orange;
            embed.Title = "Технические сообщения";
            embed.Description = $"В данный момент все технические сообщения я отправляю в канал **{LogChannel}** ";

            embed.AddField("Оповещения включены?", LogState, true);
            embed.WithFooter($" Для включения - нажми {HeavyCheckMark}, для отключения - нажми {X}, или ничего не нажимай.");

            var message = await Context.Channel.SendMessageAsync("", embed: embed.Build());
            #endregion

            //Если true или false обновляем включено или выключено логирование для гильдии, в противном случае ничего не делаем.
            bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

            if (choice == true)
            {
                await Database.ToggleGuildLogging(Context.Guild, true);
            }
            else if (choice == false)
            {
                await Database.ToggleGuildLogging(Context.Guild, false);
            }

        }
    }
}
