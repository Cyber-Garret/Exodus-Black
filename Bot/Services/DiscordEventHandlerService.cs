using Bot.Properties;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using System.Linq;
using System.IO;
using ImageMagick;
using System.Net;
using Bot.Core.Data;

namespace Bot.Services
{
	public class DiscordEventHandlerService
	{
		// declare the fields used later in this class
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly CommandHandlerService commandHandler;
		private readonly MilestoneService milestoneHandler;
		private readonly EmoteService emote;
		private readonly SelfRoleService roleService;
		public DiscordEventHandlerService(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<DiscordEventHandlerService>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			commandHandler = service.GetRequiredService<CommandHandlerService>();
			milestoneHandler = service.GetRequiredService<MilestoneService>();
			emote = service.GetRequiredService<EmoteService>();
			roleService = service.GetRequiredService<SelfRoleService>();
		}

		public void Configure()
		{
			discord.JoinedGuild += Discord_JoinedGuild;
			discord.LeftGuild += Discord_LeftGuild;

			discord.ChannelCreated += Discord_ChannelCreated;
			discord.ChannelDestroyed += Discord_ChannelDestroyed;

			discord.GuildMemberUpdated += Discord_GuildMemberUpdated;

			discord.MessageUpdated += Discord_MessageUpdated;
			discord.MessageDeleted += Discord_MessageDeleted;

			discord.RoleCreated += Discord_RoleCreated;
			discord.RoleDeleted += Discord_RoleDeleted;

			discord.UserJoined += Discord_UserJoined;
			discord.UserLeft += Discord_UserLeft;

			discord.ReactionAdded += Discord_ReactionAdded;
			discord.ReactionRemoved += Discord_ReactionRemoved;
		}

		#region Events
		private Task Discord_JoinedGuild(SocketGuild guild)
		{
			Task.Run(() =>
			{
				GuildData.GetGuildAccount(guild);
			});
			return Task.CompletedTask;
		}

		private Task Discord_LeftGuild(SocketGuild arg)
		{
			Task.Run(() =>
			{//TODO: Remove guild(or mark is deleted) and send message for guild owner
			 //await DatabaseHelper.RemoveGuildAccountAsync(guild.Id);
			});
			return Task.CompletedTask;
		}

		private Task Discord_ChannelCreated(SocketChannel arg)
		{
			Task.Run(async () =>
			{
				await ChannelCreated(arg);
			});
			return Task.CompletedTask;
		}

		private Task Discord_ChannelDestroyed(SocketChannel arg)
		{
			Task.Run(async () =>
			{
				await ChannelDestroyed(arg);
			});
			return Task.CompletedTask;
		}

		private Task Discord_GuildMemberUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
		{
			Task.Run(async () =>
			{
				await GuildMemberUpdated(userBefore, userAfter);
			});
			return Task.CompletedTask;
		}

		private Task Discord_MessageUpdated(Cacheable<IMessage, ulong> cacheMessageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
		{
			if (!cacheMessageBefore.HasValue || cacheMessageBefore.Value.Author.IsBot)
				return Task.CompletedTask;

			Task.Run(async () =>
			{
				await MessageUpdated(cacheMessageBefore, messageAfter, channel);
			});
			return Task.CompletedTask;
		}

		private Task Discord_MessageDeleted(Cacheable<IMessage, ulong> cacheMessage, ISocketMessageChannel channel)
		{
			if (!cacheMessage.HasValue || cacheMessage.Value.Author.IsBot)
				return Task.CompletedTask;

			Task.Run(async () =>
			{
				await MessageDeleted(cacheMessage);
			});
			return Task.CompletedTask;
		}

		private Task Discord_RoleCreated(SocketRole role)
		{
			Task.Run(async () =>
			{
				await RoleCreated(role);
			});
			return Task.CompletedTask;
		}

		private Task Discord_RoleDeleted(SocketRole role)
		{
			Task.Run(async () =>
			{
				await RoleDeleted(role);
			});
			return Task.CompletedTask;
		}

		private Task Discord_UserJoined(SocketGuildUser guildUser)
		{
			Task.Run(async () =>
			{
				//Message in Guild
				await UserJoined(guildUser);
				await UserWelcome(guildUser);
				//AutoRole

				var guild = GuildData.GetGuildAccount(guildUser.Guild);
				if (guild.AutoroleID != 0)
				{
					var targetRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == guild.AutoroleID);
					if (targetRole != null)
						await guildUser.AddRoleAsync(targetRole);
				}
			});
			return Task.CompletedTask;
		}

		private Task Discord_UserLeft(SocketGuildUser guildUser)
		{
			Task.Run(async () =>
			{
				await UserLeft(guildUser);
			});
			return Task.CompletedTask;
		}

		private Task Discord_ReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
		{
			Task.Run(async () =>
			{
				if (!reaction.User.Value.IsBot)
				{
					//New milestone?
					if (reaction.Emote.Equals(emote.Raid))
						await milestoneHandler.MilestoneReactionAdded(cacheable, reaction);
					// self role message?
					await roleService.SelfRoleReactionAdded(cacheable, reaction);

				}
			});
			return Task.CompletedTask;
		}

		private Task Discord_ReactionRemoved(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
		{
			Task.Run(async () =>
			{
				if (!reaction.User.Value.IsBot)
				{
					//New milestone?
					if (reaction.Emote.Equals(emote.Raid))
						await milestoneHandler.MilestoneReactionRemoved(cacheable, reaction);

					await roleService.SelfRoleReactionRemoved(cacheable, reaction);
				}
			});
			return Task.CompletedTask;
		}
		#endregion

		#region Methods
		private async Task ChannelCreated(IChannel arg)
		{
			if (!(arg is ITextChannel channel)) return;
			try
			{
				var log = await channel.Guild.GetAuditLogsAsync(1);
				var audit = log.ToList();
				var name = audit[0].Action == ActionType.ChannelCreated ? audit[0].User.Username : Resources.Unknown;
				var auditLogData = audit[0].Data as ChannelCreateAuditLogData;

				var embed = new EmbedBuilder
				{
					Color = Color.Orange,
					Footer = new EmbedFooterBuilder
					{
						IconUrl = audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl(),
						Text = string.Format(Resources.DiEvnEmbFooter, name)
					},
				};
				embed.AddField(Resources.ChanCreEmbFieldTitle, string.Format(Resources.ChanEmbFieldDesc,
					arg.Name,
					channel.IsNsfw,
					channel.GetCategoryAsync().Result.Name));

				var guildChannel = (IGuildChannel)arg;

				var guild = GuildData.GetGuildAccount(guildChannel.Guild);
				if (guild.LoggingChannel != 0)
				{
					await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "ChannelCreated");
			}

		}
		private async Task ChannelDestroyed(IChannel arg)
		{
			if (!(arg is ITextChannel channel)) return;
			try
			{
				var log = await channel.Guild.GetAuditLogsAsync(1);
				var audit = log.ToList();
				var name = audit[0].Action == ActionType.ChannelDeleted ? audit[0].User.Username : Resources.Unknown;
				var auditLogData = audit[0].Data as ChannelDeleteAuditLogData;

				var embed = new EmbedBuilder
				{
					Color = Color.Red,
					Footer = new EmbedFooterBuilder
					{
						IconUrl = audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl(),
						Text = string.Format(Resources.DiEvnEmbFooter, name)
					}
				};
				embed.AddField(Resources.ChanDelEmbFieldTitle, string.Format(
					Resources.ChanEmbFieldDesc,
					arg.Name,
					channel.IsNsfw,
					channel.GetCategoryAsync().Result.Name));

				var guildChannel = (IGuildChannel)arg;

				var guild = GuildData.GetGuildAccount(guildChannel.Guild);
				if (guild.LoggingChannel != 0)
				{
					await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "ChannelDestroyed");
			}
		}
		private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
		{
			if (after == null || before == after || before.IsBot) return;
			try
			{
				var guild = GuildData.GetGuildAccount(before.Guild);


				if (before.Nickname != after.Nickname)
				{
					var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
					var audit = log.ToList();
					var beforeName = before.Nickname ?? before.Username;
					var afterName = after.Nickname ?? after.Username;
					var embed = new EmbedBuilder
					{
						Color = Color.Gold,
						ThumbnailUrl = after.GetAvatarUrl() ?? after.GetDefaultAvatarUrl()
					};
					embed.AddField(Resources.GuMemUpdEmbFieldTitle, string.Format(Resources.GuMemUpdEmbFieldDesc, beforeName, afterName));

					if (audit[0].Action == ActionType.MemberUpdated)
					{
						var name = audit[0].User.Username ?? Resources.Unknown;
						embed.WithFooter(string.Format(Resources.DiEvnEmbFooter, name), audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
					}

					if (guild.LoggingChannel != 0)
					{
						await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}

				if (before.Roles.Count != after.Roles.Count)
				{
					string roleString;
					var beforeRoles = before.Roles.ToList();
					var afterRoles = after.Roles.ToList();
					var role = string.Empty;

					var embed = new EmbedBuilder
					{
						ThumbnailUrl = after.GetAvatarUrl() ?? after.GetDefaultAvatarUrl()
					};
					if (before.Roles.Count > after.Roles.Count)
					{
						embed.WithColor(Color.Red);
						roleString = Resources.GuMemRemRole;
						var differenceQuery = beforeRoles.Except(afterRoles);
						var socketRoles = differenceQuery as SocketRole[] ?? differenceQuery.ToArray();
						for (var i = 0; i < socketRoles.Count(); i++)
							role += socketRoles[i];
					}
					else
					{
						embed.WithColor(Color.Green);
						roleString = Resources.GuMemAddRole;
						var differenceQuery = afterRoles.Except(beforeRoles);
						var socketRoles = differenceQuery as SocketRole[] ?? differenceQuery.ToArray();
						for (var i = 0; i < socketRoles.Count(); i++)
							role += socketRoles[i];
					}

					var log = await before.Guild.GetAuditLogsAsync(1).FlattenAsync();
					var audit = log.ToList();

					embed.AddField(Resources.GuMemRolEmbFieldTitle, string.Format(Resources.GuMemRolEmbFieldDesc, before.Nickname ?? before.Username, roleString, role));
					if (audit[0].Action == ActionType.MemberRoleUpdated)
					{
						var name = audit[0].User.Username ?? Resources.Unknown;
						embed.WithFooter(string.Format(Resources.DiEvnEmbFooter, name), audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
					}


					if (guild.LoggingChannel != 0)
					{
						await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}

			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "GuildMemberUpdated");
			}

		}
		private async Task MessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
		{
			if (messageAfter.Author.IsBot) return;

			try
			{
				if (channel is IGuildChannel currentIGuildChannel)
				{
					var guild = GuildData.GetGuildAccount(currentIGuildChannel.Guild);


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

							var string2 = messageBefore.Value.Content[1000..];

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
								messageAfter.Content[1000..];
							embed.AddField("Новый текст: Далее", $"...{string2}");

						}
					}
					else if (messageAfter.Content.Length != 0)
					{
						embed.AddField("Новый текст:", $"{messageAfter.Content}");
					}


					if (guild.LoggingChannel != 0)
					{

						await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "MessageUpdated");
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
					var guild = GuildData.GetGuildAccount(textChannel.Guild);

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
								messageBefore.Value.Content[1000..];
							embedDel.AddField("Далее", $"...{string2}");

						}
					}
					else if (messageBefore.Value.Content.Length != 0)
					{
						embedDel.AddField("Текст сообщения", $"{messageBefore.Value.Content}");
					}

					if (guild.LoggingChannel != 0)
					{

						await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
							.SendMessageAsync(null, false, embedDel.Build());
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "MessageDeleted");
			}

		}
		private async Task RoleCreated(SocketRole role)
		{
			try
			{
				#region Data
				var log = await role.Guild.GetAuditLogsAsync(1).FlattenAsync();
				var audit = log.ToList();
				var check = audit[0].Data as RoleCreateAuditLogData;
				var name = "Неизвестно";
				var embed = new EmbedBuilder();
				if (check?.RoleId == role.Id)
					name = audit[0].User.Username;
				#endregion

				#region Message
				embed.WithColor(Color.Orange);
				embed.WithTimestamp(DateTimeOffset.UtcNow);
				embed.AddField("🔑 Создана роль", $"Название: **{role.Name}**");
				embed.WithFooter($"Кто создавал: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
				#endregion

				var guild = GuildData.GetGuildAccount(role.Guild);

				if (guild.LoggingChannel != 0)
				{
					await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "RoleCreated");
			}

		}
		private async Task RoleDeleted(SocketRole role)
		{
			try
			{
				#region Data
				var log = await role.Guild.GetAuditLogsAsync(1).FlattenAsync();
				var audit = log.ToList();
				var check = audit[0].Data as RoleDeleteAuditLogData;
				var name = "Неизвестно";
				var embed = new EmbedBuilder();
				if (check?.RoleId == role.Id)
					name = audit[0].User.Username;
				#endregion

				#region Message
				embed.WithColor(Color.Red);
				embed.WithTimestamp(DateTimeOffset.UtcNow);
				embed.AddField("❌ Удалена роль",
					$"Название: **{role.Name}**\n" +
					$"Цвет: **{role.Color}**");
				embed.WithFooter($"Кто удалял: {name}", audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl());
				#endregion

				var guild = GuildData.GetGuildAccount(role.Guild);

				if (guild.LoggingChannel != 0)
				{
					await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "RoleDeleted");
			}

		}
		private async Task UserJoined(SocketGuildUser user)
		{
			try
			{
				#region Checks
				if (user == null || user.IsBot) return;

				var guild = GuildData.GetGuildAccount(user.Guild);
				if (string.IsNullOrWhiteSpace(guild.WelcomeMessage)) return;
				#endregion

				IDMChannel dM = await user.GetOrCreateDMChannelAsync();
				//TODO: Welcome embed
				//await dM.SendMessageAsync(null, false, MiscHelpers.WelcomeEmbed(user, guild.WelcomeMessage).Build());
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "UserJoined");
			}

		}
		private async Task UserWelcome(SocketGuildUser user)
		{
			try
			{
				var guild = GuildData.GetGuildAccount(user.Guild.Id);
				if (guild.WelcomeChannel == 0) return;
				if (!(discord.GetChannel(guild.WelcomeChannel) is SocketTextChannel channel)) return;
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

				var rand = new Random();

				string welcomeMessage = randomWelcome[rand.Next(randomWelcome.Length)];
				string background = Path.Combine(Directory.GetCurrentDirectory(), "UserData", "WelcomeBg", $"bg{rand.Next(1, 31)}.jpg");

				using var image = new MagickImage(background, 512, 200);
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

				using var label = new MagickImage($"caption:{welcomeMessage}", readSettings);
				//Load user avatar
				using var client = new WebClient();
				var file = user.GetAvatarUrl(ImageFormat.Png, 128) ?? user.GetDefaultAvatarUrl();
				using var stream = client.OpenRead(file);
				using var avatar = new MagickImage(stream, MagickFormat.Png8);
				avatar.AdaptiveResize(128, 128);
				avatar.Border(2);

				image.Composite(avatar, 40, 33, CompositeOperator.Over);

				image.Composite(label, 251, 5, CompositeOperator.Over);
				await channel.SendFileAsync(new MemoryStream(image.ToByteArray()), "Hello from Neira.jpg", $"Страж {user.Mention} приземлился, а это значит что:");
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "UserWelcome");
			}

		}
		private async Task UserLeft(SocketGuildUser user)
		{
			try
			{
				#region Checks
				if (user == null || user.IsBot)
					return;
				#endregion

				#region Data
				var log = await user.Guild.GetAuditLogsAsync(1).FlattenAsync();
				var audit = log.ToList();
				var embed = new EmbedBuilder();
				#endregion

				#region Message
				embed.WithColor(Color.Red);
				embed.WithTimestamp(DateTimeOffset.UtcNow);
				embed.WithTitle("💢 Страж покинул сервер");
				embed.WithThumbnailUrl($"{user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()}");
				embed.WithDescription($"На корабле был известен как:\n**{user.Nickname ?? user.Username}**\n" +
					$"Discord имя стража\n**{user.Username}#{user.Discriminator}**");
				embed.AddField("Ссылка на профиль(Не всегда корректно отображает)", user.Mention);
				if (audit[0].Action == ActionType.Kick)
				{
					var test = audit[0].Data as KickAuditLogData;
					if (test.Target.Id == user.Id)
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
					if (test.Target.Id == user.Id)
					{
						embed.WithTitle("🔨 Страж был забанен");
						var name = audit[0].User.Username ?? "Неизвестно";
						embed.AddField("Причина бана:",
							 $"{audit[0].Reason ?? "Не указана."}\n\n" +
							 $"Кто забанил: {name}");
					}
				}
				embed.WithFooter($"Если ссылка на профиль некорректно отображается то просто скопируй <@{user.Id}> вместе с <> и отправь в любой чат сообщением.");
				#endregion

				var guild = GuildData.GetGuildAccount(user.Guild);
				if (guild.LoggingChannel != 0)
				{
					await discord.GetGuild(guild.Id).GetTextChannel(guild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "UserLeft");
			}
		}
		#endregion
	}
}
