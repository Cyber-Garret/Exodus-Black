﻿using Discord;
using Discord.Addons.Interactive;

namespace Neira.Bot
{
	public class BotModuleBase : InteractiveBase
	{
		public IEmote WhiteHeavyCheckMark => new Emoji("\u2705");
		public IEmote RedX => new Emoji("\u274C");

		public EmbedFooterBuilder NeiraWebsite => new EmbedFooterBuilder { Text = "neira.su", IconUrl = "http://neira.su/img/neira.png" };
	}
}
