using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Neuromatrix
{
    public class NeiraModuleBase : ModuleBase<SocketCommandContext>
    {
        public IEmote Ok => new Emoji("🆗");

        public Task ReactAsync(IEmote emote)
            => Context.Message.AddReactionAsync(emote);
    }
}
