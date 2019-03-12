using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

namespace Neuromatrix.Modules.Administration
{
    public class ModerationModule : BotModuleBase
    {
        [Command("настройки")]
        public async Task InitGuildConfig()
        {
            #region Checks
            if (Context.User.IsBot) return; //Ignore bots
            if (Context.IsPrivate)
            {
                await Context.Channel.SendMessageAsync("IsPrivate");
                return;
            }

            SocketGuildUser user = Context.User as SocketGuildUser;

            if (!user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("Administrator");
                return;
            }
            #endregion

        }
    }
}
