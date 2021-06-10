using Discord;
using Discord.Addons.Interactive;

namespace Failsafe.Modules
{
    public class RootModule : InteractiveBase
    {
        internal static IEmote WhiteHeavyCheckMark => new Emoji("\u2705");
        internal static IEmote RedX => new Emoji("\u274C");
    }
}
