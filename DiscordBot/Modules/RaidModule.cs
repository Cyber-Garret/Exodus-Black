using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DiscordBot.Preconditions;
using DiscordBot.Features.Raid;
using System.Globalization;

namespace DiscordBot.Modules
{
	public class RaidModule : BotModuleBase
	{
		[Command("сбор")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.EmbedLinks | ChannelPermission.ManageMessages | ChannelPermission.MentionEveryone)]
		[Cooldown(10)]
		public async Task RaidCollection(string raidName, string raidTime, [Remainder]string userMemo = "Лидер не указал какие-либо особенности или требования.")
		{
			var raidInfo = await FailsafeDbOperations.GetMilestone(raidName);

			if (raidInfo == null)
			{
				var AvailableRaids = "Доступные для регистрации активности:\n\n";
				var info = await FailsafeDbOperations.GetAllMilestones();

				foreach (var item in info)
				{
					AvailableRaids += $"**{item.Name}** или просто **{item.Alias}**\n";
				}

				var message = new EmbedBuilder()
					.WithTitle("Страж, я не разобрала в какую активность ты хочешь пойти")
					.WithColor(Color.Red)
					.WithDescription(AvailableRaids += "\nПример: !сбор пж 17.07.2019-20:00")
					.WithFooter("Хочу напомнить, что я ищу как по полному названию рейда так и частичному.");
				await ReplyAsync(embed: message.Build());
				return;
			}

			DateTime.TryParseExact(raidTime, "dd.MM.yyyy-HH:mm", CultureInfo.InstalledUICulture, DateTimeStyles.None, out DateTime dateTime);

			if (dateTime == new DateTime())
			{
				var message = new EmbedBuilder()
					.WithTitle("Страж, ты указал неизвестный мне формат времени")
					.WithColor(Color.Gold)
					.AddField("Я понимаю время начала рейда в таком формате",
					"Формат времени: **<день>.<месяц>.<год>-<час>:<минута>**\n" +
					"**День:** от 01 до 31\n" +
					"**Месяц:** от 01 до 12\n" +
					"**Год:** Например: 2019\n" +
					"**Час:** от 00 до 23\n" +
					"**Минута:** от 00 до 59\n" +
					"В итоге у тебя должно получиться: **05.07.2019-20:05**")
					.AddField("Уведомление", "Время начала активности учитывается только по московскому времени. Также за 15 минут до начала активности, я уведомлю участников личным сообщением.")
					.WithFooter("Пример: !сбор пж 21.05.2018-20:00");
				await ReplyAsync(embed: message.Build());
				return;
			}
			if (dateTime < DateTime.Now)
			{
				var message = new EmbedBuilder()
					.WithColor(Color.Red)
					.WithDescription($"Собрался в прошлое? Тебя ждет увлекательное шоу \"остаться в живых\" в исполнении моей команды Золотого Века. Не забудь попкорн\nБип...Удачи в {DateTime.Now.Year - 1000} г. и передай привет моему капитану.");
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
