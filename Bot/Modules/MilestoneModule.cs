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
using Discord.WebSocket;

namespace Bot.Modules
{
	public class MilestoneModule : BaseModule
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly MilestoneService milestoneHandler;
		private readonly EmoteService emote;

		public MilestoneModule(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<MilestoneModule>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			milestoneHandler = service.GetRequiredService<MilestoneService>();
			emote = service.GetRequiredService<EmoteService>();
		}

		#region Commands
		[Command("raid"), Alias("рейд", "рєйд")]
		[Summary("Анонс сбора боевой группы в рейд.")]
		public async Task RegisterRaid(string raidName, string raidTime, [Remainder]string leaderNote = null)
		{
			await GoMilestoneAsync(raidName, MilestoneType.Raid, raidTime, leaderNote);
		}

		[Command("strike"), Alias("налёт", "наліт")]
		[Summary("Aнонс сбора боевой группы в сумрачный налёт.")]
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

		//TODO: Ordeal nightfall
		//[Command("побоище")]
		//[Summary("Анонс сбора боевой группы в активный сумрачный налет: побоище")]
		//public async Task RegisterOrdeal(string strikeTime,[Remainder]string leaderNote = null)
		//{

		//}

		[Command("milestone"), Alias("сбор", "збір")]
		[Summary("Анонс сбора боевой группы в активности типа Паноптикум, Яма, Трон и тд и тп.")]
		public async Task RegisterOther(string otherName, string otherTime, [Remainder]string leaderNote = null)
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				var milestoneInfo = MilestoneInfoData.SearchMilestoneData(otherName, MilestoneType.Other);

				if (milestoneInfo == null)
				{
					await ReplyAndDeleteAsync("Страж, я не разобрала в какую активность ты хочешь собрать боевую группу.");
					return;
				}

				string[] formats = { "dd.MM-HH:mm", "dd,MM-HH,mm", "dd.MM.HH.mm", "dd,MM,HH,mm" };

				DateTime.TryParseExact(otherTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

				var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
				var raidTimeOffset = new DateTimeOffset(dateTime, guildTimeZone.BaseUtcOffset);

				if (dateTime == new DateTime())
				{
					await ReplyAndDeleteAsync("Страж, ты указал неизвестный мне формат времени.");
					return;
				}

				var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guildTimeZone);

				if (raidTimeOffset < now)
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
					DateExpire = raidTimeOffset
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
				logger.LogError(ex, "Other milestone command");
			}
		}

		// TODO: User defined milestone

		[Command("note"), Alias("заметка", "замітка")]
		[Summary("Позволяет удалить или изменить заметку активности.")]
		public async Task ChangeNote(ulong milestoneId, [Remainder]string note = null)
		{
			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);
			if (milestone.Leader == Context.User.Id)
			{
				milestone.Note = note;
				ActiveMilestoneData.SaveMilestones(milestone.MessageId);

				var channel = (ISocketMessageChannel)Context.Guild.GetChannel(milestone.ChannelId);
				var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);

				await msg.ModifyAsync(m => m.Embed = milestoneHandler.MilestoneEmbed(milestone));

				await ReplyAndDeleteAsync($"Заметка исправлена. {msg.GetJumpUrl()}");
			}
			else
				await ReplyAndDeleteAsync("Ты не лидер активности.");
		}

		[Command("cancel"), Alias("отмена", "скасувати")]
		[Summary("Позволяет отменить активность.")]
		public async Task CloseMilestone(ulong milestoneId, [Remainder]string reason = null)
		{
			var milestone = ActiveMilestoneData.GetMilestone(milestoneId);
			if (milestone.Leader == Context.User.Id)
			{
				if (reason == null)
				{
					await ReplyAndDeleteAsync("для отмены нужно написать причину.");
					return;
				}
				ActiveMilestoneData.RemoveMilestone(milestone.MessageId);

				var channel = (ISocketMessageChannel)Context.Guild.GetChannel(milestone.ChannelId);
				var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);

				await msg.ModifyAsync(m =>
				{
					m.Content = string.Empty;
					m.Embed = DeleteMilestone(milestone, reason);
				});

				await ReplyAndDeleteAsync($"Активность отменена. {msg.GetJumpUrl()}");
			}
			else
				await ReplyAndDeleteAsync("Ты не лидер активности.");
		}
		#endregion

		#region Methods
		private async Task GoMilestoneAsync(string searchName, MilestoneType type, string time, string Note = null)
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				var milestoneInfo = MilestoneInfoData.SearchMilestoneData(searchName, type);
				Embed embed;
				if (milestoneInfo == null)
				{
					embed = milestoneHandler.GetMilestonesNameEmbed(type);
					await ReplyAndDeleteAsync($"{Context.User.Mention}, я не разобрала в какую активность ты хочешь собрать боевую группу.", embed: embed, timeout: TimeSpan.FromMinutes(1));
					return;
				}

				string[] formats = { "dd.MM-HH:mm", "dd,MM-HH,mm", "dd.MM.HH.mm", "dd,MM,HH,mm" };

				var IsSucess = DateTime.TryParseExact(time, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

				if (IsSucess)
				{
					var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
					var raidTimeOffset = new DateTimeOffset(dateTime, guildTimeZone.BaseUtcOffset);

					var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guildTimeZone);

					if (raidTimeOffset < now)
					{

						await ReplyAndDeleteAsync("Собрался в прошлое? Тебя ждет увлекательное шоу \"остаться в живых\" в исполнении моей команды Золотого Века. Не забудь попкорн\nБип...Удачи и передай привет моему капитану.");
						return;
					}

					var msg = await ReplyAsync("Подготавливаю сбор боевой группы.");

					var newMilestone = new Milestone
					{
						MessageId = msg.Id,
						ChannelId = Context.Channel.Id,
						GuildId = Context.Guild.Id,
						MilestoneInfo = milestoneInfo,
						Note = Note,
						Leader = Context.User.Id,
						DateExpire = raidTimeOffset
					};

					ActiveMilestoneData.AddMilestone(newMilestone);

					embed = milestoneHandler.MilestoneEmbed(newMilestone);

					await msg.ModifyAsync(a =>
					{
						a.Content = guild.GlobalMention;
						a.Embed = embed;
					});
					//Slots
					await msg.AddReactionAsync(emote.Raid);
				}
				else
					await ReplyAndDeleteAsync("Страж, ты указал неизвестный мне формат времени.");

			}
			catch (Exception ex)
			{
				await ReplyAndDeleteAsync("Страж, произошла критическая ошибка, я не могу в данный момент выполнить команду.\nУже пишу моему создателю, он сейчас все поправит.");
				logger.LogError(ex, "Raid command");
			}
		}


		private Embed DeleteMilestone(Milestone milestone, string reason)
		{
			var embed = new EmbedBuilder
			{
				Title = $"{milestone.MilestoneInfo.Type }: { milestone.MilestoneInfo.Name}",
				Description = $"Отменен по причине: {reason}"
			};
			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = $"Состав боевой группы"
			};
			var leader = discord.GetUser(milestone.Leader);
			embedFieldUsers.Value = $"#1 {leader.Mention} - {leader.Username}\n";
			if (milestone.MilestoneUsers.Count > 0)
			{
				int count = 2;
				foreach (var user in milestone.MilestoneUsers)
				{

					var discordUser = discord.GetUser(user);
					embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";

					count++;
				}
			}
			embed.AddField(embedFieldUsers);

			return embed.Build();
		}


		#endregion
	}
}
