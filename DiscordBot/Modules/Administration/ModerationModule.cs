using System;
using System.Linq;
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
using API.Bungie.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Modules.Administration
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
	[Cooldown(5)]
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
				.AddField("Команда: **!клан новости**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда информационные сообщения например о Зуре.")
				.AddField("Команда: **!новости статус**", "Эта команда позволяет включить или выключить оповещения о Зур-е")
				.AddField("Команда: **!клан логи**", "Эту команду нужно писать в том чате где ты хочешь чтобы я отправляла туда сервисные сообщения например о том что кто-то покинул сервер.")
				.AddField("Команда: **!логи статус**", "Эта команда позволяет включить или выключить Тех. сообщения.")
				.AddField("Команда: **!посмотреть приветствие**", "Позволяет посмотреть, как будет выглядеть сообщение-приветствие новоприбывшему на сервер.")
				.AddField("Команда: **!сохранить приветствие <Message>**", "Сохраняет сообщение-приветствие и включает механизм отправки сообщения.\nПоддерживает синтаксис MarkDown для красивого оформления.")
				.AddField("Команда: **!автороль @Роль**", "Сохраняет ID роли, которую Нейроматрица будет выдавать всем новым пользователям, пришедшим на сервер.\n" +
				"**Важно! Роль Нейроматрицы должна быть над ролью, которую она будет автоматически выдавать.Имеется ввиду в списке Ваш сервер->Настройки сервера->Роли.**")
				.WithFooter($"Любые предложения по улучшению или исправлении ошибок пожалуйста сообщи моему создателю {app.Owner.Username}#{app.Owner.Discriminator}", app.Owner.GetAvatarUrl());
			#endregion

			await Context.Channel.SendMessageAsync(null, false, embed.Build());
		}

		[Command("Клан инфо")]
		[Summary("Отображает все настройки бота в гильдии где была вызвана комманда.")]
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
		[RequireBotPermission(ChannelPermission.SendMessages)]
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
		[RequireBotPermission(ChannelPermission.SendMessages)]
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

		[Command("Логи статус")]
		[Summary("Вкл. или Выкл. тех. сообщения.")]
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

		[Command("рассылка")]
		[RequireContext(ContextType.Guild, ErrorMessage = ":x: | Эта команда не доступна в личных сообщениях.")]
		public async Task SendMessage(IRole _role, [Remainder] string message)
		{
			var GuildOwner = Context.Guild.OwnerId;
			if (Context.User.Id != GuildOwner)
			{
				await Context.Channel.SendMessageAsync(":x: | Прошу прощения страж, но эта команда доступна только капитану корабля!");
				return;
			}

			var workMessage = await Context.Channel.SendMessageAsync("Приступаю к рассылке сообщений.");
			try
			{
				var Users = Context.Guild.Users;
				var role = Context.Guild.Roles.FirstOrDefault(r => r.Id == _role.Id);
				int count = 0;
				foreach (var user in Users)
				{
					if (user.Roles.Contains(role))
					{
						var DM = await user.GetOrCreateDMChannelAsync();

						EmbedBuilder embed = new EmbedBuilder();
						embed.WithAuthor($"Сообщение от {Context.User.Username}");
						embed.WithColor(Color.Gold);
						embed.WithDescription(message);
						embed.WithThumbnailUrl(Context.Guild.IconUrl);
						embed.WithCurrentTimestamp();

						await DM.SendMessageAsync(null, false, embed.Build());
						count += 1;
					}
				}
				await workMessage.ModifyAsync(m => m.Content = $"Готово. Я разослала сообщением всем у кого есть роль {role.Name}.\nСообщение получили {count} стражей.");
			}
			catch (Exception ex)
			{
				await workMessage.ModifyAsync(m => m.Content = "Ошибка рассылки! Сообщите моему создателю для исправления моих логических цепей.");
				await Logger.Log(new LogMessage(LogSeverity.Error, $"SendMessage command - {ex.Source}", ex.Message, ex.InnerException));
			}
		}

		[Command("Автороль")]
		[Summary("Автоматически добавляет роль всем новым пользователям")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task AutoRoleRoleAdd(IRole _role)
		{
			var guild = FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id).Result;
			guild.AutoroleID = _role.Id;
			await FailsafeDbOperations.SaveGuildAccountAsync(Context.Guild.Id, guild);

			var embed = new EmbedBuilder();
			embed.WithDescription($"Сохранена роль **{_role.Name}**, теперь я буду ее автоматически выдавать всем прибывшим.");
			embed.WithColor(Color.Gold);
			embed.WithFooter("Пожалуйста, убедись, что моя роль выше авто роли и у меня есть права на выдачу ролей. Тогда я без проблем буду выдавать роль всем прибывшим на корабль и сообщать об этом в сервисных сообщениях.");

			await Context.Channel.SendMessageAsync(null, false, embed.Build());
		}
	}
}
