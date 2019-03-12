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
        public readonly DiscordSocketClient _client;
        public readonly IServiceProvider _services;
        #endregion


        public ServerActivityLogger(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;
        }

        #region Methods
        public async Task ChannelCreated(IChannel arg)
        {
            try
            {
                if (!(arg is ITextChannel channel))
                    return;

                var log = await channel.Guild.GetAuditLogsAsync(1);
                var audit = log.ToList();
                var name = audit[0].Action == ActionType.ChannelCreated ? audit[0].User.Mention : "error";
                var auditLogData = audit[0].Data as ChannelCreateAuditLogData;

                var embed = new EmbedBuilder();
                embed.WithColor(Color.Orange);
                embed.AddField("📖 Создан канал",
                    $"Название: {arg.Name}\n" +
                    $"Кто создал: {name}\n" +
                    $"Тип канала: {auditLogData?.ChannelType.ToString()}\n" +
                    $"NSFW {channel.IsNsfw}\n" +
                    $"Категория: {channel.GetCategoryAsync().Result.Name}\n" +
                    $"ID: {arg.Id}\n");
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var currentIGuildChannel = (IGuildChannel)arg;
                var guild = Database.GetGuildAccount(currentIGuildChannel.Guild.Id);
                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {

            }

        }

        public async Task ChannelDestroyed(IChannel arg)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(Color.Red);

                if (arg is ITextChannel channel)
                {
                    var log = await channel.Guild.GetAuditLogsAsync(1);
                    var audit = log.ToList();

                    var name = audit[0].Action == ActionType.ChannelDeleted ? audit[0].User.Mention : "error";
                    var auditLogData = audit[0].Data as ChannelDeleteAuditLogData;
                    embed.AddField("❌ Удален канал",
                        $"Название канала: {arg.Name}\n" +
                        $"Кто удалял: {name}\n" +
                        $"Тип: {auditLogData?.ChannelType}\n" +
                        $"NSFW: {channel.IsNsfw}\n" +
                        $"Категория: {channel.GetCategoryAsync().Result.Name}\n" +
                        $"ID: {arg.Id}\n");

                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");
                }


                if (arg is IGuildChannel currentIguildChannel)
                {
                    var guild = Database.GetGuildAccount(currentIguildChannel.Guild.Id);
                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch
            {

            }
        }

        public async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            try
            {
                if (after == null || before == after || before.IsBot)
                    return;

                var guild = Database.GetGuildAccount(before.Guild.Id);

                var embed = new EmbedBuilder();

                embed.WithColor(Color.Orange);
                embed.WithTimestamp(DateTimeOffset.UtcNow);

                if (before.Nickname != after.Nickname)
                {
                    var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
                    var audit = log.ToList();
                    var beforeName = before.Nickname ?? before.Username;

                    var afterName = after.Nickname ?? after.Username;

                    embed.AddField("💢 Имя стража изменено:",
                        $"Страж: **{before.Username} {before.Id}**\n" +
                        $"Гильдия: **{before.Guild.Name}**\n" +
                        $"Предыдущее имя:\n" +
                        $"**{beforeName}**\n" +
                        $"Новое имя:\n" +
                        $"**{afterName}**");
                    if (audit[0].Action == ActionType.MemberUpdated)
                        embed.AddField("Кем изменено:", $"{audit[0].User.Mention}\n");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");

                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

                if (before.GetAvatarUrl() != after.GetAvatarUrl())
                {

                    embed.AddField("💢 Портрет стража изменен:",
                        $"Страж: **{before.Username} {before.Id}**\n" +
                        $"Предыдущий портер:\n" +
                        $"**{before.GetAvatarUrl()}**\n" +
                        $"Новый портрет:\n" +
                        $"**{after.GetAvatarUrl()}**");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");



                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

                if (before.Username != after.Username || before.Id != after.Id)
                {
                    embed.AddField("💢 Discord ID стража изменен:",
                        $"Предыдущий Discord ID:\n" +
                        $"**{before.Username} {before.Id}**\n" +
                        $"Новый Discord ID:\n" +
                        $"**{after.Username} {after.Id}**\n");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");




                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

                if (before.Roles.Count != after.Roles.Count)
                {

                    string roleString;
                    var list1 = before.Roles.ToList();
                    var list2 = after.Roles.ToList();
                    var role = "";
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
                        roleString = "Добавлена";
                        var differenceQuery = list2.Except(list1);
                        var socketRoles = differenceQuery as SocketRole[] ?? differenceQuery.ToArray();
                        for (var i = 0; i < socketRoles.Count(); i++)
                            role += socketRoles[i];
                    }

                    var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
                    var audit = log.ToList();

                    embed.AddField($"🔑 Обновлена роль стража ({roleString} роль):",

                        $"Страж: **{before.Username} {before.Id}**\n" +
                        $"Гильдия: **{before.Guild.Name}**\n" +
                        $"{roleString} роль: **{role}**");
                    if (audit[0].Action == ActionType.MemberRoleUpdated)
                        embed.AddField("Кто обновлял:", $"{audit[0].User.Mention}\n");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");


                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

            }
            catch
            {
                // ignored
            }

        }

        public async Task MessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel arg3)
        {
            try
            {
                var before = (messageBefore.HasValue ? messageBefore.Value : null) as IUserMessage;
                if (arg3 is IGuildChannel currentIGuildChannel)
                {
                    var guild = Database.GetGuildAccount(currentIGuildChannel.Guild.Id);
                    if (messageAfter.Author.IsBot)
                        return;

                    var after = messageAfter as IUserMessage;

                    if (messageAfter.Content == null)
                    {
                        return;
                    }

                    if (before == null)
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

                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch
            {

            }

        }

        public async Task MessageDeleted(Cacheable<IMessage, ulong> messageBefore, ISocketMessageChannel arg3)
        {
            try
            {
                if (messageBefore.Value.Author.IsBot)
                    return;
                if (messageBefore.Value.Channel is ITextChannel kek)
                {
                    var guild = Database.GetGuildAccount(kek.Guild.Id);

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

                        await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embedDel.Build());
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
                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleCreateAuditLogData;
                var name = "Неизвестно";

                if (check?.RoleId == arg.Id)
                {
                    name = audit[0].User.Mention;
                }

                var embed = new EmbedBuilder();
                embed.WithColor(Color.Orange);
                embed.AddField("🗝️ Создана роль",
                    $"Кем: {name}\n" +
                    $"Название: **{arg.Name}**\n" +
                    $"Цвет: {arg.Color}\n" +
                    $"ID: {arg.Id}\n");
                embed.WithTimestamp(DateTimeOffset.UtcNow);

                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var guild = Database.GetGuildAccount(arg.Guild.Id);

                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {
                //
            }

        }

        public async Task RoleDeleted(SocketRole arg)
        {
            try
            {

                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleDeleteAuditLogData;
                var name = "Неизвестно";

                if (check?.RoleId == arg.Id)
                {
                    name = audit[0].User.Mention;
                }

                var embed = new EmbedBuilder();
                embed.WithColor(Color.Red);
                embed.AddField("🗝️ Удалена роль",
                    $"Кем: {name}\n" +
                    $"Название: **{arg.Name}**\n" +
                    $"Цвет: {arg.Color}\n" +
                    $"ID: {arg.Id}\n");
                embed.WithTimestamp(DateTimeOffset.UtcNow);

                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var guild = Database.GetGuildAccount(arg.Guild.Id);

                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {

            }

        }

        public async Task UserLeft(SocketGuildUser arg)
        {
            try
            {
                if (arg == null || arg.IsBot)
                    return;

                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();

                EmbedBuilder embed = new EmbedBuilder();

                embed.AddField("Страж покинул гильдию",
                    $"Имя стража: {arg.Username}\n" +
                    $"Среди других стражей был известен как: {arg.Nickname}");
                if (audit[0].Action == ActionType.Kick)
                {
                    embed.AddField("Причина изгнания:", audit[0].Reason);
                    embed.WithFooter($"Кто выгнал: {audit[0].User.Username}",audit[0].User.GetAvatarUrl());
                }
                embed.WithColor(Color.Red);
                embed.WithThumbnailUrl($"{arg.GetAvatarUrl()}");
                embed.WithTimestamp(DateTimeOffset.UtcNow);

                var guild = Database.GetGuildAccount(arg.Guild.Id);

                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.GuildID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
            }
        }
        #endregion
    }
}
