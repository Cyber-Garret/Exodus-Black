using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neira.Bot.Services;
using Neira.Bot.Models.Db;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Neira.Bot.Services
{
	public class LevelingService
	{
		private readonly DiscordSocketClient Client;
		private readonly DbService dbService;
		private readonly NeiraContext neiraContext;
		public LevelingService(DiscordSocketClient socketClient, DbService db, NeiraContext neira)
		{
			Client = socketClient;
			dbService = db;
			neiraContext = neira;
		}

		/// <summary>
		/// Event for user level up in guild
		/// </summary>
		internal async Task Level(SocketGuildUser user)
		{
			//Load guild account settings
			var config = await dbService.GetGuildAccountAsync(user.Guild.Id);
			//Load user guild account
			GuildUserAccount userAccount = null;
			if (neiraContext.UserAccounts.Any(u => u.Id == user.Id))
				userAccount = await neiraContext.GuildUserAccounts.SingleAsync(u => u.UserId == user.Id && u.GuildId == user.Guild.Id);
			else
			{
				userAccount = new GuildUserAccount
				{
					UserId = user.Id,
					GuildId = user.Guild.Id
				};
				neiraContext.GuildUserAccounts.Add(userAccount);
			}

			DateTime now = DateTime.UtcNow;

			if (now < userAccount.LastXPMessage.AddSeconds(Global.MessageXPCooldown))
				return;

			userAccount.LastXPMessage = now;

			//save old level value
			uint oldLevel = userAccount.LevelNumber;
			userAccount.XP += 13;
			await neiraContext.SaveChangesAsync();
			//get new level value
			uint newLevel = userAccount.LevelNumber;
			//User level up?
			if (oldLevel != newLevel)
			{
				if (config.NotificationChannel != 0)
				{
					await Client.GetGuild(config.Id).GetTextChannel(config.NotificationChannel)
					   .SendMessageAsync($"Бип! Поздравляю страж {user.Username}, ты только что поднялся до уровня {newLevel}!");
					return;
				}
				else
				{
					var dM = await user.GetOrCreateDMChannelAsync();
					await dM.SendMessageAsync($"Бип! Поздравляю страж {user.Username}, ты только что поднялся до уровня {newLevel}!");
					return;
				}
			}
			else return;
		}

		/// <summary>
		/// Event for user global level up
		/// </summary>
		internal async Task GlobalLevel(SocketGuildUser user)
		{
			UserAccount userAccount = null;
			//Check if user exist
			if (neiraContext.UserAccounts.Any(u => u.Id == user.Id))
				userAccount = await neiraContext.UserAccounts.SingleAsync(u => u.Id == user.Id);
			else
			{
				var newUser = new UserAccount
				{
					Id = user.Id
				};
				neiraContext.UserAccounts.Add(newUser);
			}

			DateTime now = DateTime.UtcNow;

			if (now < userAccount.LastXPMessage.AddSeconds(Global.MessageXPCooldown))
				return;

			userAccount.LastXPMessage = now;
			//Save level old value
			uint oldLevel = userAccount.LevelNumber;
			userAccount.XP += 7;
			await neiraContext.SaveChangesAsync();
			//Get user new level value
			uint newLevel = userAccount.LevelNumber;
			//User level up?
			if (oldLevel != newLevel)
			{
				await CheckEngramRewards(user);
			}
			return;
		}

		/// <summary>
		/// Event for user message reward
		/// </summary>
		internal async Task MessageRewards(SocketGuildUser user, SocketMessage msg)
		{
			if (msg == null) return;
			//Check if user not spam in Neira DM channel.
			if (msg.Channel == msg.Author.GetOrCreateDMChannelAsync()) return;
			//Ignore all bots
			if (msg.Author.IsBot) return;

			//var userAccount = await dbService.GetUserAccountAsync(user);
			UserAccount userAccount = null;
			//Check if user exist
			if (neiraContext.UserAccounts.Any(u => u.Id == user.Id))
				userAccount = await neiraContext.UserAccounts.SingleAsync(u => u.Id == user.Id);
			else
			{
				var newUser = new UserAccount
				{
					Id = user.Id
				};
				neiraContext.UserAccounts.Add(newUser);
			}

			DateTime now = DateTime.UtcNow;

			if (now < userAccount.LastMessage.AddSeconds(Global.MessageRewardCooldown) || msg.Content.Length < Global.MessageRewardMinLenght)
				return;

			// Generate a randomized reward in the configured boundries
			userAccount.Glimmer += (ulong)Global.GetRandom.Next(1, 5);
			userAccount.LastMessage = now;

			await neiraContext.SaveChangesAsync();
		}

		/// <summary>
		/// Check if user able to receive new engram based on level
		/// </summary>
		private async Task CheckEngramRewards(SocketGuildUser user)
		{
			UserAccount userAccount = null;
			//Check if user exist
			if (neiraContext.UserAccounts.Any(u => u.Id == user.Id))
				userAccount = await neiraContext.UserAccounts.SingleAsync(u => u.Id == user.Id);
			else
			{
				var newUser = new UserAccount
				{
					Id = user.Id
				};
				neiraContext.UserAccounts.Add(newUser);
			}
			var channel = await user.GetOrCreateDMChannelAsync();
			int level = (int)userAccount.LevelNumber;

			int uc = level % 5;
			int rare = level % 10;
			int legendary = level % 15;
			int exotic = level % 20;
			if (exotic == 0)
			{
				userAccount.ExoticEngrams += 1;
				await channel.SendMessageAsync($"Поздравляю страж {user.Username}, ты получил **экзотическую** энграмму за достижение уровня {userAccount.LevelNumber}. ");
			}
			else if (legendary == 0)
			{
				userAccount.LegendaryEngrams += 1;
				await channel.SendMessageAsync($"Поздравляю страж {user.Username}, ты получил **легендарную** энграмму за достижение уровня {userAccount.LevelNumber}. ");
			}
			else if (rare == 0)
			{
				userAccount.RareEngrams += 1;
				await channel.SendMessageAsync($"Поздравляю страж {user.Username}, ты получил **редкую** энграмму за достижение уровня {userAccount.LevelNumber}. ");
			}
			else if (uc == 0)
			{
				userAccount.UncommonEngrams += 1;
				await channel.SendMessageAsync($"Поздравляю страж {user.Username}, ты получил **необычную** энграмму за достижение уровня {userAccount.LevelNumber}. ");
			}
			else
			{
				userAccount.CommonEngrams += 1;
				await channel.SendMessageAsync($"Поздравляю страж {user.Username}, ты получил **обычную** энграмму за достижение уровня {userAccount.LevelNumber}. ");
			}

			await neiraContext.SaveChangesAsync();
		}
	}
}

