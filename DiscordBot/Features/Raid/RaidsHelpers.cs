using Core;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Features.Raid
{
	internal static class RaidsHelpers
	{
		internal static async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			Core.Models.Db.ActiveRaid currentRaid;
			using (FailsafeContext context = new FailsafeContext())
			{
				currentRaid = context.ActiveRaids.FirstOrDefault(r => r.Id == reaction.MessageId);

				if (currentRaid != null)
				{
					var msg = await cache.GetOrDownloadAsync();

					if (reaction.Emote.Equals(RaidsCore.ReactOptions["2"]))
					{
						if (currentRaid.User2 == 0)
							currentRaid.User2 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
					{
						if (currentRaid.User3 == 0)
							currentRaid.User3 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
					{
						if (currentRaid.User4 == 0)
							currentRaid.User4 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
					{
						if (currentRaid.User5 == 0)
							currentRaid.User5 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
					{
						if (currentRaid.User6 == 0)
							currentRaid.User6 = reaction.User.Value.Id;
					}
					//await RaidsCore.HandleReaction(msg, reaction);
					await UpdateMessage(msg, RebuildEmbed(currentRaid));

					context.ActiveRaids.Update(currentRaid);
					context.SaveChanges();
				}
			}
		}
		internal static async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			Core.Models.Db.ActiveRaid currentRaid;
			using (FailsafeContext context = new FailsafeContext())
			{
				currentRaid = context.ActiveRaids.FirstOrDefault(r => r.Id == reaction.MessageId);

				if (currentRaid != null)
				{
					var msg = await cache.GetOrDownloadAsync();

					if (reaction.Emote.Equals(RaidsCore.ReactOptions["2"]))
					{
						if (currentRaid.User2 == reaction.User.Value.Id)
							currentRaid.User2 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
					{
						if (currentRaid.User3 == reaction.User.Value.Id)
							currentRaid.User3 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
					{
						if (currentRaid.User4 == reaction.User.Value.Id)
							currentRaid.User4 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
					{
						if (currentRaid.User5 == reaction.User.Value.Id)
							currentRaid.User5 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
					{
						if (currentRaid.User6 == reaction.User.Value.Id)
							currentRaid.User6 = 0;
					}
					//await RaidsCore.HandleReaction(msg, reaction);
					await UpdateMessage(msg, RebuildEmbed(currentRaid));

					context.ActiveRaids.Update(currentRaid);
					context.SaveChanges();
				}
			}
		}

		internal static EmbedBuilder RebuildEmbed(Core.Models.Db.ActiveRaid raid)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"{raid.DateExpire.ToShortDateString()} в {raid.DateExpire.TimeOfDay} по МСК. Рейд: {raid.Name}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl("http://neira.link/img/Raid_emblem.png");
			embed.WithDescription(raid.Description);

			embed.AddField("Рейд лидер", Program.Client.GetUser(raid.User1).Mention);

			if (raid.User2 != 0)
				embed.AddField("Страж #2", Program.Client.GetUser(raid.User2).Mention);
			else
				embed.AddField("Страж #2", "Свободно");
			if (raid.User3 != 0)
				embed.AddField("Страж #3", Program.Client.GetUser(raid.User3).Mention);
			else
				embed.AddField("Страж #3", "Свободно");
			if (raid.User4 != 0)
				embed.AddField("Страж #4", Program.Client.GetUser(raid.User4).Mention);
			else
				embed.AddField("Страж #4", "Свободно");
			if (raid.User5 != 0)
				embed.AddField("Страж #5", Program.Client.GetUser(raid.User5).Mention);
			else
				embed.AddField("Страж #5", "Свободно");
			if (raid.User6 != 0)
				embed.AddField("Страж #6", Program.Client.GetUser(raid.User6).Mention);
			else
				embed.AddField("Страж #6", "Свободно");

			embed.WithFooter("Что бы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		private static async Task UpdateMessage(IUserMessage socketMsg, EmbedBuilder embed)
		{
			await socketMsg.ModifyAsync(message =>
			{
				message.Embed = embed.Build();
				// This somehow can't be empty or it won't update the 
				// embed propperly sometimes... I don't know why
				// message.Content =  Constants.InvisibleString;
			});
		}

		#region Functions
		internal static (string, string) GetRaidInfo(RaidName raid)
		{

			switch (raid)
			{
				case RaidName.Leviathan:
					return ("Левиафан", "Короткое инфо о левике");
				case RaidName.EaterOfWorlds:
					return ("Пожиратель миров", "Короткое инфо о пожирателе");
				case RaidName.SpireOfStars:
					return ("Звездный шпиль", "Короткое инфо о шпиле");
				case RaidName.LastWish:
					return ("Последнее желание", "Короткое инфо о пж");
				case RaidName.ScourgeOfThePast:
					return ("Истребители прошлого", "Короткое инфо о ип");
				case RaidName.CrownOfSorrow:
					return ("Корона скорби", "Короткое инфо о короне");
				default:
					return ("Неизвестно", "У меня нет информации о том, чего я не знаю.");
			}
		}
		#endregion
	}

	internal enum RaidName
	{
		None,
		Leviathan,
		EaterOfWorlds,
		SpireOfStars,
		LastWish,
		ScourgeOfThePast,
		CrownOfSorrow
	}
}
