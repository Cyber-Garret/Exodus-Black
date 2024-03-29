﻿using System;
using System.Globalization;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Models;
using Failsafe.Models.Enums;
using Failsafe.Preconditions;
using Failsafe.Properties;
using Failsafe.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Failsafe.Modules
{
	[RequireContext(ContextType.Guild), Cooldown(5)]
	public class MilestoneModule : RootModule
	{
		private readonly IConfiguration _config;
		private readonly ILogger<MilestoneModule> _logger;
		private readonly DiscordSocketClient _discord;
		private readonly MilestoneService _milestoneHandler;
		private readonly EmoteService _emote;

		public MilestoneModule(IConfiguration config, ILogger<MilestoneModule> logger, DiscordSocketClient discord, MilestoneService milestoneHandler, EmoteService emote)
		{
			_config = config;
			_logger = logger;
			_discord = discord;
			_milestoneHandler = milestoneHandler;
			_emote = emote;
		}

		#region Commands
		[Command("raid"), Alias("рейд")]
		public async Task RegisterRaid(string raidName, string raidTime, [Remainder] string leaderNote = null)
		{
			await GoMilestoneAsync(raidName, MilestoneType.Raid, raidTime, leaderNote);
		}

		[Command("strike"), Alias("налёт", "наліт")]
		public async Task RegisterStrike(string nightfallName, string nightfallTime, [Remainder] string leaderNote = null)
		{
			await GoMilestoneAsync(nightfallName, MilestoneType.Nightfall, nightfallTime, leaderNote);
		}

		[Command("coll"), Alias("сбор", "збір")]
		public async Task RegisterOther(string otherName, string otherTime, [Remainder] string leaderNote = null)
		{
			await GoMilestoneAsync(otherName, MilestoneType.Other, otherTime, leaderNote);
		}

		[Command("reserve"), Alias("резерв")]
		public async Task Reserve(ulong milestoneId, int count)
		{

			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);
			if (milestone.Leader == Context.User.Id)
			{
				//calculate free space in milestone
				var freeSpace = milestone.MilestoneInfo.MaxSpace - (milestone.MilestoneUsers.Count + 1);

				//check count value is adequate?
				if (count < 1 || count >= freeSpace)
				{
					await ReplyAsync(string.Format(Resources.MilReservedFail, count));
				}
				else
				{
					try
					{
						for (int i = 0; i < count; i++)
						{
							//100500 is reserved man
							milestone.MilestoneUsers.Add(GlobalVariables.ReservedID);
						}
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);

						var channel = (ISocketMessageChannel)Context.Guild.GetChannel(milestone.ChannelId);
						var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);
						var embed = await _milestoneHandler.MilestoneEmbed(milestone);

						await msg.ModifyAsync(m => m.Embed = embed);

						await ReplyAndDeleteAsync(string.Format(Resources.MilReservedOk, count, msg.GetJumpUrl()));
					}
					catch (Exception ex)
					{
						await ReplyAndDeleteAsync(string.Format(Resources.Error, ex.Message));
						_logger.LogError(ex, "Milestone reserve command");
					}
				}
			}
			else
				await ReplyAsync(Resources.MilNotLeader);
		}

		[Command("note"), Alias("заметка", "нотатка")]
		public async Task ChangeNote(ulong milestoneId, [Remainder] string note = null)
		{
			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);
			if (milestone.Leader == Context.User.Id)
			{
				try
				{
					milestone.Note = note;
					ActiveMilestoneData.SaveMilestones(milestone.MessageId);

					var channel = (ISocketMessageChannel)Context.Guild.GetChannel(milestone.ChannelId);
					var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);
					var embed = await _milestoneHandler.MilestoneEmbed(milestone);

					await msg.ModifyAsync(m => m.Embed = embed);

					await ReplyAndDeleteAsync(string.Format(Resources.MilNoteEdited, msg.GetJumpUrl()));
				}
				catch (Exception ex)
				{
					await ReplyAndDeleteAsync(string.Format(Resources.Error, ex.Message));
					_logger.LogError(ex, "Milestone note command");
				}
			}
			else
				await ReplyAndDeleteAsync(Resources.MilNotLeader);
		}

		[Command("transfer"), Alias("передать", "передати")]
		public async Task ChangeLeader(ulong milestoneId, SocketGuildUser newLeader)
		{
			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);

			if (milestone.Leader == Context.User.Id)
			{
				var IsReacted = false;
				if (milestone.MilestoneUsers.Contains(newLeader.Id))
				{
					milestone.MilestoneUsers.Remove(newLeader.Id);
					milestone.Leader = newLeader.Id;
					IsReacted = true;
				}
				else
					milestone.Leader = newLeader.Id;
				ActiveMilestoneData.SaveMilestones(milestoneId);

				var channel = (ISocketMessageChannel)Context.Guild.GetChannel(milestone.ChannelId);
				var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);

				if (IsReacted)
					await msg.RemoveReactionAsync(_emote.Raid, newLeader);

				var embed = await _milestoneHandler.MilestoneEmbed(milestone);

				await msg.ModifyAsync(m => m.Embed = embed);

				await ReplyAndDeleteAsync(string.Format(Resources.MilChangeLeader, msg.GetJumpUrl()));
			}
			else
				await ReplyAndDeleteAsync(Resources.MilNotLeader);
		}

		[Command("time"), Alias("перенос")]
		public async Task ChangeTime(ulong milestoneId, string newTime)
		{
			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);
			if (milestone.Leader == Context.User.Id)
			{
				//Get time formats from appsetings.json
				var timeFormats = _config.GetSection("Bot:TimeFormats").Get<string[]>();
				var isSucess = DateTime.TryParseExact(newTime, timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);

				if (isSucess)
				{
					var guild = GuildData.GetGuildAccount(milestone.GuildId);
					var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
					var raidTimeOffset = new DateTimeOffset(dateTime, guildTimeZone.BaseUtcOffset);

					var now = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guildTimeZone).AddMinutes(15), guildTimeZone.BaseUtcOffset);

					if (raidTimeOffset < now)
						await ReplyAndDeleteAsync(Resources.MilPastTime);
					else
					{
						milestone.DateExpire = raidTimeOffset;
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);

						try
						{
							var channel = _discord.GetGuild(milestone.GuildId).GetTextChannel(milestone.ChannelId);
							var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);
							var embed = await _milestoneHandler.MilestoneEmbed(milestone);

							await msg.ModifyAsync(m => m.Embed = embed);

							await ReplyAndDeleteAsync(string.Format(Resources.MilTimeChanged, msg.GetJumpUrl()));

							//Notif all user about time changed
							await _milestoneHandler.TimeChangedNotificationAsync(milestone);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Milestone change time error");
							await ReplyAsync(string.Format(Resources.Error, ex.Message));
						}
					}

				}
				else
					await ReplyAndDeleteAsync(Resources.MilTimeError);
			}
			else
				await ReplyAndDeleteAsync(Resources.MilNotLeader);
		}

		[Command("cancel"), Alias("отмена", "відміна")]
		public async Task CloseMilestone(ulong milestoneId, [Remainder] string reason = null)
		{
			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);
			if (milestone.Leader == Context.User.Id)
			{
				if (reason == null)
				{
					await ReplyAndDeleteAsync(Resources.MilNoReason);
					return;
				}
				ActiveMilestoneData.RemoveMilestone(milestone.MessageId);

				try
				{
					var channel = (ISocketMessageChannel)Context.Guild.GetChannel(milestone.ChannelId);
					var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);

					await msg.ModifyAsync(m =>
					{
						m.Content = string.Empty;
						m.Embed = DeleteMilestone(milestone, reason);
					});

					await msg.RemoveAllReactionsAsync();

					await ReplyAndDeleteAsync(string.Format(Resources.MilCanceled, msg.GetJumpUrl()));
				}
				catch (Exception ex)
				{
					await ReplyAndDeleteAsync(string.Format(Resources.Error, ex.Message));
					_logger.LogError(ex, "Canceling milestone");
				}
			}
			else
				await ReplyAndDeleteAsync(Resources.MilNotLeader);
		}
		#endregion

		#region Methods
		private async Task GoMilestoneAsync(string searchName, MilestoneType type, string time, string Note = null)
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				var milestoneInfo = MilestoneInfoData.SearchMilestoneData(searchName, type);
				Embed embed;
				if (milestoneInfo == null)
				{
					embed = MilestoneService.GetMilestonesNameEmbed(Context.Guild, type);
					await ReplyAndDeleteAsync(string.Format(Resources.MilNotFound, Context.User.Mention), embed: embed, timeout: TimeSpan.FromMinutes(1));
					return;
				}

				//Get time formats from appsetings.json
				var timeFormats = _config.GetSection("Bot:TimeFormats").Get<string[]>();
				var isSucess = DateTime.TryParseExact(time, timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);

				if (isSucess)
				{
					var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
					var raidTimeOffset = new DateTimeOffset(dateTime, guildTimeZone.BaseUtcOffset);

					var now = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guildTimeZone), guildTimeZone.BaseUtcOffset);

					if (raidTimeOffset < now)
					{

						await ReplyAndDeleteAsync(Resources.MilPastTime);
						return;
					}

					var msg = await ReplyAsync(Resources.MilBake);

					var newMilestone = new Milestone
					{
						MessageId = msg.Id,
						ChannelId = Context.Channel.Id,
						GuildId = Context.Guild.Id,
						MilestoneInfo = milestoneInfo,
						Note = Note,
						Leader = Context.User.Id,
						DateExpire = raidTimeOffset
					};

					ActiveMilestoneData.AddMilestone(newMilestone);

					embed = await _milestoneHandler.MilestoneEmbed(newMilestone);

					await msg.ModifyAsync(a =>
					{
						a.Content = guild.GlobalMention;
						a.Embed = embed;
					});
					//Slots
					await msg.AddReactionAsync(_emote.Raid);
				}
				else
					await ReplyAndDeleteAsync(Resources.MilTimeError);
			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync(string.Format(Resources.MilError, ex.Message));
				_logger.LogError(ex, "Milestone method");
			}
		}

		private Embed DeleteMilestone(Milestone milestone, string reason)
		{
			var embed = new EmbedBuilder
			{
				Title = $"{milestone.MilestoneInfo.Type }: { milestone.MilestoneInfo.Name}",
				Description = string.Format(Resources.MilEmbDescCanceled, reason)
			};
			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = Resources.MilEmbMemTitleField
			};
			var leader = _discord.GetUser(milestone.Leader);
			embedFieldUsers.Value = $"#1 {leader.Mention} - {leader.Username}\n";
			if (milestone.MilestoneUsers.Count > 0)
			{
				int count = 2;
				foreach (var user in milestone.MilestoneUsers)
				{
					//100500 is reserved man
					if (user == GlobalVariables.ReservedID)
					{
						embedFieldUsers.Value += $"#{count} {Resources.MilReserved}\n";
					}
					else
					{
						var discordUser = _discord.GetUser(user);
						embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
					}
					count++;
				}
			}
			embed.AddField(embedFieldUsers);

			return embed.Build();
		}
		#endregion
	}
}
