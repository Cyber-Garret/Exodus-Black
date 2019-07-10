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
			Core.Models.Db.ActiveRaid RaidMessage;
			using (FailsafeContext context = new FailsafeContext())
			{
				RaidMessage = context.ActiveRaids.FirstOrDefault(r => r.Id == reaction.MessageId);

				if (RaidMessage != null)
				{
					var msg = await cache.GetOrDownloadAsync();

					if (reaction.Emote.Equals(RaidsCore.ReactOptions["2"]))
					{
						if (RaidMessage.User2 == 0)
							RaidMessage.User2 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
					{
						if (RaidMessage.User3 == 0)
							RaidMessage.User3 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
					{
						if (RaidMessage.User4 == 0)
							RaidMessage.User4 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
					{
						if (RaidMessage.User5 == 0)
							RaidMessage.User5 = reaction.User.Value.Id;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
					{
						if (RaidMessage.User6 == 0)
							RaidMessage.User6 = reaction.User.Value.Id;
					}
					//await RaidsCore.HandleReaction(msg, reaction);
					await UpdateMessage(msg, RebuildEmbed(RaidMessage));

					context.ActiveRaids.Update(RaidMessage);
					context.SaveChanges();
				}
			}
		}
		internal static async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			Core.Models.Db.ActiveRaid RaidMessage;
			using (FailsafeContext context = new FailsafeContext())
			{
				RaidMessage = context.ActiveRaids.FirstOrDefault(r => r.Id == reaction.MessageId);

				if (RaidMessage != null)
				{
					var msg = await cache.GetOrDownloadAsync();

					if (reaction.Emote.Equals(RaidsCore.ReactOptions["2"]))
					{
						if (RaidMessage.User2 == reaction.User.Value.Id)
							RaidMessage.User2 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
					{
						if (RaidMessage.User3 == reaction.User.Value.Id)
							RaidMessage.User3 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
					{
						if (RaidMessage.User4 == reaction.User.Value.Id)
							RaidMessage.User4 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
					{
						if (RaidMessage.User5 == reaction.User.Value.Id)
							RaidMessage.User5 = 0;
					}
					if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
					{
						if (RaidMessage.User6 == reaction.User.Value.Id)
							RaidMessage.User6 = 0;
					}
					//await RaidsCore.HandleReaction(msg, reaction);
					await UpdateMessage(msg, RebuildEmbed(RaidMessage));

					context.ActiveRaids.Update(RaidMessage);
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
			embed.WithDescription($"**О рейде:** {raid.Description}");

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
