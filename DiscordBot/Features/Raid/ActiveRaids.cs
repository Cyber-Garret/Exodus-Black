using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Features.Raid
{
	internal static class ActiveRaids
	{
		public static readonly Dictionary<string, IEmote> ReactOptions;

		static ActiveRaids()
		{
			ReactOptions = new Dictionary<string, IEmote>
			{
				{ "2", new Emoji("2\u20e3")},
				{ "3", new Emoji("3\u20e3")},
				{ "4", new Emoji("4\u20e3")},
				{ "5", new Emoji("5\u20e3")},
				{ "6", new Emoji("6\u20e3")}
			};
		}
	}
}
