using System;
using System.Threading.Tasks;

using Discord.Commands;

using DiscordBot.Preconditions;
using DiscordBot.Features.Raid;

namespace DiscordBot.Modules
{
	[Cooldown(10)]
	[Group("рейд")]
	public class RaidModule : BotModuleBase
	{
		[Command("сбор")]
		public async Task RaidCollection(string raidName, [Remainder]string raidTime)
		{
			var raidInfo = FailsafeDbOperations.GetRaidInfo(raidName);

			if (raidInfo == null)
			{
				//todo message
				return;
			}

			DateTime.TryParse(raidTime, out DateTime dateTime);

			if (dateTime == new DateTime() || dateTime < DateTime.Now)
			{
				//todo message
				return;
			}

			var msg = await Context.Channel.SendMessageAsync(embed: RaidsCore.StartRaidEmbed(Context.User, raidInfo, dateTime).Build());
			await RaidsCore.RegisterRaidAsync(msg.Id, Context.Guild.Name, Context.User.Id, raidInfo.Id, dateTime);
			//Slots
			await msg.AddReactionAsync(RaidsCore.ReactOptions["2"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["3"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["4"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["5"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["6"]);

		}
	}
}
