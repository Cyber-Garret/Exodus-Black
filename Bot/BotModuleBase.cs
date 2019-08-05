using System.Threading.Tasks;

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Bot
{
	public class BotModuleBase : InteractiveBase
	{
		public IEmote BlueOk => new Emoji("\u1F197");
		public IEmote HeavyCheckMark => new Emoji("\u2714");
		public IEmote RedX => new Emoji("\u274C");

		public Task ReactAsync(IEmote emote)
			=> Context.Message.AddReactionAsync(emote);
	}
}
