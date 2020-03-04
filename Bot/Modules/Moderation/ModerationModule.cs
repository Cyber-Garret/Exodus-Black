using Bot.Preconditions;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Bot.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using Microsoft.Extensions.Logging;
using Bot.Models;
using System.Threading;
using Bot.Properties;

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
		public ModerationModule(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<ModerationModule>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
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
		[Remarks("Для выключения оповещений напиши **!новости**. Для включения **!новости #новостной-канал**. ")]
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
		[Remarks("Для выключения оповещений напиши **!логи**. Для включения **!логи #нейра-логи**. ")]
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
		[Remarks("Для выключения оповещений напиши **!приветствие**. Для включения **!логи #флудилка**. ")]
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
		[Remarks("Пример: !сохранить приветствие <Сообщение>")]
		public async Task SaveWelcomeMessage([Remainder]string message = null)
		{
			//First check if welcome message represent ?
			if (string.IsNullOrWhiteSpace(message)) return;

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
			// TODO: Welcome embed message.
			//await ReplyAsync($"{Context.User.Mention} вот так выглядит сообщение для новоприбывших в Discord.",
			//				 embed: MiscHelpers.WelcomeEmbed(Context.Guild.CurrentUser, guild.WelcomeMessage).Build());

			// Get or create personal guild settings
			var guild = GuildData.GetGuildAccount(Context.Guild);

			if (string.IsNullOrWhiteSpace(guild.WelcomeMessage))
				await ReplyAndDeleteAsync(Resources.GuildPrivateWelcomeIsNull);
			else
				await ReplyAsync(guild.WelcomeMessage);
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
		[Remarks("Пример: **!рассылка <Роль> <Текст сообщения>**\n!рассылка @Тест Привет, завтра у нас клановый сбор в дискорде.")]
		public async Task SendMessage(IRole _role, [Remainder] string message)
		{

			var workMessage = await Context.Channel.SendMessageAsync($"Приступаю к рассылке сообщений. Всем стражам с ролью **{_role.Name}**");

			var role = Context.Guild.Roles.FirstOrDefault(r => r.Id == _role.Id);

			int SucessCount = 0;
			int FailCount = 0;

		

			foreach (var user in Context.Guild.Users)
			{
				if (user.Roles.Contains(role) || role.Name == "everyone")
				{
					try
					{
						var DM = await user.GetOrCreateDMChannelAsync();

						await DM.SendMessageAsync(embed: embed.Build());
						SucessCount++;
					}
					catch (Exception ex)
					{
						FailCount++;
						logger.LogWarning(ex, "SendMessage command");
					}
				}
			}
			await workMessage.ModifyAsync(m => m.Content =
			$"Готово. Я разослала сообщением всем у кого есть роль **{role.Name}**.\n" +
			$"- Всего получателей: **{SucessCount + FailCount}**\n" +
			$"- Успешно доставлено: **{SucessCount}**\n" +
			$"- Не удалось отправить: **{FailCount}**");
		}

		[Command("чистка")]
		[Summary("Удаляет заданное количество сообщений где была вызвана команда.")]
		[Remarks("Синтаксис: !чистка <число> Пример: !чистка 10")]
		[RequireBotPermission(ChannelPermission.ManageMessages)]
		public async Task PurgeChat(int amount = 1)
		{
			var GuildOwner = Context.Guild.OwnerId;
			if (Context.User.Id != GuildOwner)
			{
				await ReplyAndDeleteAsync(":x: | Прошу прощения страж, но эта команда доступна только капитану корабля!");
				return;
			}

			try
			{
				await ReplyAsync("Начинаю очистку канала.");
				var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
				if (messages.Count() < 1)
					return;

				var TooOlds = messages.Any(m => m.CreatedAt < DateTime.UtcNow.AddDays(-14));
				if (TooOlds)
				{
					foreach (var message in messages)
					{
						await Task.Delay(500);
						await (Context.Channel as ITextChannel).DeleteMessageAsync(message.Id);
					}
				}
				else
				{
					//Clean amount of messages in current channel
					await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);

					await ReplyAndDeleteAsync($"Задание успешно выполнено. Удалено {messages.Count()} сообщений. _Это сообщение будет автоматически удалено._");
				}

			}
			catch (Exception ex)
			{
				await ReplyAsync($"Ошибка очистки канала от {amount} сообщений. {ex.Message}.");
				logger.LogWarning(ex, "PurgeChat");
			}
		}

		[Command("рандом")]
		[Alias("ранд")]
		[Summary("Случайным образом выбирает от 1 до 10 Стражей из указаной роли. Если не указано количество, по умолчанию выбирает одного.")]
		[Remarks("Синтаксис: !ранд @Роль <1-10> Пример: !рандом ")]
		public async Task GetRandomUser(IRole mentionedRole = null, int count = 1)
		{
			if (mentionedRole == null || (count >= 10 && count <= 1))
			{
				await ReplyAndDeleteAsync("Вы не указали Роль или указали меньше 1 или больше 10 рандомов.");
				return;
			}
			try
			{
				//Get list of SocketGuildUser's from current context
				var users = Context.Guild.Users.ToList();
				//Get mentioned SocketRole
				var role = Context.Guild.Roles.First(r => r.Id == mentionedRole.Id);

				//Predicate for filtering users with mentioned role
				bool isHaveRole(SocketGuildUser x) { return x.Roles.Contains(mentionedRole); }
				//Filter users from context who have mentioned role and not a Bot
				var filteredusers = users.FindAll(isHaveRole).Where(u => u.IsBot == false);

				var embed = new EmbedBuilder()
					.WithColor(Color.Gold);
				var field = new EmbedFieldBuilder();
				var random = new Random();
				//Chose right field name by count
				if (count == 1)
					field.Name = "Капитан, генератор псевдослучайных чисел Вексов отобразил имя этого стража:";
				else
					field.Name = "Капитан, генератор псевдослучайных чисел Вексов отобразил имя этих стражей:";
				for (int i = 0; i < count; i++)
				{
					var num = random.Next(0, filteredusers.Count());
					//Pick random user
					var user = filteredusers.ElementAt(num);

					field.Value += $"#{i + 1} {user.Mention} - {user.Nickname ?? user.Username}\n";
				}
				embed.AddField(field);

				await ReplyAsync(embed: embed.Build());
			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync($"Ошибка генератора псевдослучайных чисел Вексов: {ex.Message}");
				logger.LogWarning(ex, "GetRandomUser command");
			}
		}

		[Command("онлайн")]
		[Summary("Отображает некоторую информацию о дискорд сервере.")]
		public async Task ServerInfoAsync()
		{
			try
			{
				var guild = Context.Guild;
				await guild.DownloadUsersAsync();


				var stat = new UsersInStatuses
				{
					TotalUsers = guild.Users.Count,
					UsersAFK = 0,
					UsersDnD = 0,
					UsersInvoice = 0,
					UsersOffline = 0,
					UsersOnline = 0,
					UsersPlaying = 0,
					UsersInDestiny = 0
				};

				var options = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
				Parallel.ForEach(guild.Users, options, user =>
				{
					//Playing game?
					if (user.Activity != null)
						Interlocked.Increment(ref stat.UsersPlaying);
					//User playing Destiny 2?
					if (user.Activity?.Name == "Destiny 2")
						Interlocked.Increment(ref stat.UsersInDestiny);
					//Sit in voice channel of current guild?
					if (user.VoiceState.HasValue)
						Interlocked.Increment(ref stat.UsersInvoice);
					//User current status
					if (user.Status == UserStatus.Online)
						Interlocked.Increment(ref stat.UsersOnline);
					else if (user.Status == UserStatus.Offline || user.Status == UserStatus.Invisible)
						Interlocked.Increment(ref stat.UsersOffline);
					else if (user.Status == UserStatus.Idle || user.Status == UserStatus.AFK)
						Interlocked.Increment(ref stat.UsersAFK);
					else if (user.Status == UserStatus.DoNotDisturb)
						Interlocked.Increment(ref stat.UsersDnD);
				});

				// TODO: GuildInfo Embed
				//await ReplyAsync(embed: EmbedsHelper.GuildInfo(Context, stat, NeiraWebsite));
			}
			catch (Exception ex)
			{
				await ReplyAsync($"Ошибка: {ex.Message}");
				logger.LogWarning(ex, "Online command");
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

		private Embed MailingEmbed()
		{
			var embed = new EmbedBuilder
			{
				Title = $":mailbox_with_mail: Вам сообщение от {Context.User.Username} с сервера **`{Context.Guild.Name}`**",
				Color = Color.LightOrange,
				ThumbnailUrl = Context.Guild.IconUrl,
				Description = message,
			};
			embed.WithFooter("Страж, учти что я не имею отношения к содержимому данного сообщения. | neira.su");
			embed.WithCurrentTimestamp();
		}
		#endregion
	}
}
