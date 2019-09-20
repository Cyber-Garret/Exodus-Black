using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Neira.Bot.Helpers;
using Neira.Bot.Preconditions;
using Neira.Db.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Bot.Modules.Administration
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
	[Cooldown(5)]
	public class ModerationModule : BotModuleBase
	{
		private readonly DiscordSocketClient client;
		public ModerationModule(DiscordSocketClient socketClient)
		{
			client = socketClient;
		}

		[Command("настройки")]
		[Summary("Эта команда выводит мои настройки, так же содержит некоторую полезную и не очень информацию.")]
		public async Task GetGuildConfig()
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			var OwnerName = Context.Guild.Owner.Nickname ?? Context.Guild.Owner.Username;
			string FormattedCreatedAt = Context.Guild.CreatedAt.ToString("dd-MM-yyyy");

			var embed = new EmbedBuilder()
				.WithColor(Color.Orange)
				.WithAuthor($"Мои настройки на этом сервере.", client.CurrentUser.GetAvatarUrl() ?? client.CurrentUser.GetDefaultAvatarUrl())
				.WithThumbnailUrl(Context.Guild.IconUrl)
				.WithDescription($"Сервер **{Context.Guild.Name}** зарегистрирован **{FormattedCreatedAt}**, владельцем сервера является **{OwnerName}**")
				.AddField("Основная информация:",
				$"- Всего каналов: **{Context.Guild.Channels.Count}**\n" +
				$"- Стражей на корабле: **{Context.Guild.Users.Count}**\n" +
				$"- Оповещения о Зуре я присылаю в **<#{guild.NotificationChannel}>**\n" +
				$"- Логи сервера я пишу в **<#{guild.LoggingChannel}>**\n" +
				$"- Оповещения о новых стражах я присылаю в **<#{guild.WelcomeChannel}>**\n" +
				$"- Глобальное упоминание в некоторых сообщениях: **{guild.GlobalMention}**");

			await ReplyAsync(null, embed: embed.Build());
		}

		[Command("новости")]
		[Summary("Команда позволяет включать или выключать оповещения о Зуре в определенный текстовый канал.")]
		[Remarks("Для выключения оповещений напиши **!новости**. Для включения **!новости #новостной-канал**. ")]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		public async Task SetNotificationChannel(ITextChannel channel = null)
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Новостной канал");
			if (channel == null)
			{
				embed.WithColor(Color.Red)
					.WithDescription("Я выключила оповещения о Зуре.")
					.WithFooter($"Это сообщение будет автоматически удалено через 1 минуту.");
				guild.NotificationChannel = 0;
			}
			else
			{
				embed.WithColor(Color.Gold)
					.WithDescription($"Теперь новости о Зуре я буду присылать в канал {channel.Mention}")
					.WithFooter($"Капитан, убедись пожалуйста, что в канале {channel.Name} у меня есть право [Отправлять сообщения]. Это сообщение будет автоматически удалено через 1 минуту.");
				guild.NotificationChannel = channel.Id;
			}

			await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));
			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
		}

		[Command("логи")]
		[Summary("Команда позволяет включать или выключать оповещения об изменениях на сервер, например, когда кто-то покинул сервер.")]
		[Remarks("Для выключения оповещений напиши **!логи**. Для включения **!логи #нейра-логи**. ")]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		public async Task SetLogChannel(ITextChannel channel = null)
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Технический канал");
			if (channel == null)
			{
				embed.WithColor(Color.Red)
					.WithDescription("Я выключила оповещения об изменениях на сервере.")
					.WithFooter($"Это сообщение будет автоматически удалено через 1 минуту.");
				guild.LoggingChannel = 0;
			}
			else
			{
				embed.WithColor(Color.Gold)
					.WithDescription($"Теперь большинство изменений на сервере, если у меня конечно есть права, я буду оповещать в канал {channel.Mention}")
					.WithFooter($"Капитан, убедись пожалуйста, что в канале {channel.Name} у меня есть право [Отправлять сообщения]. Это сообщение будет автоматически удалено через 1 минуту.");
				guild.LoggingChannel = channel.Id;
			}

			await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));
			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
		}

		[Command("приветствие")]
		[Summary("Команда позволяет включать или выключать оповещения о новых участниках сервера в стиле мира Destiny.")]
		[Remarks("Для выключения оповещений напиши **!приветствие**. Для включения **!логи #флудилка**. ")]
		public async Task SetWelcomeChannel(ITextChannel channel = null)
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Приветственный канал");
			if (channel == null)
			{
				embed.WithColor(Color.Red)
					.WithDescription("Я выключила оповещения о новых участниках на сервере.")
					.WithFooter($"Это сообщение будет автоматически удалено через 1 минуту.");
				guild.WelcomeChannel = 0;
			}
			else
			{
				embed.WithColor(Color.Gold)
					.WithDescription($"Теперь я буду оповещать о новых участниках в канал {channel.Mention}")
					.WithFooter($"Капитан, убедись пожалуйста, что в канале {channel.Name} у меня есть право [Прикреплять файлы]. Это сообщение будет автоматически удалено через 1 минуту.");
				guild.WelcomeChannel = channel.Id;
			}

			await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));
			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
		}

		[Command("посмотреть приветствие")]
		[Summary("Позволяет посмотреть, как будет выглядеть сообщение-приветствие новоприбывшему на сервер.")]
		public async Task WelcomeMessagePreview()
		{
			// Get or create personal guild settings
			var guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			if (string.IsNullOrWhiteSpace(guild.WelcomeMessage))
			{
				await ReplyAndDeleteAsync($":x: | В данный момент я не отправляю какое либо сообщение новоприбывшим. Для добавления или редактирования сообщения отправь команду **!сохранить приветствие <текст сообщения>**");
				return;
			}

			await ReplyAsync($"{Context.User.Mention} вот так выглядит сообщение для новоприбывших в Discord. *Это сообщение будет автоматически удалено через 2 минуты.*",
							 embed: MiscHelpers.WelcomeEmbed(Context.Guild.CurrentUser).Build());
		}

		[Command("сохранить приветствие")]
		[Summary("Сохраняет сообщение-приветствие и включает механизм отправки сообщения всем новоприбывшим на сервер.\nПоддерживает синтаксис MarkDown для красивого оформления.")]
		[Remarks("Пример: !сохранить приветствие <Сообщение>")]
		public async Task SaveWelcomeMessage([Remainder]string message)
		{
			// Get or create personal guild settings
			var guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			//Dont save empty welcome message.
			if (string.IsNullOrWhiteSpace(message)) return;

			guild.WelcomeMessage = message;

			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);

			await ReplyAsync(":smiley: Приветственное сообщение сохранено.");
		}

		[Command("префикс")]
		[Summary("Позволяет администраторам изменить префикс команд для текущего сервера. ")]
		public async Task GuildPrefix(string prefix = null)
		{
			var config = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			string message;
			if (prefix == null)
			{
				config.CommandPrefix = null;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, config);

				message = $"Для команд установлен префикс по умолчанию **!**";
			}
			else
			{
				config.CommandPrefix = prefix;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, config);

				message = $"Для команд установлен префикс **{prefix}**";
			}

			await ReplyAsync(message);
		}

		[Command("автороль")]
		[Summary("Сохраняет роль, которую я буду выдавать всем новым пользователям, пришедшим на сервер.\n" +
			"**Важно! Моя роль должна быть над ролью, которую я буду автоматически выдавать всем новоприбывшим. Имеется ввиду в списке Ваш сервер->Настройки сервера->Роли.**")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task AutoRoleRoleAdd(IRole _role)
		{
			var guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);
			guild.AutoroleID = _role.Id;
			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);

			var embed = new EmbedBuilder();
			embed.WithDescription($"Сохранена роль **{_role.Name}**, теперь я буду ее автоматически выдавать всем прибывшим.");
			embed.WithColor(Color.Gold);
			embed.WithFooter("Пожалуйста, убедись, что моя роль выше авто роли и у меня есть права на выдачу ролей. Тогда я без проблем буду выдавать роль всем прибывшим на корабль и сообщать об этом в сервисных сообщениях. Это сообщение будет автоматически удалено через 2 минуты.");

			await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(2));
		}

		[Command("упоминание")]
		[Summary("Изменяет упоминания в сборах и уведомлениях о Зуре here на everyone и наоборот.")]
		public async Task SetGuildMention()
		{
			try
			{
				var guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);
				if (guild.GlobalMention == "@here")
					guild.GlobalMention = "@everyone";
				else if (guild.GlobalMention == "@everyone")
					guild.GlobalMention = "@here";
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);

				await ReplyAsync($"Капитан, теперь в некоторых сообщениях я буду использовать {guild.GlobalMention}");
			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync($"Капитан, произошла непредвиденная ошибка. В сообщении: {ex.Message}. Бип...");
				await Logger.Log(new LogMessage(LogSeverity.Critical, "SetGuildMention command", ex.Message, ex));
			}
		}

		[Command("рассылка")]
		[Summary("Рассылает личные сообщения стражам указанной роли. По окончании работы я предоставлю небольшую статистику кому я смогла отправить, а кому нет.")]
		[Remarks("Пример: **!рассылка <Роль> <Текст сообщения>**\n!рассылка @Тест Привет, завтра у нас клановый сбор в дискорде.")]
		public async Task SendMessage(IRole _role, [Remainder] string message)
		{
			var GuildOwner = Context.Guild.OwnerId;
			if (Context.User.Id != GuildOwner)
			{
				await ReplyAndDeleteAsync(":x: | Прошу прощения страж, но эта команда доступна только капитану корабля!");
				return;
			}

			var workMessage = await Context.Channel.SendMessageAsync("Приступаю к рассылке сообщений.");
			var Users = Context.Guild.Users;
			var role = Context.Guild.Roles.FirstOrDefault(r => r.Id == _role.Id);
			int SucessCount = 0;
			int FailCount = 0;
			foreach (var user in Users)
			{
				if (user.Roles.Contains(role))
				{
					try
					{
						var DM = await user.GetOrCreateDMChannelAsync();

						EmbedBuilder embed = new EmbedBuilder();
						embed.WithAuthor($"Сообщение от {Context.User.Username}");
						embed.WithColor(Color.Gold);
						embed.WithDescription(message);
						embed.WithThumbnailUrl(Context.Guild.IconUrl);
						embed.WithCurrentTimestamp();

						await DM.SendMessageAsync(null, false, embed.Build());
						SucessCount += 1;
					}
					catch (Exception ex)
					{
						FailCount += 1;
						await Logger.Log(new LogMessage(LogSeverity.Error, "SendMessage command", ex.Message, ex));
					}
				}
			}
			await workMessage.ModifyAsync(m => m.Content =
			$"Готово. Я разослала сообщением всем у кого есть роль {role.Name}.\n" +
			$"- Всего получателей: {SucessCount + FailCount}\n" +
			$"- Успешно доставлено: {SucessCount}\n" +
			$"- Не удалось отправить: {FailCount}");
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
				await Logger.Log(new LogMessage(LogSeverity.Error, "PurgeChat", ex.Message, ex));
				await ReplyAsync($"Ошибка очистки канала от {amount} сообщений. {ex.Message}.");
			}
		}

		[Command("опрос")]
		[Summary("Создает голосование среди стражей. Поддерживает разметку MarkDown.")]
		[Remarks("Синтаксис: !опрос <текст сообщение>\nПример: !опрос Добавляем 10 рейдовых каналов?")]
		public async Task StartPoll([Remainder] string input)
		{
			var embed = new EmbedBuilder()
				.WithAuthor($"Голосование от {Context.User.Username}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
				.WithColor(Color.Green)
				.AddField($"Тема голосования", input);

			var msg = await ReplyAsync(embed: embed.Build());

			await msg.AddReactionsAsync(new IEmote[] { WhiteHeavyCheckMark, RedX });
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
				//Chose right field name by count
				if (count == 1)
					field.Name = "Капитан, генератор псевдослучайных чисел Вексов отобразил имя этого стража:";
				else
					field.Name = "Капитан, генератор псевдослучайных чисел Вексов отобразил имя этих стражей:";
				for (int i = 0; i < count; i++)
				{
					var num = Global.GetRandom.Next(0, filteredusers.Count());
					//Pick random user
					var user = filteredusers.ElementAt(num);

					field.Value += $"#{i + 1} {user.Mention} - {user.Nickname ?? user.Username}\n";
				}
				embed.AddField(field);

				await ReplyAsync(embed: embed.Build());
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "GetRandomUser command", ex.Message, ex));
				await ReplyAndDeleteAsync($"Ошибка генератора псевдослучайных чисел Вексов: {ex.Message}");
			}
		}

	}
}