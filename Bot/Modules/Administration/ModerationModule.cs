using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Bot.Helpers;
using Bot.Extensions;
using Bot.Preconditions;
using Bot.Models.Db.Discord;

namespace Bot.Modules.Administration
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
	[Cooldown(5)]
	public class ModerationModule : BotModuleBase
	{
		public static string ConvertBoolean(bool? boolean)
		{
			return boolean == true ? "**Да**" : "**Нет**";
		}

		[Command("настройки")]
		[Summary("Эта команда выводит мои настройки для текущего корабля, так же содержит некоторую полезную и не очень информацию.")]
		public async Task GetGuildConfig()
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			#region Data
			var OwnerName = Context.Guild.Owner.Nickname ?? Context.Guild.Owner.Username;
			string FormattedCreatedAt = Context.Guild.CreatedAt.ToString("dd-MM-yyyy");
			string logs = ConvertBoolean(guild.EnableLogging);
			string news = ConvertBoolean(guild.EnableNotification);
			#endregion

			#region Message
			var embed = new EmbedBuilder()
				.WithColor(Color.Orange)
				.WithTitle($"Мои настройки на этом корабле.")
				.WithThumbnailUrl(Context.Guild.IconUrl)
				.WithDescription($"Клан **{Context.Guild.Name}** имеет свой корабль с **{FormattedCreatedAt}**, капитаном корабля в данный момент является **{OwnerName}**")
				.AddField("Сейчас на корабле:",
				$"Всего каналов: **{Context.Guild.Channels.Count}**\n" +
				$"Стражей на корабле: **{Context.Guild.Users.Count}**")
				.AddField("Новостной канал", $"В данный момент используется **<#{guild.NotificationChannel}>** для сообщений о Зур-е.")
				.AddField("Оповещения о Зур-е включены?", news)
				.AddField("Технический канал", $"В данный момент используется **<#{guild.LoggingChannel}>** для сервисных сообщений клана.")
				.AddField("Тех. сообщения включены?", logs)
				.WithFooter("Это сообщение будет автоматически удалено через 2 минуты.");
			#endregion

			await ReplyAsync(embed: embed.Build());
		}

		[Command("новости")]
		[Summary("Эту команду нужно писать в том канале, где ты хочешь чтобы я отправляла информационные сообщения, например о Зуре.")]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		public async Task SetNotificationChannel()
		{
			// Get or create personal guild settings
			Guild guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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
				embed.Description = $"В данный момент у меня записанно что все новости о Зур-е я должна отправлять в **<#{guild.NotificationChannel}>**.";
			}
			embed.AddField("Оповещения о Зур-е включены?", ConvertBoolean(guild.EnableNotification));
			embed.WithFooter($"Хотите я запишу этот канал как новостной? Если да - нажмите {HeavyCheckMark}, если нет - нажмите {RedX}. Это сообщение будет автоматически удалено через 1 минуту.");
			#endregion

			var message = await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));

			bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

			if (choice == true)
			{
				guild.NotificationChannel = Context.Channel.Id;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
		}

		[Command("логи")]
		[Summary("Эту команду нужно писать в том канале, где ты хочешь, чтобы я отправляла туда сервисные сообщения, например о том, что кто-то покинул сервер.")]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		public async Task SetLogChannel()
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

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
				embed.Description = $"В данный момент у меня записанно что все сервисные сообщения я должна отправлять в **<#{guild.LoggingChannel}>**.";
			}
			embed.AddField("Cервисные сообщения включены?", ConvertBoolean(guild.EnableLogging));
			embed.WithFooter($"Хотите я запишу этот канал как технический? Если да - нажмите {HeavyCheckMark}, если нет - нажмите {RedX}. Это сообщение будет автоматически удалено через 1 минуту.");
			#endregion

			var message = await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));

			//Если true обновляем id лог канала.
			bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

			if (choice == true)
			{
				guild.LoggingChannel = Context.Channel.Id;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
		}

		[Command("статус логов")]
		[Summary("Эта команда позволяет включить или выключить Тех. сообщения.")]
		public async Task ToggleLogging()
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			#region Message
			EmbedBuilder embed = new EmbedBuilder()
			.WithColor(Color.Orange)
			.WithTitle("Технические сообщения")
			.WithDescription($"В данный момент все технические сообщения я отправляю в канал **<#{guild.LoggingChannel}>**")
			.AddField("Оповещения включены?", ConvertBoolean(guild.EnableLogging))
			.WithFooter($"Для включения - нажми {HeavyCheckMark}, для отключения - нажми {RedX}, или ничего не нажимай. Это сообщение будет автоматически удалено через 1 минуту.");
			#endregion

			var message = await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));

			//Если true или false обновляем включено или выключено логирование для гильдии, в противном случае ничего не делаем.
			bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

			if (choice == true)
			{
				guild.EnableLogging = true;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
			else if (choice == false)
			{
				guild.EnableLogging = false;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}

		}

		[Command("статус новостей")]
		[Summary("Эта команда позволяет включить или выключить оповещения о Зур-е.")]
		public async Task ToggleNews()
		{
			// Get or create personal guild settings
			Guild guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			#region Message
			EmbedBuilder embed = new EmbedBuilder()
				.WithColor(Color.Orange)
				.WithTitle("Новостные сообщения")
				.WithDescription($"В данный момент все новостные сообщения о Зур-е я отправляю в канал **<#{guild.NotificationChannel}>**")
				.AddField("Оповещения включены?", ConvertBoolean(guild.EnableNotification))
				.WithFooter($"Для включения - нажми {HeavyCheckMark}, для отключения - нажми {RedX}, или ничего не нажимай. Это сообщение будет автоматически удалено через 1 минуту.");
			#endregion

			var message = await ReplyAndDeleteAsync(null, embed: embed.Build(), timeout: TimeSpan.FromMinutes(1));

			//Если true или false обновляем включено или выключено логирование для гильдии, в противном случае ничего не делаем.
			bool? choice = await CommandContextExtensions.GetUserConfirmationAsync(Context, message.Content);

			if (choice == true)
			{
				guild.EnableNotification = true;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
			else if (choice == false)
			{
				guild.EnableNotification = false;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
		}

		[Command("посмотреть приветствие")]
		[Summary("Позволяет посмотреть, как будет выглядеть сообщение-приветствие новоприбывшему на сервер.")]
		public async Task WelcomeMessagePreview()
		{
			// Get or create personal guild settings
			var guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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

			await ReplyAndDeleteAsync(":smiley: Приветственное сообщение сохранено.");
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

	}
}