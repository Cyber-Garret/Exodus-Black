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
			var _raidTime = DateTime.Parse(raidTime);
			//var raid = new Raid
			//{
			//	Id = msg.Id,
			//	Name = raidName,
			//	dueDate = _raidTime,
			//	User1 = Context.User.Id
			//};

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"{_raidTime.Date.ToShortDateString()} в {_raidTime.TimeOfDay} по МСК. Рейд: Последнее желание");
			embed.WithColor(Color.DarkMagenta);
			embed.WithDescription($"Начало рейда: {_raidTime}");

			embed.AddField("Страж #1", Context.User.Mention, true);
			embed.AddField("Страж #2", "Свободно", true);
			embed.AddField("Страж #3", "Свободно", true);
			embed.AddField("Страж #4", "Свободно", true);
			embed.AddField("Страж #5", "Свободно", true);
			embed.AddField("Страж #6", "Свободно", true);
			embed.WithFooter("Что бы за вами закрепили место нажмите на реакцию, соответствующую месту.");
			var msg = await Context.Channel.SendMessageAsync(embed: embed.Build());
			//Slots
			try
			{
				await msg.AddReactionAsync(ActiveRaids.ReactOptions["2"]);
				await msg.AddReactionAsync(ActiveRaids.ReactOptions["3"]);
				await msg.AddReactionAsync(ActiveRaids.ReactOptions["4"]);
				await msg.AddReactionAsync(ActiveRaids.ReactOptions["5"]);
				await msg.AddReactionAsync(ActiveRaids.ReactOptions["6"]);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

		}
	}
}
