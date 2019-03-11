using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Neuromatrix.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuromatrix.Modules.Administration
{
    public class ServerActivityLogger
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.

        public readonly DiscordSocketClient _client;
        public readonly IServiceProvider _services;

        public ServerActivityLogger(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;
        }

        public async Task Client_Connected()
        {
        }

        public async Task Client_Disconnected(Exception arg)
        {

        }
        public async Task ChannelDestroyed(IChannel arg)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(14, 243, 247);

                if (arg is ITextChannel channel)
                {
                    var log = await channel.Guild.GetAuditLogsAsync(1);
                    var audit = log.ToList();

                    var name = audit[0].Action == ActionType.ChannelDeleted ? audit[0].User.Mention : "error";
                    var auditLogData = audit[0].Data as ChannelDeleteAuditLogData;
                    embed.AddField("🚫 Канал удален", $"Название канала: {arg.Name}\n" +
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
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch
            {
                //
            }
        }

        public async Task Client_ChannelDestroyed(IChannel arg)
        {
            ChannelDestroyed(arg);
            await Task.CompletedTask;
        }

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
                embed.WithColor(14, 243, 247);
                embed.AddField("📖 Канал создан", $"Название: {arg.Name}\n" +
                                                     $"Кто создал: {name}\n" +
                                                     $"Тип: {auditLogData?.ChannelType.ToString()}\n" +
                                                     $"NSFW {channel.IsNsfw}\n" +
                                                     $"Категория: {channel.GetCategoryAsync().Result.Name}\n" +
                                                     $"ID: {arg.Id}\n");
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var currentIGuildChannel = (IGuildChannel)arg;
                var guild = Database.GetGuildAccount(currentIGuildChannel.Guild.Id);
                if (guild.EnableLogging == true)
                {
                    await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {
                //
            }

        }

        public async Task Client_ChannelCreated(IChannel arg)
        {
            ChannelCreated(arg);
            await Task.CompletedTask;

        }

        public async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            try
            {
                if (after == null || before == after || before.IsBot)
                    return;

                var guild = Database.GetGuildAccount(before.Guild.Id);

                var embed = new EmbedBuilder();
                if (before.Nickname != after.Nickname)
                {
                    var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
                    var audit = log.ToList();
                    var beforeName = before.Nickname ?? before.Username;

                    var afterName = after.Nickname ?? after.Username;

                    embed.WithColor(255, 255, 0);
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.AddField("💢 Имя стража изменено:",
                        $"Страж: **{before.Username} {before.Id}**\n" +
                        $"Гильдия: **{before.Guild.Name}**\n" +
                        $"Предыдущее имя:\n" +
                        $"**{beforeName}**\n" +
                        $"Новое имя:\n" +
                        $"**{afterName}**");
                    if (audit[0].Action == ActionType.MemberUpdated)
                        embed.AddField("Who:", $"{audit[0].User.Mention}\n");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");

                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

                if (before.GetAvatarUrl() != after.GetAvatarUrl())
                {
                    embed.WithColor(255, 255, 0);
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.AddField("💢 Портрет стража изменен:",
                        $"Страж: **{before.Username} {before.Id}**\n" +
                        $"Гильдия: **{before.Guild.Name}**\n" +
                        $"Предыдущий портер:\n" +
                        $"**{before.GetAvatarUrl()}**\n" +
                        $"Новый портрет:\n" +
                        $"**{after.GetAvatarUrl()}**");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");



                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

                if (before.Username != after.Username || before.Id != after.Id)
                {
                    embed.WithColor(255, 255, 0);
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.AddField("💢 Позывной стража изменен:",
                        $"Гильдия: **{before.Guild.Name}**\n" +
                        $"Предыдущий позывной:\n" +
                        $"**{before.Username} {before.Id}**\n" +
                        $"Новый позывной:\n" +
                        $"**{after.Username} {after.Id}**\n");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");




                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
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

                    embed.WithColor(255, 255, 0);
                    embed.WithTimestamp(DateTimeOffset.UtcNow);
                    embed.AddField($"👑 Обновлена роль ({roleString} роль):",

                        $"Страж: **{before.Username} {before.Id}**\n" +
                        $"Гильдия: **{before.Guild.Name}**\n" +
                        $"{roleString} роль: **{role}**");
                    if (audit[0].Action == ActionType.MemberRoleUpdated)
                        embed.AddField("Кто обновлял:", $"{audit[0].User.Mention}\n");
                    embed.WithThumbnailUrl($"{after.GetAvatarUrl()}");


                    if (guild.EnableLogging == true)
                    {
                        await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }

            }
            catch
            {
                // ignored
            }

        }

        public async Task Client_GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            GuildMemberUpdated(before, after);
            await Task.CompletedTask;
        }

        public async Task MessageUpdated(Cacheable<IMessage, ulong> messageBefore,
            SocketMessage messageAfter, ISocketMessageChannel arg3)
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

                        embed.AddField("Before:", $"{string1}");

                        if (messageBefore.Value.Content.Length <= 2000)
                        {

                            var string2 =
                                messageBefore.Value.Content.Substring(1000, messageBefore.Value.Content.Length - 1000);
                            embed.AddField("Before: Continued", $"...{string2}");

                        }
                    }
                    else if (messageBefore.Value.Content.Length != 0)
                    {
                        embed.AddField("Before:", $"{messageBefore.Value.Content}");
                    }


                    if (messageAfter.Content.Length > 1000)
                    {
                        var string1 = messageAfter.Content.Substring(0, 1000);

                        embed.AddField("After:", $"{string1}");

                        if (messageAfter.Content.Length <= 2000)
                        {

                            var string2 =
                                messageAfter.Content.Substring(1000, messageAfter.Content.Length - 1000);
                            embed.AddField("After: Continued", $"...{string2}");

                        }
                    }
                    else if (messageAfter.Content.Length != 0)
                    {
                        embed.AddField("After:", $"{messageAfter.Content}");
                    }


                    if (guild.IsServerLoggingEnabled == true)
                    {

                        await _client.GetGuild(guild.Id).GetTextChannel(guild.ServerLoggingChannel)
                            .SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch
            {
                //  Console.WriteLine("Cath messupd");
            }

        }

        public async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == _client.CurrentUser.Id)
                return;

            await Task.CompletedTask;
        }

        public async Task Client_MessageUpdated(Cacheable<IMessage, ulong> messageBefore,
            SocketMessage messageAfter, ISocketMessageChannel arg3)
        {
            MessageUpdated(messageBefore, messageAfter, arg3);
            await Task.CompletedTask;

        }

        public async Task DeleteLogg(Cacheable<IMessage, ulong> messageBefore,
            ISocketMessageChannel arg3)
        {
            try
            {
                if (messageBefore.Value.Author.IsBot)
                    return;
                if (messageBefore.Value.Channel is ITextChannel kek)
                {
                    var guild = GlobalGuildAccounts.GetGuildAccount(kek.Guild.Id);

                    var log = await kek.Guild.GetAuditLogsAsync(1);
                    var audit = log.ToList();

                    var name = $"{messageBefore.Value.Author.Mention}";
                    var check = audit[0].Data as MessageDeleteAuditLogData;

                    if (check?.ChannelId == messageBefore.Value.Channel.Id &&
                        audit[0].Action == ActionType.MessageDeleted)
                        name = $"{audit[0].User.Mention}";

                    var embedDel = new EmbedBuilder();

                    embedDel.WithFooter($"MessageId: {messageBefore.Id}");
                    embedDel.WithTimestamp(DateTimeOffset.UtcNow);
                    embedDel.WithThumbnailUrl($"{messageBefore.Value.Author.GetAvatarUrl()}");

                    embedDel.WithColor(Color.Red);
                    embedDel.WithTitle($"🗑 Deleted Message");
                    embedDel.WithDescription($"Where: <#{messageBefore.Value.Channel.Id}>\n" +
                                             $"Who: **{name}** (not always correct)\n" +
                                             $"Message Author: **{messageBefore.Value.Author}**\n");


                    if (messageBefore.Value.Content.Length > 1000)
                    {
                        var string1 = messageBefore.Value.Content.Substring(0, 1000);

                        embedDel.AddField("Content1", $"{string1}");

                        if (messageBefore.Value.Content.Length <= 2000)
                        {

                            var string2 =
                                messageBefore.Value.Content.Substring(1000, messageBefore.Value.Content.Length - 1000);
                            embedDel.AddField("Continued", $"...{string2}");

                        }
                    }
                    else if (messageBefore.Value.Content.Length != 0)
                    {
                        embedDel.AddField("Content", $"{messageBefore.Value.Content}");
                    }

                    if (guild.IsServerLoggingEnabled == true)
                    {

                        await _client.GetGuild(guild.Id).GetTextChannel(guild.ServerLoggingChannel)
                            .SendMessageAsync("", false, embedDel.Build());
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }

        public async Task Client_MessageDeleted(Cacheable<IMessage, ulong> messageBefore,
            ISocketMessageChannel arg3)
        {
            DeleteLogg(messageBefore, arg3);
            await Task.CompletedTask;
        }

        public async Task RoleCreated(SocketRole arg)
        {
            try
            {
                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleCreateAuditLogData;
                var name = "Unknown";

                if (check?.RoleId == arg.Id)
                {
                    name = audit[0].User.Mention;
                }

                var embed = new EmbedBuilder();
                embed.WithColor(240, 51, 255);
                embed.AddField("⚰️ Role Created", $"Who: {name}\n" +

                                                  $"Name: {arg.Name} ({arg.Guild})\n" +
                                                  $"Color: {arg.Color}\n" +
                                                  $"ID: {arg.Id}\n");
                embed.WithTimestamp(DateTimeOffset.UtcNow);

                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var guild = GlobalGuildAccounts.GetGuildAccount(arg.Guild.Id);

                if (guild.IsServerLoggingEnabled == true)
                {
                    await _client.GetGuild(guild.Id).GetTextChannel(guild.ServerLoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {
                //
            }

        }

        public async Task Client_RoleCreated(SocketRole arg)
        {
            RoleCreated(arg);
            await Task.CompletedTask;
        }

        public async Task RoleDeleted(SocketRole arg)
        {
            try
            {

                var log = await arg.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleDeleteAuditLogData;
                var name = "Unknown";

                if (check?.RoleId == arg.Id)
                {
                    name = audit[0].User.Mention;
                }

                var embed = new EmbedBuilder();
                embed.WithColor(240, 51, 255);
                embed.AddField("⚰️ Role Deleted", $"Who: {name}\n" +

                                                  $"Name: {arg.Name} ({arg.Guild})\n" +
                                                  $"Color: {arg.Color}\n" +
                                                  $"ID: {arg.Id}\n");
                embed.WithTimestamp(DateTimeOffset.UtcNow);

                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var guild = GlobalGuildAccounts.GetGuildAccount(arg.Guild.Id);

                if (guild.IsServerLoggingEnabled == true)
                {
                    await _client.GetGuild(guild.Id).GetTextChannel(guild.ServerLoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {
                //
            }

        }

        public async Task Client_RoleDeleted(SocketRole arg)
        {
            RoleDeleted(arg);
            await Task.CompletedTask;
        }
        /*
        public async Task RoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            try
            {
                var before = arg1;
                var after = arg2;
                if (after == null)
                    return;
                if (before == after)
                    return;


                var roleString = "nothing";
                var list1 = before.Permissions.ToList();
                var list2 = after.Permissions.ToList();
                var role = "\n";

                if (list1.Count > list2.Count)
                {
                    roleString = "Removed";
                    var differenceQuery = list1.Except(list2);
                    var socketRoles = differenceQuery as GuildPermission[] ?? differenceQuery.ToArray();
                    for (var i = 0; i < socketRoles.Count(); i++)
                        role += $"{socketRoles[i]}\n";
                }
                else if (list1.Count < list2.Count)
                {
                    roleString = "Added";
                    var differenceQuery = list2.Except(list1);
                    var socketRoles = differenceQuery as GuildPermission[] ?? differenceQuery.ToArray();
                    for (var i = 0; i < socketRoles.Count(); i++)
                        role += $"{socketRoles[i]}\n";
                }

                var extra = "";
                if (before.Name != after.Name)
                {
                    extra += $"__**Before:**__\n" +
                             $"Name: **{before}**\n";
                    if (before.Color.ToString() != after.Color.ToString())
                    {
                        extra += $"Color: {before.Color}\n";
                    }

                    extra += $"__**After:**__\n" +
                             $"Name: **{after}**\n";
                    if (before.Color.ToString() != after.Color.ToString())
                    {
                        extra += $"Color: {after.Color}\n";
                    }

                }
                else if (before.Color.ToString() != after.Color.ToString())
                {
                    extra += $"__**Before:**__\n";
                    extra += $"Color: {before.Color}\n";
                    extra += $"__**After:**__\n";
                    extra += $"Color: {after.Color}\n";

                }

                var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
                var audit = log.ToList();
                var check = audit[0].Data as RoleUpdateAuditLogData;
                var name = "error";
                if (check?.After.Name == arg2.Name)
                {
                    name = audit[0].User.Mention;
                }

                var guild = GlobalGuildAccounts.GetGuildAccount(before.Guild.Id);
                var embed = new EmbedBuilder();
                embed.WithColor(57, 51, 255);
                embed.AddField($"🛠️ Role Updated({roleString})", $"Role: {after.Mention}\n" +
                                                                  $"Who: {name}\n" +
                                                                  $"ID: {before.Id}\n" +
                                                                  $"Guild: {before.Guild.Name}\n" +
                                                                  $"{extra}" +
                                                                  $"Permission ({roleString}): **{role}**");
                embed.WithTimestamp(DateTimeOffset.UtcNow);
                embed.WithThumbnailUrl($"{audit[0].User.GetAvatarUrl()}");


                var s = role.Replace("\n", "");
                if (s.Length < 1 && extra.Length < 1)
                    return;

                if (guild.IsServerLoggingEnabled == true)
                {
                    await _client.GetGuild(guild.Id).GetTextChannel(guild.ServerLoggingChannel)
                        .SendMessageAsync("", false, embed.Build());
                }
            }
            catch
            {
                //
            }
        }

        public async Task Client_RoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            RoleUpdated(arg1, arg2);
            await Task.CompletedTask;

        }*/
    }
}
