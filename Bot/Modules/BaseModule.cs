using Bot.Properties;
using Discord;
using Discord.Addons.Interactive;

namespace Bot.Modules
{
	public class BaseModule : InteractiveBase
	{
		internal const string NotInGuildText = ":x: | Эта команда не доступна в личных сообщениях.";
		internal IEmote WhiteHeavyCheckMark => new Emoji("\u2705");
		internal IEmote RedX => new Emoji("\u274C");
	}
}
