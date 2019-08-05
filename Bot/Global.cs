using Discord;
using System.Collections.Generic;
using System.Globalization;

namespace Bot
{
	internal static class Global
	{
		internal static readonly string InvisibleString = "\u200b";
		internal static CultureInfo culture = new CultureInfo("ru-Ru");
		internal static readonly Dictionary<string, IEmote> ReactPlaceNumber = new Dictionary<string, IEmote>
		{
			{ "2", new Emoji("2\u20e3")},
			{ "3", new Emoji("3\u20e3")},
			{ "4", new Emoji("4\u20e3")},
			{ "5", new Emoji("5\u20e3")},
			{ "6", new Emoji("6\u20e3")}
		};
	}
}
