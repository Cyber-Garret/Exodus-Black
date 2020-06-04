using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Bot.Modules
{
	public class RootModule : InteractiveBase
	{
		internal IEmote WhiteHeavyCheckMark => new Emoji("\u2705");
		internal IEmote RedX => new Emoji("\u274C");
	}
}
