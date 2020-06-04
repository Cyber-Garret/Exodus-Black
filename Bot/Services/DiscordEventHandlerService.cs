using Bot.Core.Data;
using Bot.Properties;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using ImageMagick;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class DiscordEventHandlerService
	{
		// declare the fields used later in this class
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly MilestoneService milestoneHandler;
		private readonly EmoteService emote;
		private readonly SelfRoleService roleService;
		public DiscordEventHandlerService(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<DiscordEventHandlerService>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
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
				var loadedGuild = GuildData.GetGuildAccount(channel.GuildId);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

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

				if (loadedGuild.LoggingChannel != 0)
				{
					await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("ChannelCreated: {0}", ex.Message);
			}

		}
		private async Task ChannelDestroyed(IChannel arg)
		{
			if (!(arg is ITextChannel channel)) return;
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(channel.GuildId);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

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

				if (loadedGuild.LoggingChannel != 0)
				{
					await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("ChannelDestroyed: {0}", ex.Message);
			}
		}
		private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
		{
			if (after == null || before == after || before.IsBot) return;
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(before.Guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;


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

					if (loadedGuild.LoggingChannel != 0)
					{
						await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
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


					if (loadedGuild.LoggingChannel != 0)
					{
						await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}

			}
			catch (Exception ex)
			{
				logger.LogWarning("GuildMemberUpdated: {0}", ex.Message);
			}

		}
		private async Task MessageUpdated(Cacheable<IMessage, ulong> msgBefore, SocketMessage msgAfter, ISocketMessageChannel channel)
		{
			if (msgAfter.Author.IsBot) return;

			try
			{
				if (channel is IGuildChannel currentIGuildChannel)
				{
					var loadedGuild = GuildData.GetGuildAccount(currentIGuildChannel.GuildId);
					Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;


					var after = msgAfter as IUserMessage;

					if (msgAfter.Content == null) return;

					if (!((msgBefore.HasValue ? msgBefore.Value : null) is IUserMessage before)) return;

					if (before.Content == after?.Content) return;


					var embed = new EmbedBuilder
					{
						Title = Resources.MsgUpdEmbTitle,
						Color = Color.Gold,
						Description = string.Format(Resources.MsgUpdEmbDesc, before.Channel.Id),
						Footer = new EmbedFooterBuilder
						{
							IconUrl = msgBefore.Value.Author.GetAvatarUrl() ?? msgBefore.Value.Author.GetDefaultAvatarUrl(),
							Text = string.Format(Resources.DiEvnEmbFooter, after?.Author)
						},
					};
					//old text
					if (msgBefore.Value.Content.Length > 1000)
					{
						var textBefore = msgBefore.Value.Content.Substring(0, 1000);

						embed.AddField(Resources.MsgUpdEmbOldFieldTitle, $"{textBefore}...");
					}
					else if (msgBefore.Value.Content.Length != 0)
					{
						embed.AddField(Resources.MsgUpdEmbOldFieldTitle, msgBefore.Value.Content);
					}

					//new text
					if (msgAfter.Content.Length > 1000)
					{
						var textAfter = msgAfter.Content.Substring(0, 1000);

						embed.AddField(Resources.MsgUpdEmbNewFieldTitle, $"{textAfter}...");

					}
					else if (msgAfter.Content.Length != 0)
					{
						embed.AddField(Resources.MsgUpdEmbNewFieldTitle, msgAfter.Content);
					}


					if (loadedGuild.LoggingChannel != 0)
					{

						await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
							.SendMessageAsync(null, false, embed.Build());
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("MessageUpdated: {0}", ex.Message);
			}

		}
		private async Task MessageDeleted(Cacheable<IMessage, ulong> msg)
		{
			if (msg.Value.Author.IsBot) return;
			try
			{
				if (msg.Value.Channel is ITextChannel textChannel)
				{
					var loadedGuild = GuildData.GetGuildAccount(textChannel.Guild);
					Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

					var log = await textChannel.Guild.GetAuditLogsAsync(1);
					var audit = log.ToList();

					var name = msg.Value.Author;
					var check = audit[0].Data as MessageDeleteAuditLogData;

					//if message deleted by bot finish Task.
					if (audit[0].User.IsBot) return;

					if (check?.ChannelId == msg.Value.Channel.Id && audit[0].Action == ActionType.MessageDeleted)
						name = audit[0].User;

					var embedDel = new EmbedBuilder
					{
						Title = Resources.MsgDelEmbTitle,
						Color = Color.Red,
						Description = string.Format(Resources.MsgDelEmbDesc, msg.Value.Channel.Id, msg.Value.Author),
						Footer = new EmbedFooterBuilder
						{//TODO FIX corect name and avatar if in log last action delete message
							IconUrl = name.GetAvatarUrl() ?? name.GetDefaultAvatarUrl(),
							Text = string.Format(Resources.DiEvnEmbFooter, name)
						}
					};

					//deleted text message
					if (msg.Value.Content.Length > 1000)
					{
						var string1 = msg.Value.Content.Substring(0, 1000);

						embedDel.AddField(Resources.MsgDelEmbFieldTitle, $"{string1}...");
					}
					else if (msg.Value.Content.Length != 0)
					{
						embedDel.AddField(Resources.MsgDelEmbFieldTitle, $"{msg.Value.Content}");
					}

					if (loadedGuild.LoggingChannel != 0)
					{

						await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
							.SendMessageAsync(null, false, embedDel.Build());
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("MessageDeleted: {0}", ex.Message);
			}

		}
		private async Task RoleDeleted(SocketRole role)
		{
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(role.Guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				var log = await role.Guild.GetAuditLogsAsync(1).FlattenAsync();
				var audit = log.ToList();
				var check = audit[0].Data as RoleDeleteAuditLogData;
				var name = Resources.Unknown;

				if (check?.RoleId == role.Id)
					name = audit[0].User.Username;

				var embed = new EmbedBuilder
				{
					Title = Resources.RolDelEmbTitle,
					Color = Color.Red,
					Description = string.Format(Resources.RolDelEmbDesc, role.Name, role.Color),
					Footer = new EmbedFooterBuilder
					{
						IconUrl = audit[0].User.GetAvatarUrl() ?? audit[0].User.GetDefaultAvatarUrl(),
						Text = string.Format(Resources.DiEvnEmbFooter, name)
					}
				};

				if (loadedGuild.LoggingChannel != 0)
				{
					await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("RoleDeleted: {0}", ex.Message);
			}

		}
		private async Task UserJoined(SocketGuildUser user)
		{
			try
			{
				if (user == null || user.IsBot) return;

				var loadedGuild = GuildData.GetGuildAccount(user.Guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				if (string.IsNullOrWhiteSpace(loadedGuild.WelcomeMessage)) return;

				var dM = await user.GetOrCreateDMChannelAsync();
				var embed = WelcomeEmbed(user, loadedGuild.WelcomeMessage);

				await dM.SendMessageAsync(embed: embed);
			}
			catch (Exception ex)
			{
				logger.LogWarning("UserJoined: {0}", ex.Message);
			}

		}
		private async Task UserWelcome(SocketGuildUser user)
		{
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(user.Guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				if (loadedGuild.WelcomeChannel == 0) return;
				if (!(discord.GetChannel(loadedGuild.WelcomeChannel) is SocketTextChannel channel)) return;
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
				string background = Path.Combine(Directory.GetCurrentDirectory(), "UserData", "welcome-bg", $"bg{rand.Next(1, 31)}.jpg");

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
				await channel.SendFileAsync(new MemoryStream(image.ToByteArray()), "Hello from Neira.jpg", string.Format(Resources.Hellotxt, user.Mention));
			}
			catch (Exception ex)
			{
				logger.LogWarning("UserWelcome: {0}", ex.Message);
			}

		}
		private async Task UserLeft(SocketGuildUser user)
		{
			if (user == null || user.IsBot) return;
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(user.Guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				var log = await user.Guild.GetAuditLogsAsync(1).FlattenAsync();
				var audit = log.ToList();
				var embed = new EmbedBuilder
				{
					Title = Resources.UsrLefEmbTitle,
					Color = Color.Red,
					ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
					Description = string.Format(Resources.UsrLefEmbDesc, user.Nickname ?? user.Username, user),

				};
				if (audit[0].Action == ActionType.Kick)
				{
					var kick = audit[0].Data as KickAuditLogData;
					if (kick.Target.Id == user.Id)
					{
						var who = audit[0].User;

						embed.Title = Resources.UsrKicEmbTitle;
						embed.AddField(Resources.UsrLefEmbFieldTitle, audit[0].Reason ?? Resources.Unknown);
						embed.Footer = new EmbedFooterBuilder
						{
							IconUrl = who.GetAvatarUrl() ?? who.GetDefaultAvatarUrl(),
							Text = string.Format(Resources.DiEvnEmbFooter, who.Username)
						};
					}
				}
				else if (audit[0].Action == ActionType.Ban)
				{
					var ban = audit[0].Data as BanAuditLogData;
					if (ban.Target.Id == user.Id)
					{
						var who = audit[0].User;

						embed.Title = Resources.UsrBanEmbTitle;
						embed.AddField(Resources.UsrLefEmbFieldTitle, audit[0].Reason ?? Resources.Unknown);
						embed.Footer = new EmbedFooterBuilder
						{
							IconUrl = who.GetAvatarUrl() ?? who.GetDefaultAvatarUrl(),
							Text = string.Format(Resources.DiEvnEmbFooter, who.Username)
						};
					}
				}

				if (loadedGuild.LoggingChannel != 0)
				{
					await discord.GetGuild(loadedGuild.Id).GetTextChannel(loadedGuild.LoggingChannel)
						.SendMessageAsync(null, false, embed.Build());
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("UserLeft: {0}", ex.Message);
			}
		}

		public Embed WelcomeEmbed(SocketGuildUser user, string text)
		{
			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.WlcmEmbTitle, user.Guild.Name),
				Color = Color.Orange,
				ThumbnailUrl = user.Guild.IconUrl,
				Description = text
			};
			return embed.Build();
		}
		#endregion
	}
}
