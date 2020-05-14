using Bot.Properties;

using Discord.Commands;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Bot.Modules
{
	public class GuildOwnerModule : BaseModule
	{
		private readonly ILogger<GuildOwnerModule> logger;

		public GuildOwnerModule(ILogger<GuildOwnerModule> logger)
		{
			this.logger = logger;
		}

		[Command("разбан")]
		public async Task MassUnban()
		{
			if (Context.User.Id != Context.Guild.OwnerId)
			{
				await ReplyAndDeleteAsync(Resources.OnlyForGuildOwner);
				return;
			}
			if (Context.Guild.CurrentUser.GuildPermissions.BanMembers)
			{
				int SucessCount = 0;
				int FailCount = 0;
				//Get list of all banned users in guild
				var bannedList = await Context.Guild.GetBansAsync();

				// Place simple start message 
				var workMessage = await Context.Channel.SendMessageAsync(string.Format(Resources.UnbanStart, bannedList.Count));

				foreach (var user in bannedList)
				{
					try
					{
						await Context.Guild.RemoveBanAsync(user.User);
						SucessCount++;
					}
					catch (Exception ex)
					{
						FailCount++;
						logger.LogWarning(ex, "Unban command");
					}

				}
				//We inform the server owner of the work done by changing the message.
				await workMessage.ModifyAsync(m => m.Content = string.Format(Resources.UnbanDone, bannedList.Count, SucessCount, FailCount));
			}
			else
			{
				await ReplyAsync(Resources.BanPermission);
			}


		}
	}
}
