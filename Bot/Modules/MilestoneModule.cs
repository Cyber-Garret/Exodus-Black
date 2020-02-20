using Bot.Models;
using Bot.Services;
using Bot.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Discord;

namespace Bot.Modules
{
	public class MilestoneModule : BaseModule
	{
		private readonly ILogger logger;
		private readonly MilestoneService milestoneHandler;
		private readonly EmoteService emote;

		public MilestoneModule(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<MilestoneModule>>();
			milestoneHandler = service.GetRequiredService<MilestoneService>();
			emote = service.GetRequiredService<EmoteService>();
		}

		[Command("рейд")]
		[Summary("Команда для анонса сбора в рейд.")]
		[Remarks("Пример: !рейд <Название> <Дата> <Заметка лидера(Не обязательно)>, например !рейд сс 20.02.20:00 Тестовая заметка.\nВведите любой параметр команды неверно, и я отображу по нему справку.")]
		public async Task RegisterRaid(string raidName, string raidTime, [Remainder]string leaderNote = null)
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				var milestoneInfo = MilestoneInfoData.SearchMilestoneData(raidName, MilestoneType.Raid);

				if (milestoneInfo == null)
				{
					await ReplyAndDeleteAsync("Страж, я не разобрала в какой рейд ты хочешь собрать боевую группу.");
					return;
				}

				string[] formats = { "dd.MM-HH:mm:z", "dd,MM-HH,mm:z", "dd.MM.HH.mm:z", "dd,MM,HH,mm:z" };

				DateTimeOffset.TryParseExact($"{raidTime}:{guild.TimeZone}", formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffset);
				DateTime.TryParseExact($"{raidTime}:{guild.TimeZone}", formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

				if (dateTime == new DateTime())
				{
					await ReplyAndDeleteAsync("Страж, ты указал неизвестный мне формат времени.");
					return;
				}
				if (dateTime < DateTime.Now)
				{

					await ReplyAndDeleteAsync("Собрался в прошлое? Тебя ждет увлекательное шоу \"остаться в живых\" в исполнении моей команды Золотого Века. Не забудь попкорн\nБип...Удачи и передай привет моему капитану.");
					return;
				}

				var newMilestone = new Milestone
				{
					GuildId = Context.Guild.Id,
					MilestoneInfo = milestoneInfo,
					Note = leaderNote,
					Leader = Context.User.Id,
					DateExpire = dateTime
				};
				var embed = milestoneHandler.MilestoneEmbed(newMilestone);

				var msg = await ReplyAsync(message: guild.GlobalMention, embed: embed);

				newMilestone.MessageId = msg.Id;

				ActiveMilestoneData.AddMilestone(newMilestone);

				//Slots
				await msg.AddReactionAsync(emote.Raid);
			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync("Страж, произошла критическая ошибка, я не могу в данный момент выполнить команду.\nУже пишу моему создателю, он сейчас все поправит.");
				logger.LogError(ex, "Raid command");
			}
		}
		// TODO: Strike command
		[Command("налёт")]
		[Summary("Команда для анонса сбора в сумрачный налёт.")]
		public async Task RegisterStrike(string strikeName, string strikeTime, [Remainder]string leaderNote = null)
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				var milestoneInfo = MilestoneInfoData.SearchMilestoneData(strikeName, MilestoneType.Strike);

				if (milestoneInfo == null)
				{
					await ReplyAndDeleteAsync("Страж, я не разобрала в какой сумрачный налёт ты хочешь собрать боевую группу.");
					return;
				}

				string[] formats = { "dd.MM-HH:mm", "dd,MM-HH,mm", "dd.MM.HH.mm", "dd,MM,HH,mm" };

				DateTime.TryParseExact(strikeTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

				if (dateTime == new DateTime())
				{
					await ReplyAndDeleteAsync("Страж, ты указал неизвестный мне формат времени.");
					return;
				}
				if (dateTime < DateTime.Now)
				{

					await ReplyAndDeleteAsync("Собрался в прошлое? Тебя ждет увлекательное шоу \"остаться в живых\" в исполнении моей команды Золотого Века. Не забудь попкорн\nБип...Удачи и передай привет моему капитану.");
					return;
				}

				var newMilestone = new Milestone
				{
					GuildId = Context.Guild.Id,
					MilestoneInfo = milestoneInfo,
					Note = leaderNote,
					Leader = Context.User.Id,
					DateExpire = dateTime
				};
				var embed = milestoneHandler.MilestoneEmbed(newMilestone);

				var msg = await ReplyAsync(message: guild.GlobalMention, embed: embed);

				newMilestone.MessageId = msg.Id;

				ActiveMilestoneData.AddMilestone(newMilestone);

				//Slots
				await msg.AddReactionAsync(emote.Raid);
			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync("Страж, произошла критическая ошибка, я не могу в данный момент выполнить команду.\nУже пишу моему создателю, он сейчас все поправит.");
				logger.LogError(ex, "Strike command");
			}
		}
		// TODO: Other command
		//[Command("активность")]
	}
}
