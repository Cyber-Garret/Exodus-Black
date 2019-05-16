using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Core;
using DiscordBot.Helpers;

namespace DiscordBot.Services
{
	public class DiscordEventHandlerService
	{
		#region Private Fields
		private readonly DiscordShardedClient _client = Program._client;
		private readonly CommandHandlerService _commandHandlingService;
		#endregion


		public DiscordEventHandlerService(CommandHandlerService command)
		{
			_commandHandlingService = command;
		}

		public void Configure()
		{
			//_client.ShardConnected += _client_ShardConnected;
			_client.ShardDisconnected += _client_ShardDisconnectedAsync;
			_client.JoinedGuild += _client_JoinedGuildAsync;
			_client.ChannelCreated += _client_ChannelCreatedAsync;
			_client.ChannelDestroyed += _client_ChannelDestroyedAsync;
			_client.GuildMemberUpdated += _client_GuildMemberUpdatedAsync;
			_client.MessageDeleted += _client_MessageDeletedAsync;
			_client.MessageReceived += _client_MessageReceived;
			_client.MessageUpdated += _client_MessageUpdatedAsync;
			_client.RoleCreated += _client_RoleCreatedAsync;
			_client.RoleDeleted += _client_RoleDeletedAsync;
			_client.UserJoined += _client_UserJoinedAsync;
			_client.UserLeft += _client_UserLeftAsync;
		}
		#region Events
		private async Task _client_ShardDisconnectedAsync(Exception ex, DiscordSocketClient client)
		{
			await Logger.Log(new LogMessage(LogSeverity.Warning, $"Shard {client.ShardId} Disconnected", ex.Message, ex.InnerException));
		}
		private async Task _client_JoinedGuildAsync(SocketGuild guild) => _ = await FailsafeDbOperations.GetGuildAccountAsync(guild.Id);
		private async Task _client_ChannelCreatedAsync(SocketChannel arg) => await ChannelCreated(arg);
		private async Task _client_ChannelDestroyedAsync(SocketChannel arg) => await ChannelDestroyed(arg);
		private async Task _client_GuildMemberUpdatedAsync(SocketGuildUser userBefore, SocketGuildUser userAfter) => await GuildMemberUpdated(userBefore, userAfter);
		private async Task _client_MessageReceived(SocketMessage message)
		{
			if (message.Author.IsBot)
				return;
			await _commandHandlingService.HandleCommandAsync(message);
			await MessageReceived(message);
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
		private async Task _client_UserJoinedAsync(SocketGuildUser arg) => await UserJoined(arg);
		private async Task _client_UserLeftAsync(SocketGuildUser arg) => await UserLeft(arg);
		#endregion

		#region Methods
		public async Task ChannelCreated(IChannel arg)
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
				if (guild.EnableLogging == true)
				{
					await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"ChannelCreated Method - {ex.Source}", ex.Message, ex.InnerException));
			}

		}
		public async Task ChannelDestroyed(IChannel arg)
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
					if (guild.EnableLogging == true)
					{
						await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"ChannelDestroyed Method - {ex.Source}", ex.Message, ex.InnerException));
			}
		}
		public async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
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

					if (guild.EnableLogging == true)
					{
						await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
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

					if (guild.EnableLogging == true)
					{
						await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}
				#endregion

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"GuildMemberUpdated Method - {ex.Source}", ex.Message, ex.InnerException));
			}

		}
		private async Task MessageReceived(SocketMessage arg)
		{
			if (arg.Author.Id == _client.CurrentUser.Id)
				return;

			await Task.CompletedTask;
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


					if (guild.EnableLogging == true)
					{

						await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"MessageUpdated Method - {ex.Source}", ex.Message, ex.InnerException));
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

						await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embedDel.Build());
					}

				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"MessageDeleted Method - {ex.Source}", ex.Message, ex.InnerException));
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

				if (guild.EnableLogging == true)
				{
					await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"RoleCreated Method - {ex.Source}", ex.Message, ex.InnerException));
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

				if (guild.EnableLogging == true)
				{
					await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"RoleDeleted Method - {ex.Source}", ex.Message, ex.InnerException));
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
				await Logger.Log(new LogMessage(LogSeverity.Error, $"UserJoined Method - {ex.Source}", ex.Message, ex.InnerException));
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
				embed.WithThumbnailUrl($"{arg.GetAvatarUrl()}");
				embed.AddField($"💢 Страж покинул клан",
					$"На корабле был известен как:\n**{arg.Nickname ?? arg.Username}**\n" +
					$"Discord имя стража\n**{arg.Username}#{arg.Discriminator}**");
				embed.AddField("Ссылка на профиль(Не всегда корректно отображает)", arg.Mention);
				if (audit[0].Action == ActionType.Kick)
				{
					var name = audit[0].User.Username ?? "Неизвестно";
					embed.AddField("Причина изгнания:",
						 $"{audit[0].Reason ?? "Не указана."}\n\n" +
						 $"Кто выгнал: {name}");
				}
				embed.WithFooter($"Если ссылка на профиль некорректно отображается то просто скопируй <@{arg.Id}> вместе с <> и отправь в любой чат сообщением.");
				#endregion

				var guild = FailsafeDbOperations.GetGuildAccountAsync(arg.Guild.Id).Result;
				if (guild.EnableLogging == true)
				{
					await _client.GetGuild(guild.ID).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"UserLeft Method - {ex.Source}", ex.Message, ex.InnerException));
			}
		}
		#endregion

	}
}
