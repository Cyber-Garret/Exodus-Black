using Bot.Core.Data;
using Bot.Models;
using Bot.Properties;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class MilestoneService
	{
		private readonly ILogger _logger;
		private readonly DiscordSocketClient _discord;
		private readonly EmoteService _emote;

		public MilestoneService(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<MilestoneService>>();
			_discord = service.GetRequiredService<DiscordSocketClient>();
			_emote = service.GetRequiredService<EmoteService>();
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
					var UserExist = milestone.MilestoneUsers.Any(u => u == reaction.UserId);

					if (reaction.UserId != milestone.Leader && !UserExist && milestone.MilestoneUsers.Count + 1 < milestone.MilestoneInfo.MaxSpace)
					{
						milestone.MilestoneUsers.Add(reaction.UserId);
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);

						HandleReaction(msg, milestone);
					}
					else
					{
						var user = _discord.GetUser(reaction.UserId);
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
					var UserExist = milestone.MilestoneUsers.Any(u => u == reaction.UserId);

					if (reaction.UserId != milestone.Leader && UserExist)
					{
						milestone.MilestoneUsers.Remove(reaction.UserId);
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);
						HandleReaction(msg, milestone);
					}
					else
					{
						var user = _discord.GetUser(reaction.UserId);
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
			var newEmbed = MilestoneEmbed(milestone);
			if (newEmbed.Length != 0)
				await message.ModifyAsync(m => m.Embed = newEmbed);
		}

		public async Task RaidNotificationAsync(Milestone milestone)
		{
			try
			{
				var Guild = _discord.GetGuild(milestone.GuildId);
				var RemindEmbed = MilestoneRemindEmbed(milestone);

				var Leader = _discord.GetUser(milestone.Leader);
				var LeaderDM = await Leader.GetOrCreateDMChannelAsync();

				await LeaderDM.SendMessageAsync(embed: RemindEmbed);

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{
						var LoadedUser = _discord.GetUser(user);

						var DM = await LoadedUser.GetOrCreateDMChannelAsync();
						await DM.SendMessageAsync(embed: RemindEmbed);
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

		public Embed MilestoneEmbed(Milestone milestone)
		{
			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.MilEmbTitle,
						  milestone.DateExpire.ToString("dd.MM.yyyy"),
						  milestone.DateExpire.ToString("HH:MM"),
						  GetCityForMilestoneEmbed(milestone.DateExpire.Offset),
						  milestone.MilestoneInfo.Type,
						  milestone.MilestoneInfo.Name),
				Author = GetGame(milestone.MilestoneInfo.Game),
				ThumbnailUrl = milestone.MilestoneInfo.Icon,
				Color = GetColorByType(milestone.MilestoneInfo.MilestoneType)

			};
			if (milestone.Note != null)
				embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

			var leader = _discord.GetUser(milestone.Leader);

			embed.AddField(Resources.MilEmbInfTitleField, string.Format(Resources.MilEmbInfDescField, leader.Mention, leader.Username, _emote.Raid));

			if (milestone.MilestoneUsers.Count > 0)
			{
				var embedFieldUsers = new EmbedFieldBuilder
				{
					Name = Resources.MilEmbMemTitleField
				};
				int count = 2;
				foreach (var user in milestone.MilestoneUsers)
				{
					var discordUser = _discord.GetUser(user);
					embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
					count++;
				}

				embed.AddField(embedFieldUsers);
			}
			embed.WithFooter($"ID: {milestone.MessageId}");

			return embed.Build();
		}

		public Embed GetMilestonesNameEmbed(MilestoneType type)
		{
			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.MilInfEmbTitle, GetNameForMilestoneType(type)),
				Color = GetColorByType(type)
			};
			if (type == MilestoneType.Raid)
			{
				var Des2names = string.Empty;
				var Div2names = string.Empty;
				foreach (var item in MilestoneInfoData.GetMilestonesByType(type))
				{
					if (item.Game == GameName.Destiny)
						Des2names += $"**{item.Alias}** - {item.Name}\n";
					else
						Div2names += $"**{item.Alias}** - {item.Name}\n";

				}
				embed.AddField("Destiny 2", Des2names);
				embed.AddField("The Division 2", Div2names);
			}
			else
			{
				var text = string.Empty;
				foreach (var milestone in MilestoneInfoData.GetMilestonesByType(type))
				{
					text += $"**{milestone.Alias}** - {milestone.Name}\n";
				}
				embed.Description = text;
			}
			embed.WithFooter(Resources.EmbFooterAboutDel);

			return embed.Build();
		}

		private Embed MilestoneRemindEmbed(Milestone milestone)
		{
			var guild = _discord.GetGuild(milestone.GuildId);

			var embed = new EmbedBuilder()
			{
				Title = string.Format(Resources.MilRemEmbTitle, milestone.MilestoneInfo.Type),
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
			var leader = _discord.GetUser(milestone.Leader);
			embedFieldUsers.Value = $"#1 {leader.Mention} - {leader.Username}\n";
			int count = 2;
			foreach (var user in milestone.MilestoneUsers)
			{

				var discordUser = _discord.GetUser(user);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";

				count++;
			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			embed.WithFooter(string.Format(Resources.MilRemEmbFooter, milestone.MilestoneInfo.Type, milestone.MilestoneInfo.Name, guild.Name), guild.IconUrl);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}

		private Color GetColorByType(MilestoneType type)
		{
			return type switch
			{
				MilestoneType.Raid => Color.DarkMagenta,
				MilestoneType.Nightfall => Color.DarkGreen,
				MilestoneType.Other => Color.DarkBlue,
				_ => Color.Magenta,
			};
		}

		private string GetNameForMilestoneType(MilestoneType type)
		{
			return type switch
			{
				MilestoneType.Raid => Resources.Raid,
				MilestoneType.Nightfall => Resources.Nightfall,
				MilestoneType.Other => Resources.Other,
				_ => Resources.Unknown,
			};
		}

		private EmbedAuthorBuilder GetGame(GameName game)
		{
			var author = new EmbedAuthorBuilder();
			if (game == GameName.Destiny)
			{
				author.Name = "Destiny 2";
				author.IconUrl = @"https://neira.su/img/Destiny2.png";
				author.Url = @"https://www.bungie.net/";
			}
			else if (game == GameName.Division)
			{
				author.Name = "The Division 2";
				author.IconUrl = @"https://neira.su/img/Division2.png";
				author.Url = @"https://tomclancy-thedivision.ubisoft.com/game/";
			}
			else
			{
				author.Name = "Unknown";
			}
			return author;
		}


		private string GetCityForMilestoneEmbed(TimeSpan Offset)
		{
			return Offset.Hours switch
			{
				3 => "Москве",
				2 => "Киеву",
				_ => "Unknown",
			};
		}
	}
}