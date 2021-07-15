using Discord.Commands;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Failsafe.Preconditions
{

    public class TeamMember : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var info = await context.Client.GetApplicationInfoAsync();

            return info.Team.TeamMembers.Any(x => x.User.Id == context.User.Id)
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Прошу прощения, но эта команда вам недоступна.");
        }
    }
}
