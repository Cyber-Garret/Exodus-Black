using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Neira.Bot
{
	internal static class GlobalVariables
	{
		// Modules
		internal const string InvisibleString = "\u200b";
		internal const string NotInGuildText = ":x: | Эта команда не доступна в личных сообщениях.";
		internal const ulong Cyber_Garret = 316272461291192322;

		internal static CultureInfo culture = new CultureInfo("ru-Ru");
		internal static Random GetRandom = new Random();
	}
}
