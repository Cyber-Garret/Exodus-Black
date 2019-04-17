using Discord.Commands;
using System;
using System.Linq;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using API.Bungie.Models;
using API.Bungie.Profile;
using API.Bungie.GroupV2;
using Discord;

namespace Neuromatrix.Modules.Commands
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
            GetAllMembers members = new GetAllMembers();
            var z = await members.GetXur();
            var x = (await members.GetGuildMembers(3735687)).Response;

            EmbedBuilder embed = new EmbedBuilder();
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var item in x.results)
            {
                stringBuilder.Append($"Страж: {item.destinyUserInfo.displayName} Онлайн: {ConvertBoolean(item.isOnline)}\n");
            }

            string test = stringBuilder.ToString();
            embed.WithDescription(test);
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
