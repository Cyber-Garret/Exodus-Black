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
            //Send calculating message because stastic forming near 30-50 sec.
            var message = await Context.Channel.SendMessageAsync("Начинаю проводить подсчет.");

            //Initialize Bungie GroupV2
            DestinyClanMemberWeekStat weekStat = new DestinyClanMemberWeekStat(GuildId);
            //Get GroupV2 main info
            var GuildInfo = weekStat.GuildInfoAsync(GuildId).Result.Response;
            //Get GroupV2 Member info
            var GuildMembers = await weekStat.GetGuildCharacterIdsAsync();

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle($"Онлайн статус стражей");
            embed.WithColor(Color.Orange);
            //Bungie Clan link
            embed.WithUrl($"https://www.bungie.net/ru/ClanV2?groupId={GuildInfo.Detail.GroupId}");
            //Some clan main info
            embed.WithDescription($"Клан зарегистрирован **{GuildInfo.Detail.CreationDate.ToString("dd-MM-yyyy")}**\n" +
                $"В данный момент в клане **{GuildInfo.Detail.MemberCount}**/100 стражей.\n" +
                $"Девиз клана - **{GuildInfo.Detail.Motto}**");

            #region list for member sorted for some days
            List<string> _ThisDay = new List<string>();
            List<string> _Yesterday = new List<string>();
            List<string> _FewDays = new List<string>();
            List<string> _ThisWeek = new List<string>();
            List<string> _MoreOneWeek = new List<string>();
            #endregion

            //Main Sorting logic
            foreach (var item in GuildMembers)
            {
                //Property for calculate how long days user did not enter the Destiny
                var days = (DateTime.Today.Day - item.LastOnlineDate.Day);

                //Sorting user to right list
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
                    //ProfileId is empty if user hide his profile from public
                    if (item.ProfileId == string.Empty)
                        _MoreOneWeek.Add($"{item.MemberName}(**Профиль скрыт**)");
                    else
                        _MoreOneWeek.Add($"[{item.MemberName}](https://www.bungie.net/ru/Profile/254/{item.ProfileId}/)");

                }
            }

            //Create one string who enter to the game today, like "Petya,Vasia,Grisha",
            //and if string ThisDay not empty add to embed message special field.
            string ThisDay = string.Join(',', _ThisDay);
            if (!string.IsNullOrEmpty(ThisDay))
                embed.AddField("Был сегодня", ThisDay);
            //Same as above, but who enter to the game yesterday
            string Yesterday = string.Join(',', _Yesterday);
            if (!string.IsNullOrEmpty(Yesterday))
                embed.AddField("Был вчера", Yesterday);
            //Same as above, but who enter to the game more 2 days but fewer 4 days ago
            string FewDays = string.Join(',', _FewDays);
            if (!string.IsNullOrEmpty(FewDays))
                embed.AddField("Был в течении пары дней", FewDays);
            //Same as above, but who enter to the game more 5 days but fewer 7 days ago
            string ThisWeek = string.Join(',', _ThisWeek);
            if (!string.IsNullOrEmpty(ThisWeek))
                embed.AddField("Был на этой неделе", ThisWeek);
            //Same as above, but who enter to the game more 7 days ago
            string MoreOneWeek = string.Join(',', _MoreOneWeek);
            if (!string.IsNullOrEmpty(MoreOneWeek))
                embed.AddField("Был больше недели тому назад", MoreOneWeek);
            //Simple footer with clan name
            embed.WithFooter($"Данные о клане {GuildInfo.Detail.Name}");
            //Mention user with ready statistic
            await Context.Channel.SendMessageAsync($"Бип! {Context.User.Mention}. Статистика подсчитана.", false, embed.Build());

            //Delete message from start this command
            await Context.Channel.DeleteMessageAsync(message);

        }
    }
}
