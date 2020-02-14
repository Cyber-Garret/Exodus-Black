using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
	internal static class GlobalVariables
	{
		// Modules
		internal const string InvisibleString = "\u200b";
		internal const string NotInGuildText = ":x: | Эта команда не доступна в личных сообщениях.";

		internal static Random GetRandom = new Random();
	}
}
