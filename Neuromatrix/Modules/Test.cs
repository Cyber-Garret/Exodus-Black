using Discord.Commands;
using System;
using System.Linq;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Discord;

using API.Bungie;
namespace DiscordBot.Modules.Commands
{
    public class Test : BotModuleBase
    {
        public static string ConvertBoolean(bool? boolean)
        {
            return boolean == true ? "**Да**" : "**Нет**";
        }

        [Command("test")]
        public async Task TestTask()
        {
            int firstClanId = 3526561;
            int secondClanId = 3735687;
            //List<>
            DestinyClanMemberWeekStat weekStat = new DestinyClanMemberWeekStat(firstClanId);
            await weekStat.GetGuildMemberId();
            await weekStat.GetGuildCharacterIds();
            
            EmbedBuilder embed = new EmbedBuilder();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
