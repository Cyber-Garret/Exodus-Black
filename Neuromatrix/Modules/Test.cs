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
        public async Task TestTask(int GuildId)
        {
            await Context.Channel.SendMessageAsync("Начинаю проводить подсчет.");
            DestinyClanMemberWeekStat weekStat = new DestinyClanMemberWeekStat(GuildId);
            var GuildInfo = weekStat.GuildInfoAsync(GuildId).Result.Response;
            var GuildMembers = await weekStat.GetGuildCharacterIdsAsync();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Онлайн статус стражей");
            embed.WithColor(Color.Orange);
            embed.WithUrl($"https://www.bungie.net/ru/ClanV2?groupId={GuildInfo.Detail.GroupId}");
            embed.WithDescription($"Клан зарегистрирован **{GuildInfo.Detail.CreationDate.ToString("dd-MM-yyyy")}**\n" +
                $"В данный момент в клане **{GuildInfo.Detail.MemberCount}**/100 стражей.\n" +
                $"Девиз клана - **{GuildInfo.Detail.Motto}**");
            List<string> _ThisDay = new List<string>();
            List<string> _Yesterday = new List<string>();
            List<string> _FewDays = new List<string>();
            List<string> _ThisWeek = new List<string>();
            List<string> _MoreOneWeek = new List<string>();
            foreach (var item in GuildMembers)
            {
                var days = (DateTime.Today.Day - item.LastOnlineDate.Day);
                if (days == 0)
                {
                    _ThisDay.Add(item.MemberName);
                }
                else if (days == 1)
                {
                    _Yesterday.Add(item.MemberName);
                }
                else if (days > 1 && days < 5)
                {
                    _FewDays.Add(item.MemberName);
                }
                else if (days > 4 && days < 7)
                {
                    _ThisWeek.Add(item.MemberName);
                }
                else
                {
                    if (item.ProfileId == string.Empty)
                        _MoreOneWeek.Add($"{item.MemberName}(**Профиль скрыт**)");
                    else
                        _MoreOneWeek.Add($"[{item.MemberName}](https://www.bungie.net/ru/Profile/254/{item.ProfileId}/)");

                }
            }

            string ThisDay = string.Join(',', _ThisDay);
            if (!string.IsNullOrEmpty(ThisDay))
                embed.AddField("Был сегодня", ThisDay);

            string Yesterday = string.Join(',', _Yesterday);
            if (!string.IsNullOrEmpty(Yesterday))
                embed.AddField("Был вчера", Yesterday);

            string FewDays = string.Join(',', _FewDays);
            if (!string.IsNullOrEmpty(FewDays))
                embed.AddField("Был в течении пары дней", FewDays);

            string ThisWeek = string.Join(',', _ThisWeek);
            if (!string.IsNullOrEmpty(ThisWeek))
                embed.AddField("Был на этой неделе", ThisWeek);

            string MoreOneWeek = string.Join(',', _MoreOneWeek);
            if (!string.IsNullOrEmpty(MoreOneWeek))
                embed.AddField("Был больше недели тому назад", MoreOneWeek);

            embed.WithFooter($"Данные о клане {GuildInfo.Detail.Name}");
            await Context.Channel.SendMessageAsync(embed: embed.Build());

        }
    }
}
