using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

using Core;
using Core.Models.Db;

using DiscordBot.Helpers;
using DiscordBot.Extensions;
using DiscordBot.Preconditions;

using API.Bungie;

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

		[Command("Клан")]
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
				.AddField("Команда: **!клан инфо**", "Эта команда выводит мои настройки для текущей гильдии, так же содержит некоторую полезную и не очень информацию.")
				.AddField("Команда: **!клан статус <ID гильдии>**", "Эта команда выводит онлайн соклановцев указаной гильдии. Более детальную информацю о команде ты можешь получить просто написав **!клан статус**")
				.AddField("Команда: **!клан новости**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда информационные сообщения например о Зуре.")
				.AddField("Команда: **!новости статус**", "Эта команда позволяет включить или выключить оповещения о Зур-е")
				.AddField("Команда: **!клан логи**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда сервисные сообщения например о том что кто-то покинул сервер.")
				.AddField("Команда: **!логи статус**", "Эта команда позволяет включить или выключить Тех. сообщения.")
				.AddField("Команда: **!посмотреть приветствие**", "Позволяет посмотреть, как будет выглядеть сообщение-приветствие новоприбывшему на сервер.")
				.AddField("Команда: **!сохранить приветствие <Message>**", "Сохраняет сообщение-приветствие и включает механизм отправки сообщения.\nПоддерживает синтаксис MarkDown для красивого оформления.")
				.WithFooter($"Любые предложения по улучшению или исправлении ошибок пожалуйста сообщи моему создателю {app.Owner.Username}#{app.Owner.Discriminator}", app.Owner.GetAvatarUrl());
			#endregion

			await Context.Channel.SendMessageAsync(null, false, embed.Build());
		}

		[Command("Клан инфо")]
		[Summary("Отображает все настройки бота в гильдии где была вызвана комманда.")]
		[Cooldown(5)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task GetGuildConfig()
		{
			// Get or create personal guild settings
			Guild guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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

		[Command("Клан новости")]
		[Summary("Сохраняет ID канала для использования в новостных сообщениях.")]
		[Cooldown(5)]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task SetNotificationChannel()
		{
			// Get or create personal guild settings
			Guild guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
		}

		[Command("Клан логи")]
		[Summary("Сохраняет ID канала для использования в тех сообщениях.")]
		[Cooldown(5)]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task SetLogChannel()
		{
			// Get or create personal guild settings
			Guild guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
		}


		[Command("Клан статус")]
		[Summary("Возвращает результат онлайна соклановцев заданой гильдии")]
		[Cooldown(5)]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task GetGuildInfoFromBungie(int GuildId = 0)
		{
			try
			{
				#region Checks
				if (GuildId == 0)
				{
					await Context.Channel.SendMessageAsync("Капитан, ты не указал ID гильдии.\nЧтобы узнать ID достаточно открыть любой клан на сайте Bungie, например: <https://www.bungie.net/ru/ClanV2?groupid=3526561> и скопировать цифры после groupid=\n Синтаксис команды простой: **!клан статус 3526561**");
					return;
				}
				#endregion
				//Send calculating message because stastic forming near 30-50 sec.
				var message = await Context.Channel.SendMessageAsync("Это займет некоторое время.\nНачинаю проводить подсчет.");

				//Initialize Bungie GroupV2
				BungieApi bungieApi = new BungieApi();
				var membersOfGroup =  await bungieApi.GetMembersOfGroupResponse(GuildId);
				//Get GroupV2 main info
				//var GuildInfo = weekStat.GuildInfoAsync(GuildId).Result.Response;
				//Get GroupV2 Member info
				//var GuildMembers = await weekStat.GetGuildCharacterIdsAsync();

				//EmbedBuilder embed = new EmbedBuilder();
				//embed.WithTitle($"Онлайн статус стражей");
				//embed.WithColor(Color.Orange);
				////Bungie Clan link
				//embed.WithUrl($"https://www.bungie.net/ru/ClanV2?groupId={GuildInfo.Detail.GroupId}");
				////Some clan main info
				//embed.WithDescription($"Клан зарегистрирован **{GuildInfo.Detail.CreationDate.ToString("dd-MM-yyyy")}**\n" +
				//	$"В данный момент в клане **{GuildInfo.Detail.MemberCount}**/100 стражей.\n" +
				//	$"Девиз клана - **{GuildInfo.Detail.Motto}**");

				//#region list for member sorted for some days
				//List<string> _ThisDay = new List<string>();
				//List<string> _Yesterday = new List<string>();
				//List<string> _FewDays = new List<string>();
				//List<string> _ThisWeek = new List<string>();
				//List<string> _MoreOneWeek = new List<string>();
				//#endregion

				////Main Sorting logic
				//foreach (var item in GuildMembers)
				//{
				//	//Property for calculate how long days user did not enter the Destiny
				//	var days = (DateTime.Today.Day - item.LastOnlineDate.Day);

				//	//Sorting user to right list
				//	if (days == 0)
				//	{
				//		_ThisDay.Add(item.MemberName);
				//	}
				//	else if (days == 1)
				//	{
				//		_Yesterday.Add(item.MemberName);
				//	}
				//	else if (days > 1 && days < 5)
				//	{
				//		_FewDays.Add(item.MemberName);
				//	}
				//	else if (days > 4 && days < 7)
				//	{
				//		_ThisWeek.Add(item.MemberName);
				//	}
				//	else
				//	{
				//		//ProfileId is empty if user hide his profile from public
				//		if (item.ProfileId == string.Empty)
				//			_MoreOneWeek.Add($"{item.MemberName}(**Профиль скрыт**)");
				//		else
				//			_MoreOneWeek.Add($"[{item.MemberName}](https://www.bungie.net/ru/Profile/254/{item.ProfileId}/)");

				//	}
				//}

				////Create one string who enter to the game today, like "Petya,Vasia,Grisha",
				////and if string ThisDay not empty add to embed message special field.
				//string ThisDay = string.Join(',', _ThisDay);
				//if (!string.IsNullOrEmpty(ThisDay))
				//	embed.AddField("Был сегодня", ThisDay);
				////Same as above, but who enter to the game yesterday
				//string Yesterday = string.Join(',', _Yesterday);
				//if (!string.IsNullOrEmpty(Yesterday))
				//	embed.AddField("Был вчера", Yesterday);
				////Same as above, but who enter to the game more 2 days but fewer 4 days ago
				//string FewDays = string.Join(',', _FewDays);
				//if (!string.IsNullOrEmpty(FewDays))
				//	embed.AddField("Был в течении пары дней", FewDays);
				////Same as above, but who enter to the game more 5 days but fewer 7 days ago
				//string ThisWeek = string.Join(',', _ThisWeek);
				//if (!string.IsNullOrEmpty(ThisWeek))
				//	embed.AddField("Был на этой неделе", ThisWeek);
				////Same as above, but who enter to the game more 7 days ago
				//string MoreOneWeek = string.Join(',', _MoreOneWeek);
				//if (!string.IsNullOrEmpty(MoreOneWeek))
				//	embed.AddField("Был больше недели тому назад", MoreOneWeek);
				////Simple footer with clan name
				//embed.WithFooter($"Данные о клане {GuildInfo.Detail.Name}");
				////Mention user with ready statistic
				await Context.Channel.SendMessageAsync($"Бип! {Context.User.Mention}. Статистика подсчитана.", false/*, embed.Build()*/);

				//Delete message from start this command
				await Context.Channel.DeleteMessageAsync(message);
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"ClanStatus Command - {ex.Source}", ex.Message, ex.InnerException));
				Console.WriteLine(ex.ToString());
			}
		}

		[Command("Логи статус")]
		[Summary("Вкл. или Выкл. тех. сообщения.")]
		[Cooldown(5)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task ToggleLogging()
		{
			// Get or create personal guild settings
			Guild guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
			else if (choice == false)
			{
				guild.EnableLogging = false;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}

		}

		[Command("Новости статус")]
		[Summary("Включает или выключает новости о зуре")]
		[Cooldown(5)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task ToggleNews()
		{
			// Get or create personal guild settings
			Guild guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

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
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
			else if (choice == false)
			{
				guild.EnableNotification = false;
				await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);
			}
		}

		[Command("Посмотреть приветствие")]
		[Summary("Команда предпросмотра приветственного сообщения")]
		[Cooldown(5)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task WelcomeMessagePreview()
		{
			// Get or create personal guild settings
			var guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

			if (string.IsNullOrWhiteSpace(guild.WelcomeMessage))
			{
				await Context.Channel.SendMessageAsync($":x: | В данный момент я не отправляю какое либо сообщение новоприбывшим. Для добавления или редактирования сообщения отправь команду **!сохранить приветствие <текст сообщения>**");
				return;
			}

			await Context.Channel.SendMessageAsync($"{Context.User.Mention} вот так выглядит сообщение для новоприбывших в Discord.", embed: MiscHelpers.WelcomeEmbed(Context.Guild.CurrentUser).Build());
		}

		[Command("Сохранить приветствие")]
		[Summary("Сохраняет сообщение для отправки всем кто пришел в гильдию")]
		[Cooldown(5)]
		[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task SaveWelcomeMessage([Remainder]string message)
		{
			// Get or create personal guild settings
			var guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;

			//Dont save empty welcome message.
			if (string.IsNullOrWhiteSpace(message)) return;

			guild.WelcomeMessage = message;

			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);

			await Context.Channel.SendMessageAsync(":smiley: Приветственное сообщение сохранено.");
		}
	}
}
