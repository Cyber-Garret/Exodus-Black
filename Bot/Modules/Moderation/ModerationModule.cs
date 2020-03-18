using Bot.Core.Data;
using Bot.Models;
using Bot.Preconditions;
using Bot.Properties;
using Bot.Services;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану и его избранным стражам.",
			NotAGuildErrorMessage = NotInGuildText)]
	[Cooldown(5)]
	public class ModerationModule : BaseModule
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly DiscordEventHandlerService discordEvent;
		public ModerationModule(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<ModerationModule>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			discordEvent = service.GetRequiredService<DiscordEventHandlerService>();
		}

		#region Commands
		[Command("настройки")]
		[Summary("Эта команда выводит мои настройки, так же содержит некоторую полезную и не очень информацию.")]
		public async Task GetGuildConfig()
		{
			// Get or create personal guild settings
			var guild = GuildData.GetGuildAccount(Context.Guild);
			var embed = GuildConfigEmbed(guild);

			await ReplyAsync(embed: embed);
		}

		[Command("новости")]
		[Summary("Команда позволяет включать или выключать оповещения о Зуре в определенный текстовый канал.")]
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

		[Command("логи")]
		[Summary("Команда позволяет включать или выключать оповещения об изменениях на сервер, например, когда кто-то покинул сервер.")]
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

		[Command("приветствие"), RequireBotPermission(ChannelPermission.AttachFiles)]
		[Summary("Команда позволяет включать или выключать оповещения о новых участниках сервера в стиле мира Destiny.")]
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

		[Command("сохранить приветствие")]
		[Summary("Сохраняет сообщение-приветствие и включает механизм отправки сообщения всем новоприбывшим на сервер.\nПоддерживает синтаксис MarkDown для красивого оформления.")]
		public async Task SaveWelcomeMessage([Remainder]string message = null)
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

		[Command("посмотреть приветствие")]
		[Summary("Позволяет посмотреть, как будет выглядеть сообщение-приветствие новоприбывшему на сервер.")]
		public async Task WelcomeMessagePreview()
		{
			// Get or create personal guild settings
			var guild = GuildData.GetGuildAccount(Context.Guild);

			if (string.IsNullOrWhiteSpace(guild.WelcomeMessage))
				await ReplyAndDeleteAsync(Resources.GuildPrivateWelcomeIsNull);
			else
				await ReplyAsync(embed: discordEvent.WelcomeEmbed((SocketGuildUser)Context.User, guild.WelcomeMessage));
		}

		[Command("префикс")]
		[Summary("Позволяет изменить префикс команд для сервера.")]
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

		[Command("автороль")]
		[Summary("Сохраняет роль, которую я буду выдавать всем новым пользователям, пришедшим на сервер.\n" +
			"**Важно! Моя роль должна быть над ролью, которую я буду автоматически выдавать всем новоприбывшим. Имеется ввиду в списке Ваш сервер->Настройки сервера->Роли.**")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task AutoRoleRoleAdd(IRole role = null)
		{
			var guild = GuildData.GetGuildAccount(Context.Guild);

			if (role == null)
			{
				guild.AutoroleID = 0;
				await ReplyAndDeleteAsync(Resources.GuildAutoroleOff);
			}
			else
			{
				guild.AutoroleID = role.Id;
				await ReplyAndDeleteAsync(string.Format(Resources.GuildAutoroleOn, role.Name));
			}
			GuildData.SaveAccounts(Context.Guild);
		}

		[Command("упоминание")]
		[Summary("Изменяет упоминания в сборах и уведомлениях о Зуре here->everyone->Без упоминания и наоборот.")]
		public async Task SetGuildMention(SocketRole role = null)
		{
			var guild = GuildData.GetGuildAccount(Context.Guild);
			if (role == null)
			{
				if (guild.GlobalMention == "@here")
					guild.GlobalMention = "@everyone";
				else if (guild.GlobalMention == "@everyone")
					guild.GlobalMention = null;
				else if (guild.GlobalMention == null)
					guild.GlobalMention = "@here";
				else
					guild.GlobalMention = null;
			}
			else
			{
				guild.GlobalMention = $"<@&{role.Id}>";
			}

			GuildData.SaveAccounts(Context.Guild);

			await ReplyAndDeleteAsync(string.Format(Resources.GuildMilMention, guild.GlobalMention ?? Resources.GuildNoMention));
		}

		[Command("рассылка")]
		[Summary("Рассылает личные сообщения стражам указанной роли. По окончании работы я предоставлю небольшую статистику кому я смогла отправить, а кому нет.")]
		public async Task SendMessage(IRole role, [Remainder] string message)
		{
			var workMessage = await Context.Channel.SendMessageAsync(string.Format(Resources.MailStart, role.Name));

			int SucessCount = 0;
			int FailCount = 0;
			var embed = MailingEmbed(message);

			foreach (var user in Context.Guild.Users)
			{
				if (user.Roles.Contains(role) || role.Name == "everyone")
				{
					try
					{
						var DM = await user.GetOrCreateDMChannelAsync();

						await DM.SendMessageAsync(embed: embed);
						SucessCount++;
					}
					catch (Exception ex)
					{
						FailCount++;
						logger.LogWarning(ex, "SendMessage command");
					}
				}
			}
			await workMessage.ModifyAsync(m => m.Content = string.Format(Resources.MailDone, role.Name, SucessCount + FailCount, SucessCount, FailCount));
		}

		[Command("чистка")]
		[Summary("Удаляет заданное количество сообщений где была вызвана команда.")]
		[RequireBotPermission(ChannelPermission.ManageMessages)]
		public async Task PurgeChat(int amount = 1)
		{
			var GuildOwner = Context.Guild.OwnerId;
			if (Context.User.Id != GuildOwner)
			{
				await ReplyAndDeleteAsync(Resources.OnlyForGuildOwner);
				return;
			}

			try
			{
				await ReplyAsync(Resources.PurgeStart);
				var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
				if (messages.Count() < 1)
					return;

				var TooOlds = messages.Any(m => m.CreatedAt < DateTime.UtcNow.AddDays(-14));
				if (TooOlds)
				{
					foreach (var message in messages)
					{
						await Task.Delay(1000);
						await (Context.Channel as ITextChannel).DeleteMessageAsync(message.Id);
					}
				}
				else
				{
					//Clean amount of messages in current channel
					await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);

					await ReplyAndDeleteAsync(string.Format(Resources.PurgeDone, messages.Count()));
				}

			}
			catch (Exception ex)
			{
				await ReplyAsync(string.Format(Resources.Error, ex.Message));
				logger.LogWarning(ex, "PurgeChat");
			}
		}

		[Command("рандом")]
		[Summary("Случайным образом выбирает от 1 до 10 Стражей из указаной роли. Если не указано количество, по умолчанию выбирает одного.")]
		public async Task GetRandomUser(IRole mentionedRole = null, int count = 1)
		{
			if (mentionedRole == null || (count >= 10 && count <= 1))
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
				logger.LogWarning(ex, "GetRandomUser command");
			}
		}
		#endregion

		#region Methods
		private Embed GuildConfigEmbed(Guild guild)
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = discord.CurrentUser.GetAvatarUrl(),
					Name = Resources.GuCoEmbTitle
				},
				Color = Color.Orange,
				ThumbnailUrl = Context.Guild.IconUrl,

			}
			.AddField(Resources.GuCoEmbTitleField,
			string.Format(Resources.GuCoEmbDescField,
				Context.Guild.Channels.Count,
				Context.Guild.Users.Count,
				guild.NotificationChannel,
				guild.LoggingChannel,
				guild.WelcomeChannel,
				guild.GlobalMention ?? Resources.GuildNoMention));

			return embed.Build();
		}

		private Embed MailingEmbed(string message)
		{
			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.MailEmbTitle, Context.User.Username, Context.Guild.Name),
				Color = Color.LightOrange,
				ThumbnailUrl = Context.Guild.IconUrl,
				Description = message,
				Timestamp = DateTimeOffset.Now,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = Resources.NeiraFooterIcon,
					Text = Resources.MailEmbFooterDesc
				}
			};
			return embed.Build();
		}

		private Embed RandomGuardian(IRole mentionedRole, int count)
		{
			//Get list of SocketGuildUser's from current context
			var users = Context.Guild.Users.ToList();

			//Predicate for filtering users with mentioned role
			bool isHaveRole(SocketGuildUser x) { return x.Roles.Contains(mentionedRole); }
			//Filter users from context who have mentioned role and not a Bot
			var filteredusers = users.FindAll(isHaveRole).Where(u => u.IsBot == false);

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
				var num = random.Next(0, filteredusers.Count());
				//Pick random user
				var user = filteredusers.ElementAt(num);

				field.Value += $"#{i + 1} {user.Mention} - {user.Nickname ?? user.Username}\n";
			}
			embed.AddField(field);

			return embed.Build();
		}
		#endregion
	}
}
