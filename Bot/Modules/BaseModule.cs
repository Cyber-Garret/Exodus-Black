using Discord;
using Discord.Addons.Interactive;

namespace Bot.Modules
{
	public class BaseModule : InteractiveBase
	{
		internal IEmote WhiteHeavyCheckMark => new Emoji("\u2705");
		internal IEmote RedX => new Emoji("\u274C");
	}
}
