using Discord;
using Discord.Commands;
using DiscordBot.Preconditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Core.Models.Db;
using DiscordBot.Features.Raid;

namespace DiscordBot.Modules
{
	[Cooldown(10)]
	public class RaidModule : BotModuleBase
	{
		[Command("сбор")]
		public async Task RaidCollection(string raidName, [Remainder]string raidTime)
		{

			var msg = await Context.Channel.SendMessageAsync(embed: RaidsCore.RaidEmbed(Context.User, raidName, raidTime).Build());
			RaidsCore.RegisterRaid(msg.Id, Context.User.Id, raidName, raidTime);
			//Slots
			await msg.AddReactionAsync(RaidsCore.ReactOptions["2"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["3"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["4"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["5"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["6"]);

		}
	}
}
