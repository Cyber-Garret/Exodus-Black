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
			RaidName raidEnum = ParseRaidName(raidName);
			if (raidEnum == RaidName.None)
			{
				//todo message
				return;
			}
			DateTime dateTime/* = new DateTime()*/;
			DateTime.TryParse(raidTime, out dateTime);

			if (dateTime == new DateTime() || dateTime < DateTime.Now)
			{
				//todo message
				return;
			}

			var msg = await Context.Channel.SendMessageAsync(embed: RaidsCore.StartRaidEmbed(Context.User, raidEnum, dateTime).Build());
			RaidsCore.RegisterRaid(msg.Id, Context.Guild.Name, Context.User.Id, raidEnum, dateTime);
			//Slots
			await msg.AddReactionAsync(RaidsCore.ReactOptions["2"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["3"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["4"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["5"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["6"]);

		}

		private RaidName ParseRaidName(string raidname)
		{
			raidname.ToLower();

			if (raidname == "левик" || raidname == "левиафан")
				return RaidName.Leviathan;
			else if (raidname == "пм" || raidname == "пожиратель миров")
				return RaidName.EaterOfWorlds;
			else if (raidname == "зш" || raidname == "звездный шпиль")
				return RaidName.SpireOfStars;
			else if (raidname == "пж" || raidname == "последнее желание")
				return RaidName.LastWish;
			else if (raidname == "ип" || raidname == "истребители прошлого")
				return RaidName.ScourgeOfThePast;
			else if (raidname == "кс" || raidname == "корона скорби")
				return RaidName.CrownOfSorrow;
			else
				return RaidName.None;
		}
	}
}
