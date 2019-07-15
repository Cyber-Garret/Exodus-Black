using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DiscordBot.Preconditions;
using DiscordBot.Features.Raid;
using System.Globalization;

namespace DiscordBot.Modules
{
	[Cooldown(10)]
	[Group("рейд")]
	public class RaidModule : BotModuleBase
	{
		[Command("сбор")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.EmbedLinks | ChannelPermission.ManageMessages | ChannelPermission.MentionEveryone)]
		public async Task RaidCollection(string raidName, string raidTime, [Remainder]string userMemo = "Рейд-лидер не указал какие-либо особенности или требования.")
		{
			var raidInfo = FailsafeDbOperations.GetRaidInfo(raidName);


			if (raidInfo == null)
			{
				var message = new EmbedBuilder()
					.WithTitle("Страж, я не разобрала в какой рейд ты хочешь пойти")
					.WithColor(Color.Red)
					.AddField(Global.InvisibleString,
					"Известные мне рейды:\n" +
					"**Левиафан**, или просто **левик**.\n" +
					"**Пожиратель миров** или просто **пм**.\n" +
					"**Звездный шпиль** или просто **зш**.\n" +
					"**Последнее желание** или просто **пж**.\n" +
					"**Истребители прошлого** или просто **ип**.\n" +
					"**Корона скорби** или просто **кс**.")
					.WithFooter("Хочу напомнить, что я ищу как по полному названию рейда так и частичному.");
				await ReplyAsync(embed: message.Build());
				return;
			}
			DateTime.TryParseExact(raidTime, "d.M.yy-HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

			if (dateTime == new DateTime() || dateTime < DateTime.Now)
			{
				var message = new EmbedBuilder()
					.WithTitle("Страж, ты указал неизвестный мне формат времени")
					.WithColor(Color.Red)
					.AddField("Я понимаю время начала рейда в таком формате",
					"Формат времени: **<день>.<месяц>.<год>-<час>:<минута>**\n" +
					"**День:** от 1 до 31\n" +
					"**Месяц:** от 1 до 12\n" +
					"**Год:** последние две цифры, например: 19\n" +
					"**Час:** от 00 до 23\n" +
					"**Минута:** от 01 до 59\n" +
					"В итоге у тебя должно получиться: **20.7.19-20:05**")
					.WithFooter("Пример: !рейд сбор пж 21.5.18-20:00");
				await ReplyAsync(embed: message.Build());
				return;
			}

			//if (string.IsNullOrWhiteSpace(userMemo))
			//	userMemo = "**Рейд-лидер не указал какие-либо особенности или требования.**";

			var msg = await Context.Channel.SendMessageAsync(text: "@everyone", embed: RaidsCore.StartRaidEmbed(Context.User, raidInfo, dateTime, userMemo).Build());
			await RaidsCore.RegisterRaidAsync(msg.Id, Context.Guild.Name, Context.User.Id, raidInfo.Id, dateTime, userMemo);
			//Slots
			await msg.AddReactionAsync(RaidsCore.ReactOptions["2"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["3"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["4"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["5"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["6"]);

		}
	}
}
