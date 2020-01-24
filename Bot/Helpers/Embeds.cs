using Bot.Entity;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Helpers
{
	class Embeds
	{
		internal static Embed BuildedExotic(Exotic exotic)
		{
			var embed = new EmbedBuilder
			{
				Color = Color.Gold,
				Title = exotic.Type + " - " + exotic.Name,
				ThumbnailUrl = exotic.IconUrl,
				ImageUrl = exotic.ImageUrl,
				Description = exotic.Description
			};
			// only weapon can have catalyst
			if (exotic.isWeapon)
			{
				embed.AddField("Катализатор", exotic.isHaveCatalyst == true ? "**Есть**" : "**Отсутствует**");
			}
			// main exotic perk and description
			embed.AddField(exotic.Perk, exotic.PerkDescription, true);
			// second perk if him unique
			if (exotic.SecondPerk != null && exotic.SecondPerkDescription != null)
				embed.AddField(exotic.SecondPerk, exotic.SecondPerkDescription, true);
			// where exotic gear can obtain
			embed.AddField("Как получить:", exotic.DropLocation, true);
			// simple footer =)
			embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: Cyber_Garret#5898",
				"https://www.bungie.net/common/destiny2_content/icons/ee21b5bc72f9e48366c9addff163a187.png");

			return embed.Build();
		}
	}
}
