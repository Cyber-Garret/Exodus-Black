using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Neira.Bot.Database;
using Neira.Bot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Neira.Bot.Services
{
	public class MilestoneService
	{
		private readonly DiscordSocketClient Client;
		private readonly EmoteService Emote;

		private Timer MilestoneTimer;

		public MilestoneService(DiscordSocketClient socketClient, EmoteService emoteService)
		{
			Client = socketClient;
			Emote = emoteService;
		}

		public void Initialize()
		{
			MilestoneTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
			};
			MilestoneTimer.Elapsed += MilestoneTimer_Elapsed;
		}

		private void MilestoneTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() =>
			{
				var timer = DateTime.Now.AddMinutes(15);
				using (var Db = new NeiraLinkContext())
				{
					var query = Db.ActiveMilestones.Include(r => r.Milestone).Include(ac => ac.MilestoneUsers).OrderBy(o => o.DateExpire);

					if (query.Count() > 0)
					{
						Parallel.ForEach(query, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async item =>
						{
							if (timer.Date == item.DateExpire.Date && timer.Hour == item.DateExpire.Hour && timer.Minute == item.DateExpire.Minute && timer.Second < 10)
							{
								//List with milestone leader and users who first click reaction
								var users = new List<ulong>();

								if (item.MilestoneUsers.Count > 0)
								{
									foreach (var user in item.MilestoneUsers)
										users.Add(user.UserId);

								}

								//Add leader in list for friendly remainder in direct messaging
								users.Add(item.Leader);
								await RaidNotificationAsync(users, item);
							}
						});
					}
				}
			});
		}

		public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(Emote.Raid))
				{
					using (var Db = new NeiraLinkContext())
					{
						//get milestone
						var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Include(mu => mu.MilestoneUsers).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

						if (milestone == null) return;

						//check reaction
						var UserExist = milestone.MilestoneUsers.Any(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId);

						if (reaction.UserId != milestone.Leader && !UserExist && milestone.MilestoneUsers.Count < 5)
						{
							Db.MilestoneUsers.Add(new MilestoneUser
							{
								MessageId = milestone.MessageId,
								UserId = reaction.UserId
							});
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
						else
						{
							var user = Client.GetUser(reaction.UserId);
							await msg.RemoveReactionAsync(Emote.Raid, user);
						}
					}
				}

			}
			catch (Exception ex)
			{
				await Logger.LogFullException(new LogMessage(LogSeverity.Critical, "Reaction Added in Milestone", ex.Message, ex));
			}
		}

		public async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(Emote.Raid))
				{
					using (var Db = new NeiraLinkContext())
					{
						//get milestone
						var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Include(mu => mu.MilestoneUsers).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

						if (milestone == null) return;

						//check reaction
						var UserExist = milestone.MilestoneUsers.Any(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId);

						if (reaction.UserId != milestone.Leader && UserExist)
						{
							var milestoneUser = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId);

							Db.Remove(milestoneUser);
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
						else
						{
							var user = Client.GetUser(reaction.UserId);
							await msg.RemoveReactionAsync(Emote.Raid, user);
						}
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.LogFullException(new LogMessage(LogSeverity.Critical, "Reaction Removed in Milestone", ex.Message, ex));
			}
		}

		private async void HandleReaction(IUserMessage message, ActiveMilestone activeMilestone)
		{
			var newEmbed = EmbedsHelper.MilestoneRebuild(Client, activeMilestone, Emote.Raid);
			if (newEmbed.Length != 0)
				await message.ModifyAsync(m => m.Embed = newEmbed);
		}

		public async Task RegisterNewMilestoneAsync(ulong msgId, SocketCommandContext context, int raidInfoId, DateTime date, string userMemo)
		{
			try
			{
				using (var Db = new NeiraLinkContext())
				{
					ActiveMilestone newMilestone = new ActiveMilestone
					{
						MessageId = msgId,
						TextChannelId = context.Channel.Id,
						GuildId = context.Guild.Id,
						MilestoneId = raidInfoId,
						Memo = userMemo,
						DateExpire = date,
						Leader = context.User.Id
					};

					Db.ActiveMilestones.Add(newMilestone);
					await Db.SaveChangesAsync();

					//Update Bot stat for website.
					var stats = Db.BotInfos.FirstOrDefault();
					stats.Milestones++;
					stats.Servers = Client.Guilds.Count;
					stats.Users = Client.Guilds.Sum(u => u.Users.Count);
					Db.BotInfos.Update(stats);
					Db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "Register Milestone Method", ex.Message, ex));
			}

		}

		public async Task RaidNotificationAsync(List<ulong> userIds, ActiveMilestone milestone)
		{
			foreach (var item in userIds)
			{
				if (item != 0)
				{
					try
					{
						var User = Client.GetUser(item);
						var Guild = Client.GetGuild(milestone.GuildId);
						IDMChannel Dm = await User.GetOrCreateDMChannelAsync();

						await Dm.SendMessageAsync(embed: EmbedsHelper.MilestoneRemindInDM(User, milestone, Guild));
						Thread.Sleep(1000);
					}
					catch (Exception ex)
					{
						await Logger.Log(new LogMessage(LogSeverity.Error, "RaidNotification", ex.Message, ex));
					}

				}
			}
		}
	}
}
