using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Neira.Bot
{
	internal static class GlobalVariables
	{
		#region Economy
		internal const ulong DailyGlimmerGain = 50;
		public const int MessageRewardCooldown = 30;
		public const int MessageXPCooldown = 6;
		public const int MessageRewardMinLenght = 20;
		#endregion
		internal const string InvisibleString = "\u200b";
		internal const string NotInGuildText = ":x: | Эта команда не доступна в личных сообщениях.";

		internal static CultureInfo culture = new CultureInfo("ru-Ru");
		internal static Random GetRandom = new Random();
	}
}
