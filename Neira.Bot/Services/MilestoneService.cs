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

namespace Neira.Bot.Services
{
	public class MilestoneService
	{
		private readonly DiscordSocketClient Client;
		private readonly EmoteService Emote;

		private const byte Second = 2;
		private const byte Three = 3;
		private const byte Four = 4;
		private const byte Five = 5;
		private const byte Six = 6;

		public MilestoneService(DiscordSocketClient socketClient, EmoteService emoteService)
		{
			Client = socketClient;
			Emote = emoteService;
		}

		public async Task MilestoneReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				using (var Db = new NeiraLinkContext())
				{
					//get milestone
					var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Include(mu => mu.MilestoneUsers).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

					if (milestone == null) return;

					if (reaction.Emote.Equals(Emote.Raid))
					{
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
					else if (reaction.Emote.Equals(Emote.ReactSecond))
					{
						if (!milestone.MilestoneUsers.Any(u => u.Place == 2))
						{
							Db.MilestoneUsers.Add(new MilestoneUser
							{
								MessageId = milestone.MessageId,
								UserId = reaction.UserId,
								Place = 2
							});
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactThird))
					{
						if (!milestone.MilestoneUsers.Any(u => u.Place == 3))
						{
							Db.MilestoneUsers.Add(new MilestoneUser
							{
								MessageId = milestone.MessageId,
								UserId = reaction.UserId,
								Place = 3
							});
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactFourth))
					{
						if (!milestone.MilestoneUsers.Any(u => u.Place == 4))
						{
							Db.MilestoneUsers.Add(new MilestoneUser
							{
								MessageId = milestone.MessageId,
								UserId = reaction.UserId,
								Place = 4
							});
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactFifth))
					{
						if (!milestone.MilestoneUsers.Any(u => u.Place == 5))
						{
							Db.MilestoneUsers.Add(new MilestoneUser
							{
								MessageId = milestone.MessageId,
								UserId = reaction.UserId,
								Place = 5
							});
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactSixth))
					{
						if (!milestone.MilestoneUsers.Any(u => u.Place == 6))
						{
							Db.MilestoneUsers.Add(new MilestoneUser
							{
								MessageId = milestone.MessageId,
								UserId = reaction.UserId,
								Place = 6
							});
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.LogFullException(new LogMessage(LogSeverity.Critical, "Reaction Added in Milestone", ex.Message, ex));
			}
		}

		public async Task MilestoneReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				using (var Db = new NeiraLinkContext())
				{
					//get milestone
					var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Include(mu => mu.MilestoneUsers).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

					if (milestone == null) return;

					if (reaction.Emote.Equals(Emote.Raid))
					{
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
					else if (reaction.Emote.Equals(Emote.ReactSecond))
					{
						if (milestone.MilestoneUsers.Any(m => m.Place == Second && m.UserId == reaction.UserId && m.MessageId == reaction.MessageId))
						{
							var user = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId && u.Place == Second);
							Db.Remove(user);
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactThird))
					{
						if (milestone.MilestoneUsers.Any(m => m.Place == Three && m.UserId == reaction.UserId && m.MessageId == reaction.MessageId))
						{
							var user = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId && u.Place == Three);
							Db.Remove(user);
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactFourth))
					{
						if (milestone.MilestoneUsers.Any(m => m.Place == Four && m.UserId == reaction.UserId && m.MessageId == reaction.MessageId))
						{
							var user = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId && u.Place == Four);
							Db.Remove(user);
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactFifth))
					{
						if (milestone.MilestoneUsers.Any(m => m.Place == Five && m.UserId == reaction.UserId && m.MessageId == reaction.MessageId))
						{
							var user = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId && u.Place == Five);
							Db.Remove(user);
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
						}
					}
					else if (reaction.Emote.Equals(Emote.ReactSixth))
					{
						if (milestone.MilestoneUsers.Any(m => m.Place == Six && m.UserId == reaction.UserId && m.MessageId == reaction.MessageId))
						{
							var user = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId && u.Place == Six);
							Db.Remove(user);
							await Db.SaveChangesAsync();
							HandleReaction(msg, milestone);
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
			if (activeMilestone.Milestone.MaxSpace == activeMilestone.MilestoneUsers.Count + 1)
			{
				await message.RemoveAllReactionsAsync();
				await message.ModifyAsync(c => c.Embed = EmbedsHelper.MilestoneEnd(Client, activeMilestone));

				await RaidNotificationAsync(activeMilestone);
			}
		}

		private async Task UpdateBotStatAsync()
		{
			using (var Db = new NeiraLinkContext())
			{
				//Update Bot stat for website.
				var stats = Db.BotInfos.FirstOrDefault();
				stats.Milestones++;
				stats.Servers = Client.Guilds.Count;
				stats.Users = Client.Guilds.Sum(u => u.Users.Count);
				Db.BotInfos.Update(stats);
				await Db.SaveChangesAsync();
			}
		}

		public async Task RegisterMilestoneAsync(ulong msgId, SocketCommandContext context, MilestoneType type, byte raidInfoId, string userMemo)
		{
			try
			{
				using (var Db = new NeiraLinkContext())
				{
					ActiveMilestone newMilestone = new ActiveMilestone
					{
						MessageId = msgId,
						GuildId = context.Guild.Id,
						MilestoneId = raidInfoId,
						Memo = userMemo,
						CreateDate = DateTime.Now,
						Leader = context.User.Id,
						MilestoneType = type
					};

					Db.ActiveMilestones.Add(newMilestone);
					await Db.SaveChangesAsync();

					_ = Task.Run(async () => await UpdateBotStatAsync());
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "Register Milestone Method", ex.Message, ex));
			}

		}

		public async Task RaidNotificationAsync(ActiveMilestone milestone)
		{
			try
			{
				var Guild = Client.GetGuild(milestone.GuildId);
				var Leader = Client.GetUser(milestone.Leader);
				var LeaderDM = await Leader.GetOrCreateDMChannelAsync();

				await LeaderDM.SendMessageAsync(embed: EmbedsHelper.MilestoneRemindInDM(Client, milestone, Guild));

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{
						if (user.UserId == milestone.Leader) continue;
						var LoadedUser = Client.GetUser(user.UserId);

						var DM = await LoadedUser.GetOrCreateDMChannelAsync();
						await DM.SendMessageAsync(embed: EmbedsHelper.MilestoneRemindInDM(Client, milestone, Guild));
					}
					catch (Exception ex)
					{
						await Logger.LogFullException(new LogMessage(LogSeverity.Error, "RaidNotification in DM of user", ex.Message, ex));
					}
				}
				using (var Db = new NeiraLinkContext())
				{
					var expiredMilestone = Db.ActiveMilestones.Include(m => m.MilestoneUsers).First(m => m.MessageId == milestone.MessageId);
					Db.ActiveMilestones.Remove(expiredMilestone);
					Db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				await Logger.LogFullException(new LogMessage(LogSeverity.Error, "RaidNotification Global", ex.Message, ex));
			}

		}
	}

}
