using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Models;
using Failsafe.Preconditions;
using Failsafe.Properties;
using Failsafe.Services;

using Microsoft.Extensions.Logging;

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Failsafe.Modules
{
    [RequireContext(ContextType.Guild), Cooldown(5), RequireUserPermission(GuildPermission.Administrator)]
    public class ModerationModule : RootModule
    {
        private readonly ILogger<ModerationModule> _logger;
        private readonly DiscordSocketClient _discord;

        public ModerationModule(ILogger<ModerationModule> logger, DiscordSocketClient discord)
        {
            _logger = logger;
            _discord = discord;
        }

        #region Commands
        [Command("settings"), Alias("настройки", "налаштування")]
        public async Task GetGuildConfig()
        {
            // Get or create personal guild settings
            var guild = GuildData.GetGuildAccount(Context.Guild);
            var embed = GuildConfigEmbed(guild);

            await ReplyAsync(embed: embed);
        }

        [Command("locale"), Alias("язык", "мова")]
        public async Task ChangeGuildLocale()
        {
            var guild = GuildData.GetGuildAccount(Context.Guild);

            guild.Language = guild.Language.Name switch
            {
                "en-US" => new CultureInfo("ru-RU"),
                "ru-RU" => new CultureInfo("uk-UA"),
                _ => new CultureInfo("en-US")
            };

            GuildData.SaveAccounts(Context.Guild);

            Thread.CurrentThread.CurrentUICulture = guild.Language;

            await ReplyAsync(string.Format(Resources.LocaleChanged, guild.Language.NativeName));
        }

        [Command("utc"), Alias("время", "час")]
        public async Task ChangeTimeForServer(sbyte time)
        {
            if (time < -12 || time > 12)
            {
                //TODO: resx
                await ReplyAsync(Resources.UTCIncorrect);
            }
            else
            {
                //TODO: clean code
                var timeSpan = new TimeSpan(time, 0, 0);
                foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (timeZone.BaseUtcOffset != timeSpan) continue;
                    var guild = GuildData.GetGuildAccount(Context.Guild);
                    guild.TimeZone = timeZone.Id;

                    GuildData.SaveAccounts(guild.Id);

                    var regex = new Regex(@"\(.*?\)");
                    var parsedTimeZone = regex.Match(timeZone.DisplayName);
                    //TODO: resx
                    await ReplyAsync(string.Format(Resources.UTCChanged, parsedTimeZone));

                    break;
                }
            }

        }

        [Command("news"), Alias("новости", "новини")]
        public async Task SetNotificationChannel(ITextChannel channel = null)
        {
            // Get or create personal guild settings
            var guild = GuildData.GetGuildAccount(Context.Guild);

            if (channel == null)
            {
                guild.NotificationChannel = 0;
                await ReplyAndDeleteAsync(Resources.GuildXurOff);
            }
            else
            {
                guild.NotificationChannel = channel.Id;
                await ReplyAndDeleteAsync(string.Format(Resources.GuildXurOn, channel.Mention));
            }
            GuildData.SaveAccounts(Context.Guild);
        }

        [Command("logs"), Alias("логи", "логування")]
        public async Task SetLogChannel(ITextChannel channel = null)
        {
            // Get or create personal guild settings
            var guild = GuildData.GetGuildAccount(Context.Guild);

            if (channel == null)
            {
                guild.LoggingChannel = 0;
                await ReplyAndDeleteAsync(Resources.GuildLogsOff);
            }
            else
            {
                guild.LoggingChannel = channel.Id;
                await ReplyAndDeleteAsync(string.Format(Resources.GuildLogsOn, channel.Mention));
            }
            GuildData.SaveAccounts(Context.Guild);
        }

        [Command("welcome"), Alias("приветствие", "привітання"), RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task SetWelcomeChannel(ITextChannel channel = null)
        {
            // Get or create personal guild settings
            var guild = GuildData.GetGuildAccount(Context.Guild);

            if (channel == null)
            {
                guild.WelcomeChannel = 0;
                await ReplyAndDeleteAsync(Resources.GuildWelcomeOff);
            }
            else
            {
                guild.WelcomeChannel = channel.Id;
                await ReplyAndDeleteAsync(string.Format(Resources.GuildWelcomeOn, channel.Mention));
            }
            GuildData.SaveAccounts(Context.Guild);
        }

        [Command("save welcome"), Alias("сохранить приветствие", "зберегти привітання")]
        public async Task SaveWelcomeMessage([Remainder] string message = null)
        {
            // Get or create personal guild settings
            var guild = GuildData.GetGuildAccount(Context.Guild);

            if (string.IsNullOrWhiteSpace(message))
            {
                guild.WelcomeMessage = null;
                await ReplyAndDeleteAsync(Resources.GuildPrivateWelcomeRemove);
            }
            else
            {
                guild.WelcomeMessage = message;
                await ReplyAndDeleteAsync(Resources.GuildPrivateWelcomeSave);
                //TODO send example welcome
            }
            GuildData.SaveAccounts(Context.Guild);
        }

        [Command("preview welcome"), Alias("посмотреть приветствие", "переглянути привітання")]
        public async Task WelcomeMessagePreview()
        {
            // Get or create personal guild settings
            var guild = GuildData.GetGuildAccount(Context.Guild);

            if (string.IsNullOrWhiteSpace(guild.WelcomeMessage))
                await ReplyAndDeleteAsync(Resources.GuildPrivateWelcomeIsNull);
            else
                await ReplyAsync(embed: DiscordEventHandlerService.WelcomeEmbed((SocketGuildUser)Context.User, guild.WelcomeMessage));
        }

        [Command("prefix"), Alias("префикс", "префікс")]
        public async Task GuildPrefix(string prefix = null)
        {
            var config = GuildData.GetGuildAccount(Context.Guild);

            if (prefix == null)
            {
                config.CommandPrefix = null;
                await ReplyAsync(Resources.GuildPrefixDefault);
            }
            else
            {
                config.CommandPrefix = prefix;
                await ReplyAsync(string.Format(Resources.GuildPrefixCustom, prefix));
            }
            GuildData.SaveAccounts(Context.Guild);
        }

        [Command("autorole"), Alias("автороль", "автороль")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AutoRoleRoleAdd(IRole role = null)
        {
            var guild = GuildData.GetGuildAccount(Context.Guild);

            if (role == null)
            {
                guild.AutoroleId = 0;
                await ReplyAndDeleteAsync(Resources.GuildAutoroleOff);
            }
            else
            {
                guild.AutoroleId = role.Id;
                await ReplyAndDeleteAsync(string.Format(Resources.GuildAutoroleOn, role.Name));
            }
            GuildData.SaveAccounts(Context.Guild);
        }

        [Command("mention"), Alias("упоминание", "згадування")]
        public async Task SetGuildMention(SocketRole role = null)
        {
            var guild = GuildData.GetGuildAccount(Context.Guild);
            if (role == null)
            {
                guild.GlobalMention = guild.GlobalMention switch
                {
                    "@here" => "@everyone",
                    "@everyone" => null,
                    null => "@here",
                    _ => null
                };
            }
            else
            {
                guild.GlobalMention = $"<@&{role.Id}>";
            }

            GuildData.SaveAccounts(Context.Guild);

            await ReplyAndDeleteAsync(string.Format(Resources.GuildMilMention, guild.GlobalMention ?? Resources.GuildNoMention));
        }

        [Command("mailing"), Alias("рассылка", "розсилка")]
        public async Task SendMessage(IRole role, [Remainder] string message)
        {
            var workMessage = await Context.Channel.SendMessageAsync(string.Format(Resources.MailStart, role.Name));

            var successCount = 0;
            var failCount = 0;
            var embed = MailingEmbed(message);

            foreach (var user in Context.Guild.Users)
            {
                if (!user.Roles.Contains(role) && role.Name != "everyone") continue;
                try
                {
                    var dm = await user.GetOrCreateDMChannelAsync();

                    await dm.SendMessageAsync(embed: embed);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogWarning(ex, "SendMessage command");
                }
            }
            await workMessage.ModifyAsync(m => m.Content = string.Format(Resources.MailDone, role.Name, successCount + failCount, successCount, failCount));
        }

        [Command("purge"), Alias("чистка", "очищення")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChat(int amount = 1)
        {
            var guildOwner = Context.Guild.OwnerId;
            if (Context.User.Id != guildOwner)
            {
                await ReplyAndDeleteAsync(Resources.OnlyForGuildOwner);
                return;
            }

            try
            {
                await ReplyAsync(Resources.PurgeStart);
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                var messagesForDeletion = messages as IMessage[] ?? messages.ToArray();
                if (!messagesForDeletion.Any())
                    return;

                var tooOlds = messagesForDeletion.Any(m => m.CreatedAt < DateTime.UtcNow.AddDays(-14));
                if (tooOlds)
                {
                    foreach (var message in messagesForDeletion)
                    {
                        await Task.Delay(1000);
                        await ((ITextChannel)Context.Channel).DeleteMessageAsync(message.Id);
                    }
                }
                else
                {
                    //Clean amount of messages in current channel
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messagesForDeletion);

                    await ReplyAndDeleteAsync(string.Format(Resources.PurgeDone, messagesForDeletion.Length));
                }

            }
            catch (Exception ex)
            {
                await ReplyAsync(string.Format(Resources.Error, ex.Message));
                _logger.LogWarning(ex, "PurgeChat");
            }
        }

        [Command("random"), Alias("рандом")]
        public async Task GetRandomUser(IRole mentionedRole = null, int count = 1)
        {
            if (mentionedRole == null || (count < 1 || count > 10))
            {
                await ReplyAndDeleteAsync(Resources.RndErrorStart);
                return;
            }
            try
            {
                var embed = RandomGuardian(mentionedRole, count);
                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                await ReplyAndDeleteAsync(string.Format(Resources.Error, ex.Message));
                _logger.LogWarning(ex, "GetRandomUser command");
            }
        }
        #endregion

        #region Methods
        private Embed GuildConfigEmbed(Guild guild)
        {
            var parsedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone).DisplayName;

            var regex = new Regex(@"\(.*?\)");
            var guildTimeZone = regex.Match(parsedTimeZone);

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = _discord.CurrentUser.GetAvatarUrl(),
                    Name = Resources.GuCoEmbTitle
                },
                Color = Color.Orange,
                ThumbnailUrl = Context.Guild.IconUrl
            }
            .AddField(Resources.GuCoEmbTitleField,
            string.Format(Resources.GuCoEmbDescField,
                Context.Guild.Channels.Count,
                Context.Guild.Users.Count,
                guild.NotificationChannel,
                guild.LoggingChannel,
                guild.WelcomeChannel,
                guild.GlobalMention ?? Resources.GuildNoMention,
                guild.Language.NativeName,
                guildTimeZone));

            return embed.Build();
        }

        private Embed MailingEmbed(string message)
        {
            return new EmbedBuilder()
                .WithTitle(string.Format(Resources.MailEmbTitle, Context.User.Username, Context.Guild.Name))
                .WithColor(Color.LightOrange)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription(message)
                .WithCurrentTimestamp()
                //TODO: Donate text
                //.AddField(Resources.DonateEmbFieldTitle, Resources.DonateEmbFieldDesc)
                .WithFooter(Resources.MailEmbFooterDesc, Resources.NeiraFooterIcon)
                .Build();
        }

        private Embed RandomGuardian(IRole mentionedRole, int count)
        {
            //Get list of SocketGuildUser's from current context
            var users = Context.Guild.Users.ToList();

            //Predicate for filtering users with mentioned role
            bool IsHaveRole(SocketGuildUser x) => x.Roles.Contains(mentionedRole);
            //Filter users from context who have mentioned role and not a Bot
            var filteredUsers = users.FindAll(IsHaveRole).Where(u => u.IsBot == false);

            var embed = new EmbedBuilder()
            {
                Color = Color.Gold
            };
            var field = new EmbedFieldBuilder
            {
                Name = Resources.RndEmbFieldTitle
            };
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var num = random.Next(0, filteredUsers.Count());
                //Pick random user
                var user = filteredUsers.ElementAt(num);

                field.Value += $"#{i + 1} {user.Mention} - {user.Nickname ?? user.Username}\n";
            }
            embed.AddField(field);

            return embed.Build();
        }
        #endregion
    }
}
