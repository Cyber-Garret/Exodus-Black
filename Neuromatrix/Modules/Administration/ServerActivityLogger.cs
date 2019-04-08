using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Neuromatrix.Data;

namespace Neuromatrix.Modules.Administration
{
    public class ServerActivityLogger
    {
        #region Private Fields
        private readonly DiscordShardedClient _client = Program._client;
        private readonly IServiceProvider _services;
        #endregion


        public ServerActivityLogger(IServiceProvider services)
        {
            _services = services;
        }

        #region Methods
        public async Task ChannelCreated(IChannel arg)
        {
            try
            {
                #region Checks
                if (!(arg is ITextChannel channel))
                    return;
                #endregion

                #region Data
                var log = await channel.Guild.GetAuditLogsAsync(1);
                var audit = log.ToList();
                var name = audit[0].Action == ActionType.ChannelCreated ? audit[0].User.Username : "Неизвестно";
                var auditLogData = audit[0].Data as ChannelCreateAuditLogData;
                var embed = new EmbedBuilder();
                #endregion

                #region Message
                embed.WithColor(Color.Orange);
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.AddField("📖 Создан канал",
                    $"Название: **{arg.Name}**\n" +
                    $"Тип канала: **{auditLogData?.ChannelType.ToString()}**\n" +
                    $"NSFW **{channel.IsNsfw}**");
                //$"Категория: {channel.GetCategoryAsync().Result.Name}\n" +
                embed.WithFooter($"Кто создавал: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
                #endregion

                var currentIGuildChannel = (IGuildChannel)arg;
                var guild = Database.GetGuildAccount(currentIGuildChannel.Guild);
                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync(null, false, embed.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }

        }

        public async Task ChannelDestroyed(IChannel arg)
        {
            try
            {
                #region Checks
                if (!(arg is ITextChannel channel))
                    return;
                #endregion

                #region Data
                var log = await channel.Guild.GetAuditLogsAsync(1);
                var audit = log.ToList();
                var name = audit[0].Action == ActionType.ChannelDeleted ? audit[0].User.Username : "Неизвестно";
                var auditLogData = audit[0].Data as ChannelDeleteAuditLogData;
                var embed = new EmbedBuilder();
                #endregion

                #region Message
                embed.WithColor(Color.Red);
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.AddField("❌ Удален канал",
                    $"Название канала: **{arg.Name}**\n" +
                    $"Тип: **{auditLogData?.ChannelType}**\n" +
                    $"NSFW: **{channel.IsNsfw}**");
                //$"Категория: {channel.GetCategoryAsync().Result.Name}\n" +
                embed.WithFooter($"Кто удалял: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
                #endregion

                if (arg is IGuildChannel currentIguildChannel)
                {
                    var guild = Database.GetGuildAccount(currentIguildChannel.Guild);
                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync(null, false, embed.Build());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }
        }

        public async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            try
            {
                #region Checks
                if (after == null || before == after || before.IsBot)
                    return;
                #endregion

                #region Data
                var guild = Database.GetGuildAccount(before.Guild);
                #endregion

                #region Different Messages 
                if (before.Nickname != after.Nickname)
                {
                    #region Data
                    var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
                    var audit = log.ToList();
                    var beforeName = before.Nickname ?? before.Username;
                    var afterName = after.Nickname ?? after.Username;
                    var embed = new EmbedBuilder();
                    #endregion

                    #region Message
                    embed.WithColor(Color.Orange);
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl() ?? after.GetDefaultAvatarUrl()}");
                    embed.AddField("💢 Имя стража изменено:",
                        $"Предыдущее имя:\n" +
                        $"**{beforeName}**\n" +
                        $"Новое имя:\n" +
                        $"**{afterName}**");
                    if (audit[0].Action == ActionType.MemberUpdated)
                    {
                        var name = audit[0].User.Username ?? "Неизвестно";
                        embed.WithFooter($"Кем изменено: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
                    }
                    #endregion

                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync(null, false, embed.Build());
                    }
                }

                if (before.Roles.Count != after.Roles.Count)
                {
                    #region Data
                    string roleString;
                    var list1 = before.Roles.ToList();
                    var list2 = after.Roles.ToList();
                    var role = "";
                    var embed = new EmbedBuilder();
                    if (before.Roles.Count > after.Roles.Count)
                    {
                        embed.WithColor(Color.Red);
                        roleString = "Убрана";
                        var differenceQuery = list1.Except(list2);
                        var socketRoles = differenceQuery as SocketRole[] ?? differenceQuery.ToArray();
                        for (var i = 0; i < socketRoles.Count(); i++)
                            role += socketRoles[i];
                    }
                    else
                    {
                        embed.WithColor(Color.Orange);
                        roleString = "Добавлена";
                        var differenceQuery = list2.Except(list1);
                        var socketRoles = differenceQuery as SocketRole[] ?? differenceQuery.ToArray();
                        for (var i = 0; i < socketRoles.Count(); i++)
                            role += socketRoles[i];
                    }

                    var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
                    var audit = log.ToList();
                    #endregion

                    #region Message
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl() ?? after.GetDefaultAvatarUrl()}");
                    embed.AddField($"🔑 Обновлена роль стража:",
                        $"Имя: **{before.Nickname ?? before.Username}**\n" +
                        $"{roleString} роль: **{role}**");
                    if (audit[0].Action == ActionType.MemberRoleUpdated)
                    {
                        var name = audit[0].User.Username ?? "Неизвестно";
                        embed.WithFooter($"Кто обновлял: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
                    }
                    #endregion

                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync(null, false, embed.Build());
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }

        }

        public async Task MessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel arg3)
        {
            try
            {
                if (arg3 is IGuildChannel currentIGuildChannel)
                {
                    var guild = Database.GetGuildAccount(currentIGuildChannel.Guild);
                    if (messageAfter.Author.IsBot)
                        return;

                    var after = messageAfter as IUserMessage;

                    if (messageAfter.Content == null)
                    {
                        return;
                    }

                    if (!((messageBefore.HasValue ? messageBefore.Value : null) is IUserMessage before))
                        return;


                    if (before.Content == after?.Content)
                        return;


                    var embed = new EmbedBuilder();
                    embed.WithColor(Color.Green);
                    embed.WithFooter($"ID сообщения: {messageBefore.Id}");
                    embed.WithThumbnailUrl($"{messageBefore.Value.Author.GetAvatarUrl()}");
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.WithTitle($"📝 Обновлено сообщение");
                    embed.WithDescription($"Где: <#{before.Channel.Id}>" +
                                          $"\nАвтор сообщения: **{after?.Author}**\n");




                    if (messageBefore.Value.Content.Length > 1000)
                    {
                        var string1 = messageBefore.Value.Content.Substring(0, 1000);

                        embed.AddField("Предыдущий текст:", $"{string1}");

                        if (messageBefore.Value.Content.Length <= 2000)
                        {

                            var string2 =
                                messageBefore.Value.Content.Substring(1000, messageBefore.Value.Content.Length - 1000);
                            embed.AddField("Предыдущий текст: Далее", $"...{string2}");

                        }
                    }
                    else if (messageBefore.Value.Content.Length != 0)
                    {
                        embed.AddField("Предыдущий текст:", $"{messageBefore.Value.Content}");
                    }


                    if (messageAfter.Content.Length > 1000)
                    {
                        var string1 = messageAfter.Content.Substring(0, 1000);

                        embed.AddField("Новый текст:", $"{string1}");

                        if (messageAfter.Content.Length <= 2000)
                        {

                            var string2 =
                                messageAfter.Content.Substring(1000, messageAfter.Content.Length - 1000);
                            embed.AddField("Новый текст: Далее", $"...{string2}");

                        }
                    }
                    else if (messageAfter.Content.Length != 0)
                    {
                        embed.AddField("Новый текст:", $"{messageAfter.Content}");
                    }


                    if (guild.EnableLogging == true)
                    {

                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync(null, false, embed.Build());
                    }
                }
            }
            catch
            {

            }

        }

        public async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == _client.CurrentUser.Id)
                return;

            await Task.CompletedTask;
        }

        public async Task MessageDeleted(Cacheable<IMessage, ulong> messageBefore, ISocketMessageChannel arg3)
        {
            try
            {
                if (messageBefore.Value.Author.IsBot)
                    return;
                if (messageBefore.Value.Channel is ITextChannel kek)
                {
                    var guild = Database.GetGuildAccount(kek.Guild);

                    var log = await kek.Guild.GetAuditLogsAsync(1);
                    var audit = log.ToList();

                    var name = $"{messageBefore.Value.Author.Mention}";
                    var check = audit[0].Data as MessageDeleteAuditLogData;

                    if (check?.ChannelId == messageBefore.Value.Channel.Id &&
                        audit[0].Action == ActionType.MessageDeleted)
                        name = $"{audit[0].User.Mention}";

                    var embedDel = new EmbedBuilder();

                    embedDel.WithFooter($"ID сообщения: {messageBefore.Id}");
                    embedDel.WithTimestamp(DateTimeOffset.UtcNow);
                    embedDel.WithThumbnailUrl($"{messageBefore.Value.Author.GetAvatarUrl()}");

                    embedDel.WithColor(Color.Red);
                    embedDel.WithTitle($"🗑 Удалено сообщение");
                    embedDel.WithDescription($"Где: <#{messageBefore.Value.Channel.Id}>\n" +
                                             $"Кем: **{name}** (Не всегда корректно показывает)\n" +
                                             $"Автор сообщения: **{messageBefore.Value.Author}**\n");

                    if (messageBefore.Value.Content.Length > 1000)
                    {
                        var string1 = messageBefore.Value.Content.Substring(0, 1000);

                        embedDel.AddField("Текст сообщения", $"{string1}");

                        if (messageBefore.Value.Content.Length <= 2000)
                        {

                            var string2 =
                                messageBefore.Value.Content.Substring(1000, messageBefore.Value.Content.Length - 1000);
                            embedDel.AddField("Далее", $"...{string2}");

                        }
                    }
                    else if (messageBefore.Value.Content.Length != 0)
                    {
                        embedDel.AddField("Текст сообщения", $"{messageBefore.Value.Content}");
                    }

                    if (guild.EnableLogging == true)
                    {

                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync(null, false, embedDel.Build());
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }

        public async Task RoleCreated(SocketRole arg)
        {
            try
            {
                #region Data
                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleCreateAuditLogData;
                var name = "Неизвестно";
                var embed = new EmbedBuilder();
                if (check?.RoleId == arg.Id)
                    name = audit[0].User.Username;
                #endregion

                #region Message
                embed.WithColor(Color.Orange);
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.AddField("🔑 Создана роль", $"Название: **{arg.Name}**");
                embed.WithFooter($"Кто создавал: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
                #endregion

                var guild = Database.GetGuildAccount(arg.Guild);

                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync(null, false, embed.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }

        }

        public async Task RoleDeleted(SocketRole arg)
        {
            try
            {
                #region Data
                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleDeleteAuditLogData;
                var name = "Неизвестно";
                var embed = new EmbedBuilder();
                if (check?.RoleId == arg.Id)
                    name = audit[0].User.Username;
                #endregion

                #region Message
                embed.WithColor(Color.Red);
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.AddField("❌ Удалена роль",
                    $"Название: **{arg.Name}**\n" +
                    $"Цвет: **{arg.Color}**");
                embed.WithFooter($"Кто удалял: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
                #endregion

                var guild = Database.GetGuildAccount(arg.Guild);

                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync(null, false, embed.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }

        }

        public async Task UserLeft(SocketGuildUser arg)
        {
            try
            {
                #region Checks
                if (arg == null || arg.IsBot)
                    return;
                #endregion

                #region Data
                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var embed = new EmbedBuilder();
                #endregion

                #region Message
                embed.WithColor(Color.Red);
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.WithThumbnailUrl($"{arg.GetAvatarUrl()}");
                embed.AddField($"💢 Страж покинул клан",
                    $"На корабле был известен как:\n**{arg.Nickname ?? arg.Username}**\n" +
                    $"Discord имя стража\n**{arg.Username}#{arg.Discriminator}**\n" +
                    $"Discord ID:\n**{arg.Id}**");
                embed.AddField("Ссылка на профиль(Не всегда корректно отображает)", arg.Mention);
                if (audit[0].Action == ActionType.Kick)
                {
                    var name = audit[0].User.Username ?? "Неизвестно";
                    embed.AddField("Причина изгнания:",
                         $"{audit[0].Reason ?? "Не указана."}\n\n" +
                         $"Кто выгнал: {name}");
                }
                embed.WithFooter($"Если ссылка на профиль некорректно отображается то просто скопируй <@{arg.Id}> вместе с <> и отправь в любой чат сообщением.");
                #endregion

                var guild = Database.GetGuildAccount(arg.Guild);
                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync(null, false, embed.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }
        }
        #endregion
    }
}
