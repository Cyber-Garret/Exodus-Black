using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Models;
using Failsafe.Models.Enums;
using Failsafe.Properties;

using Microsoft.Extensions.Logging;

namespace Failsafe.Services
{
	public class MilestoneService
	{
		private readonly ILogger<MilestoneService> _logger;
		private readonly DiscordRestClient _discordRest;
		private readonly EmoteService _emote;

		public MilestoneService(ILogger<MilestoneService> logger, EmoteService emote, DiscordRestClient discordRest)
		{
			_logger = logger;
			_emote = emote;
			_discordRest = discordRest;
		}

		public async Task MilestoneReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();
				//get milestone
				var milestone = ActiveMilestoneData.GetMilestone(msg.Id);

				if (milestone == null) return;

				if (reaction.Emote.Equals(_emote.Raid))
				{
					//check reaction
					var userExist = milestone.MilestoneUsers.Any(u => u == reaction.UserId);

					if (reaction.UserId != milestone.Leader && !userExist && milestone.MilestoneUsers.Count + 1 < milestone.MilestoneInfo.MaxSpace)
					{
						milestone.MilestoneUsers.Add(reaction.UserId);
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);

						HandleReaction(msg, milestone);
					}
					else
					{
						var user = await _discordRest.GetUserAsync(reaction.UserId);
						//var user = _discord.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(_emote.Raid, user);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Reaction Added in Milestone");
			}
		}

		public async Task MilestoneReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();
				//get milestone
				var milestone = ActiveMilestoneData.GetMilestone(msg.Id);

				if (milestone == null) return;

				if (reaction.Emote.Equals(_emote.Raid))
				{
					//check reaction
					var userExist = milestone.MilestoneUsers.Any(u => u == reaction.UserId);

					if (reaction.UserId != milestone.Leader && userExist)
					{
						milestone.MilestoneUsers.Remove(reaction.UserId);
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);
						HandleReaction(msg, milestone);
					}
					else
					{
						var user = await _discordRest.GetUserAsync(reaction.UserId);
						//var user = _discord.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(_emote.Raid, user);
					}
				}

			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Reaction Removed in Milestone");
			}
		}

		private async void HandleReaction(IUserMessage message, Milestone milestone)
		{
			try
			{
				var newEmbed = await MilestoneEmbed(milestone);
				if (newEmbed.Length != 0)
					await message.ModifyAsync(m => m.Embed = newEmbed);
			}
			catch (Exception e)
			{
				_logger.LogError(e, nameof(HandleReaction));
			}
		}

		public async Task MilestoneNotificationAsync(Milestone milestone)
		{
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(milestone.GuildId);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				var remindEmbed = await RemindEmbed(milestone);

				var leader = await _discordRest.GetUserAsync(milestone.Leader);
				//var leader = _discord.GetUser(milestone.Leader);
				var leaderDm = await leader.CreateDMChannelAsync();

				await leaderDm.SendMessageAsync(embed: remindEmbed);

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{

						if (user == GlobalVariables.ReservedID) continue;

						var loadedUser = await _discordRest.GetUserAsync(user);
						//var loadedUser = _discord.GetUser(user);

						var dm = await loadedUser.CreateDMChannelAsync();
						await dm.SendMessageAsync(embed: remindEmbed);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "RaidNotification in DM of user");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "RaidNotification Global");
			}

		}

		public async Task TimeChangedNotificationAsync(Milestone milestone)
		{
			try
			{
				var guild = await _discordRest.GetGuildAsync(milestone.GuildId);
				var loadedGuild = GuildData.GetGuildAccount(guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				var channel = await guild.GetTextChannelAsync(milestone.ChannelId);
				var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);

				var remindEmbed = TimeChangedEmbed(guild, milestone, msg.GetJumpUrl());

				var leader = await _discordRest.GetUserAsync(milestone.Leader);
				//var leader = _discord.GetUser(milestone.Leader);
				var leaderDm = await leader.CreateDMChannelAsync();

				await leaderDm.SendMessageAsync(embed: remindEmbed);

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{

						if (user == GlobalVariables.ReservedID) continue;

						var loadedUser = await _discordRest.GetUserAsync(user);
						//var loadedUser = _discord.GetUser(user);

						var dm = await loadedUser.CreateDMChannelAsync();
						await dm.SendMessageAsync(embed: remindEmbed);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "RaidNotification in DM of user");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "RaidNotification Global");
			}

		}

		public async Task<Embed> MilestoneEmbed(Milestone milestone)
		{
			//TODO: Donate text
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(milestone.GuildId);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;
				var activityTitle = new StringBuilder();

				if (milestone.MilestoneInfo.Type != null)
					activityTitle.Append(milestone.MilestoneInfo.Type);
				activityTitle.Append(milestone.MilestoneInfo.Name);

				var embed = new EmbedBuilder
				{
					Title = string.Format(Resources.MilEmbTitle,
						milestone.DateExpire.ToString("dd.MM.yyyy"),
						milestone.DateExpire.ToString("HH:mm(zzz)"),
						BuildActivityName(milestone.MilestoneInfo)),
					Author = GetGame(milestone.MilestoneInfo.Game),
					ThumbnailUrl = milestone.MilestoneInfo.Icon,
					Color = GetColorByType(milestone.MilestoneInfo.MilestoneType)

				};
				if (milestone.Note != null)
					embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

				var leader = await _discordRest.GetUserAsync(milestone.Leader);

				var leaderTitle = milestone.MilestoneInfo.Game == GameName.WoT ? Resources.MilEmbInfDescFieldWoT
					: Resources.MilEmbInfDescFieldMain;

				embed.AddField(Resources.MilEmbInfTitleField, string.Format(leaderTitle, leader.Mention, leader.Username, _emote.Raid));

				if (milestone.MilestoneUsers.Count > 0)
				{
					var embedFieldUsers = new EmbedFieldBuilder
					{
						Name = Resources.MilEmbMemTitleField
					};
					var count = 2;
					foreach (var user in milestone.MilestoneUsers)
					{
						if (user == GlobalVariables.ReservedID)
						{
							embedFieldUsers.Value += $"#{count} {Resources.MilReserved}\n";
						}
						else
						{
							var discordUser = await _discordRest.GetUserAsync(user);
							//var discordUser = _discord.GetUser(user);
							embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
						}
						count++;
					}

					embed.AddField(embedFieldUsers);
				}
				embed.WithFooter($"ID: {milestone.MessageId}");

				return embed.Build();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "MilestoneEmbed");
				return null;
			}

		}

		public static Embed GetMilestonesNameEmbed(SocketGuild guild, MilestoneType type)
		{
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.MilInfEmbTitle, GetNameForMilestoneType(guild, type)),
				Color = GetColorByType(type)
			};
			if (type == MilestoneType.Raid)
			{
				var des2Names = string.Empty;
				var div2Names = string.Empty;
				var lostArkNames = string.Empty;
				var other = string.Empty;
				foreach (var item in MilestoneInfoData.GetMilestonesByType(type))
				{
					switch (item.Game)
					{
						case GameName.Destiny:
							des2Names += $"**{item.Alias}** - {item.Name}\n";
							break;
						case GameName.Division:
							div2Names += $"**{item.Alias}** - {item.Name}\n";
							break;
						case GameName.LostArk:
							lostArkNames += $"**{item.Alias}** - {item.Name}\n";
							break;
						default:
							other += $"**{item.Alias}** - {item.Name}\n";
							break;
					}
				}
				embed.AddField("Destiny 2", des2Names);
				embed.AddField("The Division 2", div2Names);
				embed.AddField("Lost Ark", lostArkNames);

				if (!string.IsNullOrWhiteSpace(other))
					embed.AddField(GlobalVariables.InvisibleString, other);
			}
			else
			{
				var text = MilestoneInfoData.GetMilestonesByType(type)
					.Aggregate(string.Empty,
						(current,
							milestone) => current + $"**{milestone.Alias}** - {milestone.Name}\n");
				embed.Description = text;
			}
			embed.WithFooter(Resources.EmbFooterAboutDel);

			return embed.Build();
		}

		private static string BuildActivityName(MilestoneInfo milestone)
		{
			if (string.IsNullOrEmpty(milestone.Type))
				return milestone.Name;

			var activityName = new[] { milestone.Type, milestone.Name };

			return string.Join(' ', activityName);
		}

		private async Task<Embed> RemindEmbed(Milestone milestone)
		{
			var guild = await _discordRest.GetGuildAsync(milestone.GuildId);
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;


			var embed = new EmbedBuilder()
			{
				Title = string.Format(Resources.MilRemEmbTitle, BuildActivityName(milestone.MilestoneInfo)),
				Author = GetGame(milestone.MilestoneInfo.Game),
				Color = GetColorByType(milestone.MilestoneInfo.MilestoneType),
				ThumbnailUrl = milestone.MilestoneInfo.Icon
			};
			if (milestone.Note != null)
				embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = Resources.MilEmbMemTitleField
			};
			var leader = await _discordRest.GetUserAsync(milestone.Leader);
			//var leader = _discord.GetUser(milestone.Leader);
			embedFieldUsers.Value = $"#1 {leader.Mention} - {leader.Username}\n";
			var count = 2;
			foreach (var user in milestone.MilestoneUsers.Where(user => user != GlobalVariables.ReservedID))
			{
				var discordUser = await _discordRest.GetUserAsync(user);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";

				count++;
			}

			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			embed.WithFooter(string.Format(Resources.MilRemEmbFooter, BuildActivityName(milestone.MilestoneInfo), guild.Name), guild.IconUrl);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}

		private static Embed TimeChangedEmbed(IGuild guild, Milestone milestone, string jumpUrl)
		{
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			var embed = new EmbedBuilder
			{
				Title = Resources.MilEmbTitleChangeTime,
				Author = GetGame(milestone.MilestoneInfo.Game),
				Color = GetColorByType(milestone.MilestoneInfo.MilestoneType),
				ThumbnailUrl = milestone.MilestoneInfo.Icon
			};
			if (milestone.Note != null)
				embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

			embed.AddField(Resources.MilEmbTimeFieldTitle, string.Format(Resources.MilEmbTimeFieldDesc, jumpUrl));

			embed.WithFooter(string.Format(Resources.MilRemEmbFooter, milestone.MilestoneInfo.Type, milestone.MilestoneInfo.Name, guild.Name), guild.IconUrl);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}

		private static Color GetColorByType(MilestoneType type)
		{
			return type switch
			{
				MilestoneType.Raid => Color.DarkMagenta,
				MilestoneType.Nightfall => Color.DarkGreen,
				MilestoneType.Other => Color.DarkBlue,
				_ => Color.Magenta,
			};
		}

		private static string GetNameForMilestoneType(IGuild guild, MilestoneType type)
		{
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			return type switch
			{
				MilestoneType.Raid => Resources.Raid,
				MilestoneType.Nightfall => Resources.Nightfall,
				MilestoneType.Other => Resources.Other,
				_ => Resources.Unknown,
			};
		}

		private static EmbedAuthorBuilder GetGame(GameName game)
		{
			var author = new EmbedAuthorBuilder();
			switch (game)
			{
				case GameName.Destiny:
					author.Name = "Destiny 2";
					author.IconUrl = @"https://www.neira.app/img/Destiny2.png";
					author.Url = @"https://www.bungie.net/";
					break;
				case GameName.Division:
					author.Name = "The Division 2";
					author.IconUrl = @"https://www.neira.app/img/Division2.png";
					author.Url = @"https://tomclancy-thedivision.ubisoft.com/game/";
					break;
				case GameName.Warzone:
					author.Name = "CoD: Warzone";
					author.IconUrl = @"https://www.neira.app/img/Warzone.png";
					author.Url = @"https://www.callofduty.com/warzone";
					break;
				case GameName.Warframe:
					author.Name = "Warframe";
					author.IconUrl = "https://www.neira.app/img/Warframe.png";
					author.Url = @"https://www.warframe.com/";
					break;
				case GameName.WildRift:
					author.Name = "League of Legends: Wild Rift";
					author.IconUrl = "https://www.neira.app/img/WildRift.png";
					author.Url = "https://wildrift.leagueoflegends.com/";
					break;
				case GameName.LostArk:
					author.Name = "Lost Ark";
					author.IconUrl = "https://www.neira.app/img/LostArk.png";
					author.Url = "https://la.mail.ru/";
					break;
				case GameName.WoT:
					author.Name = "World of Tanks";
					author.IconUrl = "https://www.neira.app/img/WoT.png";
					author.Url = "https://worldoftanks.ru/";
					break;
				default:
					author.Name = "Unknown";
					break;
			}
			return author;
		}
	}
}
