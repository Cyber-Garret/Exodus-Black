using Discord;
using Discord.Rest;
using Discord.WebSocket;
using ImageMagick;
using Neira.Bot.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

namespace Neira.Bot.Services
{
	public class DiscordEventHandlerService
	{
		private readonly DiscordSocketClient Client;
		private readonly CommandHandlerService CommandHandlingService;
		private readonly LavaSocketClient lavaSocket;
		private readonly MilestoneService milestone;
		private readonly MusicService music;


		public DiscordEventHandlerService(CommandHandlerService command, DiscordSocketClient socketClient, MilestoneService milestoneService, LavaSocketClient lavaSocketClient, MusicService musicService)
		{
			Client = socketClient;
			CommandHandlingService = command;
			lavaSocket = lavaSocketClient;
			milestone = milestoneService;
			music = musicService;
		}

		public void Configure()
		{
			Client.Ready += Client_Ready;
			Client.Disconnected += Client_Disconnected;
			Client.JoinedGuild += _client_JoinedGuildAsync;
			Client.ChannelCreated += _client_ChannelCreatedAsync;
			Client.ChannelDestroyed += _client_ChannelDestroyedAsync;
			Client.GuildMemberUpdated += _client_GuildMemberUpdatedAsync;
			Client.MessageDeleted += _client_MessageDeletedAsync;
			Client.MessageReceived += Client_MessageReceived;
			Client.MessageUpdated += _client_MessageUpdatedAsync;
			Client.RoleCreated += _client_RoleCreatedAsync;
			Client.RoleDeleted += _client_RoleDeletedAsync;
			Client.UserJoined += _client_UserJoinedAsync;
			Client.UserLeft += _client_UserLeftAsync;
			Client.ReactionAdded += Client_ReactionAdded;
			Client.ReactionRemoved += Client_ReactionRemoved;
		}

		#region Events
		private Task Client_Ready()
		{
			Task.Run(async () =>
			{
				await lavaSocket.StartAsync(Client, new Configuration
				{
					LogSeverity = LogSeverity.Verbose,
					AutoDisconnect = true,
					InactivityTimeout = TimeSpan.FromMinutes(1),
					PreservePlayers = false
				});

				lavaSocket.Log += Logger.Log;
				lavaSocket.OnTrackFinished += music.OnTrackFinished;

				milestone.Initialize();
			});

			return Task.CompletedTask;
		}

		private Task Client_Disconnected(Exception arg)
		{
			Task.Run(async () =>
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "Neira Disconnected", arg.Message, arg));
				await lavaSocket.DisposeAsync();
			});

			return Task.CompletedTask;
		}
		private Task _client_JoinedGuildAsync(SocketGuild guild)
		{
			Task.Run(async () =>
			{
				await FailsafeDbOperations.GetGuildAccountAsync(guild.Id);
			});
			return Task.CompletedTask;
		}

		private async Task _client_ChannelCreatedAsync(SocketChannel arg) => await ChannelCreated(arg);
		private async Task _client_ChannelDestroyedAsync(SocketChannel arg) => await ChannelDestroyed(arg);
		private async Task _client_GuildMemberUpdatedAsync(SocketGuildUser userBefore, SocketGuildUser userAfter) => await GuildMemberUpdated(userBefore, userAfter);
		private Task Client_MessageReceived(SocketMessage message)
		{
			//Ignore messages from bots
			if (message.Author.IsBot) return Task.CompletedTask;

			//New Task for fix disconeting from Discord WebSockets by 1001 if current Task not completed.
			_ = Task.Run(async () =>
				 {
					 await CommandHandlingService.HandleCommandAsync(message);
				 });
			return Task.CompletedTask;
		}
		private async Task _client_MessageUpdatedAsync(Cacheable<IMessage, ulong> cacheMessageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
		{
			if (!cacheMessageBefore.HasValue)
				return;
			if (cacheMessageBefore.Value.Author.IsBot)
				return;
			await MessageUpdated(cacheMessageBefore, messageAfter, channel);
		}
		private async Task _client_MessageDeletedAsync(Cacheable<IMessage, ulong> cacheMessage, ISocketMessageChannel channel)
		{
			if (!cacheMessage.HasValue)
				return;
			if (cacheMessage.Value.Author.IsBot)
				return;
			await MessageDeleted(cacheMessage);
		}
		private async Task _client_RoleCreatedAsync(SocketRole arg) => await RoleCreated(arg);
		private async Task _client_RoleDeletedAsync(SocketRole arg) => await RoleDeleted(arg);
		private Task _client_UserJoinedAsync(SocketGuildUser arg)
		{
			Task.Run(async () =>
			{
				await UserJoined(arg);
				await UserWelcome(arg);
				await MiscHelpers.Autorole(arg);
			});
			return Task.CompletedTask;
		}

		private async Task _client_UserLeftAsync(SocketGuildUser arg) => await UserLeft(arg);
		private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!reaction.User.Value.IsBot)
				await milestone.HandleReactionAdded(cache, reaction);
		}
		private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!reaction.User.Value.IsBot)
				await milestone.HandleReactionRemoved(cache, reaction);
		}
		#endregion

		#region Methods
		private async Task ChannelCreated(IChannel arg)
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
				var guild = FailsafeDbOperations.GetGuildAccountAsync(currentIGuildChannel.Guild.Id).Result;
				if (guild.LoggingChannel != 0)
				{
					await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "ChannelCreated", ex.Message, ex));
			}

		}
		private async Task ChannelDestroyed(IChannel arg)
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
					var guild = FailsafeDbOperations.GetGuildAccountAsync(currentIguildChannel.Guild.Id).Result;
					if (guild.LoggingChannel != 0)
					{
						await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "ChannelDestroyed", ex.Message, ex));
			}
		}
		private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
		{
			try
			{
				#region Checks
				if (after == null || before == after || before.IsBot)
					return;
				#endregion

				#region Data
				var guild = FailsafeDbOperations.GetGuildAccountAsync(before.Guild.Id).Result;
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

					if (guild.LoggingChannel != 0)
					{
						await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
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

					if (guild.LoggingChannel != 0)
					{
						await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}
				#endregion

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "GuildMemberUpdated", ex.Message, ex));
			}

		}
		private async Task MessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel arg3)
		{
			try
			{
				if (arg3 is IGuildChannel currentIGuildChannel)
				{
					var guild = FailsafeDbOperations.GetGuildAccountAsync(currentIGuildChannel.Guild.Id).Result;
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


					if (guild.LoggingChannel != 0)
					{

						await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "MessageUpdated", ex.Message, ex));
			}

		}
		private async Task MessageDeleted(Cacheable<IMessage, ulong> messageBefore)
		{
			try
			{
				if (messageBefore.Value.Author.IsBot)
					return;
				if (messageBefore.Value.Channel is ITextChannel textChannel)
				{
					var guild = FailsafeDbOperations.GetGuildAccountAsync(textChannel.Guild.Id).Result;

					var log = await textChannel.Guild.GetAuditLogsAsync(1);
					var audit = log.ToList();

					var name = $"{messageBefore.Value.Author.Mention}";
					var check = audit[0].Data as MessageDeleteAuditLogData;

					//if message deleted by bot finish Task.
					if (audit[0].User.IsBot) return;

					if (check?.ChannelId == messageBefore.Value.Channel.Id && audit[0].Action == ActionType.MessageDeleted)
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

					if (guild.LoggingChannel != 0)
					{

						await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embedDel.Build());
					}

				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "MessageDeleted", ex.Message, ex));
			}

		}
		private async Task RoleCreated(SocketRole arg)
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

				var guild = FailsafeDbOperations.GetGuildAccountAsync(arg.Guild.Id).Result;

				if (guild.LoggingChannel != 0)
				{
					await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "RoleCreated", ex.Message, ex));
			}

		}
		private async Task RoleDeleted(SocketRole arg)
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

				var guild = FailsafeDbOperations.GetGuildAccountAsync(arg.Guild.Id).Result;

				if (guild.LoggingChannel != 0)
				{
					await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "RoleDeleted", ex.Message, ex));
			}

		}
		private async Task UserJoined(SocketGuildUser user)
		{
			try
			{
				#region Checks
				if (user == null || user.IsBot) return;

				var guild = FailsafeDbOperations.GetGuildAccountAsync(user.Guild.Id).Result;
				if (string.IsNullOrWhiteSpace(guild.WelcomeMessage)) return;
				#endregion

				IDMChannel dM = await user.GetOrCreateDMChannelAsync();

				await dM.SendMessageAsync(null, false, MiscHelpers.WelcomeEmbed(user).Build());
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "UserJoined", ex.Message, ex));
			}

		}
		private async Task UserWelcome(SocketGuildUser user)
		{
			try
			{
				var guild = await FailsafeDbOperations.GetGuildAccountAsync(user.Guild.Id);
				if (guild.WelcomeChannel == 0) return;
				if (!(Client.GetChannel(guild.WelcomeChannel) is SocketTextChannel channel)) return;
				string[] randomWelcome =
					{
					"Опять Кабал? ©Ашер",
					"Бип. ©Нейра",
					"Я использовала часть своего кода, программируя эту броню! Но если умрешь, во всем виноват будешь ты, а не я ©Нейра",
					"Капитан, вы знали, что я подслушивала за Гоулом, Хм... Я думала он вас на атомы распылит ©Нейра",
					"Раз уж вы здесь, не хотите ненадолго остаться?.. Несколько тысяч лет меня устроит ©Нейра",
					"\"Мотиватор дня\" Не сдавайся детка! \"Конец записи\"©Нейра",
					"Значит им танков сколько захочешь, а мне ни одного? ©Кейд-6",
					"Свет живет во всем вокруг и во всех нас. Можешь заслонить его, можешь даже попытаться запереть его. Но он всегда найдет выход. - ©Глашатай Странника.",
					"Окей, значит так... Эм... Вы - сборище неудачников. Но раз других поубивали, то и вы сойдёте. ©Кейд-6",
					"Короче, всё пропало. Мой шмот, ваш шмот... Важнее, конечно, мой шмот. ©Кейд-6",
					"Пришло время новых легенд.",
					"Это - конец вашего пути. ©Гоул",
					"Все любят плохую идею, если она сработала. ©Кейд-6",
					"Эй, как насчет того, чтобы ты встал здесь и делал мою работу? А я пойду и буду делать твою. Которая заключается в том, чтобы тут околачиваться. ©Кейд-6",
					"Тут не библиотека. Проходи, не задерживайся. ©Кейд-6",
					"Да, ты клёвый и всё такое, но проваливай. ©Кейд-6",
					"Так, стоп, погоди... отойди назад... ещё... вот так нормально. ©Кейд-6",
					"Убери свой камень с моей карты. ©Кейд-6",
					"(Смешно пародируя голос Кабалов) Отдайте нам Праймуса, или мы взорвем корабль. ©Кейд-6",
					"Если ты увидишь их... просто пристрели. ©Кейд-6",
					"Расслабься, он работает нормально. Приготовься для воскрешения, Призрак. ©Кейд-6",
					"(Шепотом) Ты мой любимчик. Тссс, никому не говори. ©Кейд-6",
					"Я бы хотел постоять тут с тобой весь день, но... Я соврал, я бы совсем не хотел стоять тут с тобой весь день. ©Кейд-6",
					"Они убили Кейда! Сволочи!",
					"Так я прав... или я прав? ©Кейд-6",
					"Сколько раз стиралась моя система? 41,42,43? ©Банши-44" };

				string welcomeMessage = randomWelcome[Global.GetRandom.Next(randomWelcome.Length)];
				string background = Path.Combine(Directory.GetCurrentDirectory(), "UserData", "WelcomeBg", $"bg{Global.GetRandom.Next(1, 31)}.jpg");

				using (var image = new MagickImage(background, 512, 200))
				{
					var readSettings = new MagickReadSettings
					{
						FillColor = MagickColors.GhostWhite,
						BackgroundColor = MagickColor.FromRgba(69, 69, 69, 200),
						FontWeight = FontWeight.Bold,

						TextGravity = Gravity.Center,
						// This determines the size of the area where the text will be drawn in
						Width = 256,
						Height = 190
					};

					using (var label = new MagickImage($"caption:{welcomeMessage}", readSettings))
					{
						using (var avatar = new MagickImage(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()))
						{
							avatar.AdaptiveResize(128, 128);
							avatar.Border(2);

							image.Composite(avatar, 40, 33, CompositeOperator.Over);

							image.Composite(label, 251, 5, CompositeOperator.Over);
							await channel.SendFileAsync(new MemoryStream(image.ToByteArray()), "Hello from Neira.jpg", $"Страж {user.Mention} приземлился, а это значит что:");
						}
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
			}

		}
		private async Task UserLeft(SocketGuildUser arg)
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
				embed.WithTitle("💢 Страж покинул сервер");
				embed.WithThumbnailUrl($"{arg.GetAvatarUrl() ?? arg.GetDefaultAvatarUrl()}");
				embed.AddField(Global.InvisibleString,
					$"На корабле был известен как:\n**{arg.Nickname ?? arg.Username}**\n" +
					$"Discord имя стража\n**{arg.Username}#{arg.Discriminator}**");
				embed.AddField("Ссылка на профиль(Не всегда корректно отображает)", arg.Mention);
				if (audit[0].Action == ActionType.Kick)
				{
					var test = audit[0].Data as KickAuditLogData;
					if (test.Target.Id == arg.Id)
					{
						embed.WithTitle("🦶 Страж был выгнан");
						var name = audit[0].User.Username ?? "Неизвестно";
						embed.AddField("Причина изгнания:",
							 $"{audit[0].Reason ?? "Не указана."}\n\n" +
							 $"Кто выгнал: {name}");
					}
				}
				if (audit[0].Action == ActionType.Ban)
				{
					var test = audit[0].Data as BanAuditLogData;
					if (test.Target.Id == arg.Id)
					{
						embed.WithTitle("🔨 Страж был забанен");
						var name = audit[0].User.Username ?? "Неизвестно";
						embed.AddField("Причина бана:",
							 $"{audit[0].Reason ?? "Не указана."}\n\n" +
							 $"Кто забанил: {name}");
					}
				}
				embed.WithFooter($"Если ссылка на профиль некорректно отображается то просто скопируй <@{arg.Id}> вместе с <> и отправь в любой чат сообщением.");
				#endregion

				var guild = (await FailsafeDbOperations.GetGuildAccountAsync(arg.Guild.Id));
				if (guild.LoggingChannel != 0)
				{
					await Client.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "UserLeft", ex.Message, ex));
			}
		}
		#endregion

	}
}
