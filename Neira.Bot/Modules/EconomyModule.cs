using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Neira.Bot.Helpers;
using Neira.Bot.Preconditions;
using Neira.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neira.Bot.Modules
{
	public class EconomyModule : BotModuleBase
	{
		private readonly DiscordSocketClient Client;
		private readonly EmoteService emote;
		private readonly DbService db;
		public EconomyModule(DiscordSocketClient socketClient, EmoteService emoteService, DbService dbService)
		{
			Client = socketClient;
			db = dbService;
			emote = emoteService;
		}

		[Command("награда")]
		[Summary("Ежедневная порция блеска, бесплатно!")]
		[Cooldown(30)]
		public async Task GetDaily()
		{
			var result = await db.GetDailyAsync(Context.User.Id);

			if (result.Success)
			{
				var embed = new EmbedBuilder
				{
					Color = Color.Gold,
					Description = $"{emote.Glimmer}  | Поздравляю страж {Context.User.Username}, ты получил **{Global.DailyGlimmerGain}** блеска! Приходи завтра, чтобы получить еще!"
				};

				await ReplyAsync(embed: embed.Build());
			}
			else
			{
				var embed = new EmbedBuilder
				{
					Color = Color.Red,
					Description = $"{emote.Glimmer}:clock1:  | **Страж {Context.User.Username}, ты уже получил свою бесплатную порцию блеска!\nВозвращайся через {result.RefreshTimeSpan:%h} ч. {result.RefreshTimeSpan:%m} мин.**"
				};
				await ReplyAsync(embed: embed.Build());
			}
		}

		[Command("респект")]
		[Summary("Увеличивает репутацию указанного стража. ")]
		[Remarks("Пример: **!респект @Cyber_Garret**")]
		[Cooldown(30), RequireContext(ContextType.Guild, ErrorMessage = Global.NotInGuildText)]
		public async Task GetRep([NoSelf]SocketGuildUser recipient)
		{
			//Get user can add reputation?
			var result = await db.GetRepAsync((SocketGuildUser)Context.User);

			if (result.Success)
			{
				//Add Reputation to mentioned user
				await db.AddRepAsync(recipient);

				var embed = new EmbedBuilder
				{
					Color = Color.Gold,
					Description = $":diamond_shape_with_a_dot_inside: | Внимание страж {recipient.Mention}! Твоя репутация была повышена стражем {Context.User.Username}!"
				};

				await ReplyAsync(embed: embed.Build());
			}
			else
			{
				var embed = new EmbedBuilder
				{
					Color = Color.Red,
					Description = $":diamond_shape_with_a_dot_inside::clock1:  | **Страж {Context.User.Username}, ты уже кому-то повышал репутацию.\nТы сможешь кому-то поднять репутацию через {result.RefreshTimeSpan:%h} ч. {result.RefreshTimeSpan:%m} мин.**"
				};
				await ReplyAsync(embed: embed.Build());
			}

		}

		[Command("подарить"), Alias("дать")]
		[Summary("Дарит блеск указанному стражу (само собой сумма списывается с твоего счета). ")]
		[Remarks("!дать <количество> <Страж которому ты хочешь сделать подарок> Пример: **!дать 500 @Cyber_Garret**")]
		[Cooldown(30), RequireContext(ContextType.Guild, ErrorMessage = Global.NotInGuildText)]
		public async Task Gift(uint Ammount = 0, [NoSelf]IGuildUser recipient = null)
		{
			var config = await db.GetGuildAccountAsync(Context.Guild.Id);

			if (Ammount == 0)
			{
				var message = $"Страж, ты не указал сколько {emote.Glimmer} ты хочешь подарить.\n" +
					$"Напомню что команда должна быть выполнена в таком формате **{config.CommandPrefix ?? "!"}дать 10 @Cyber_Garret**";

				await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Red, message));
				return;
			}
			else if (recipient == null)
			{
				var message = $"Страж, ты не указал кому ты хочешь подарить {emote.Glimmer}\n" +
					$"Напомню что команда должна быть выполнена в таком формате **{config.CommandPrefix ?? "!"}дать 10 @Cyber_Garret**";

				await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Red, message));
				return;
			}

			var senderAccount = await db.GetUserAccountAsync(Context.User);

			if (senderAccount.Glimmer < Ammount)
			{
				var message = $":angry:  | Страж, ты не можешь подарить сумму {emote.Glimmer}, которой у тебя нет на счету!";
				await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Red, message));
			}
			else
			{
				var recipientAccount = await db.GetUserAccountAsync(recipient);

				senderAccount.Glimmer -= Ammount;
				recipientAccount.Glimmer += Ammount;

				await db.SaveUserAccountAsync(senderAccount);

				await db.SaveUserAccountAsync(recipientAccount);

				var message = $":clap: | {Context.User.Username} подарил стражу {recipient.Mention} {Ammount} {emote.Glimmer}\nКак благородно!";
				await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Gold, message));

			}
		}

		[Command("топ"), Alias("RichBitch", "богачи")]
		[Summary("Отображает список стражей, отсортированных по количеству блеска. Есть возможность указывать страницу чтобы увидеть стражей с более низким количеством блеска. ")]
		[Remarks("!топ <номер страницы (если оставить пустым, по умолчанию будет первая страница)> пример: **!топ**\\**!топ 2**")]
		[Cooldown(10)]
		public async Task ShowRichesPeople(int page = 1)
		{
			if (page < 1)
			{
				var message = "Tы действительно этого хочешь? ***ТОЧНО?***";
				await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Red, message));
				return;
			}
			var guildUserIds = Context.Guild.Users.Select(user => user.Id);
			var accounts = db.GetFilteredAccounts(acc => guildUserIds.Contains(acc.Id));

			const int usersPerPage = 9;
			// Calculate the highest accepted page number => amount of pages we need to be able to fit all users in them
			// (amount of users) / (how many to show per page + 1) results in +1 page more every time we exceed our usersPerPage  
			var lastPageNumber = 1 + (accounts.Count / (usersPerPage + 1));
			if (page > lastPageNumber)
			{
				var message = $"Такой страницы не существует. Последняя страница имеет номер **{lastPageNumber}**";
				await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Red, message));
				return;
			}
			// Sort the accounts descending by currency
			var ordered = accounts.OrderByDescending(acc => acc.Glimmer).ToList();

			var embed = new EmbedBuilder()
				.WithTitle($"{emote.Glimmer} Рейтинг богатых стражей на этом корабле:")
				.WithFooter($"Страница {page}/{lastPageNumber}");

			page--;
			for (var i = 1; i <= usersPerPage && i + usersPerPage * page <= ordered.Count; i++)
			{
				var account = ordered[i - 1 + usersPerPage * page];
				var user = Client.GetUser(account.Id);
				embed.WithColor(Color.Gold);
				embed.AddField($"#{i + usersPerPage * page} {user.Username}", $"{account.Glimmer} {emote.Glimmer}", true);
			}

			await ReplyAsync(embed: embed.Build());
		}

		[Command("блеск")]
		[Summary("Проверяет твой баланс блеска или указанного стража.")]
		[Remarks("!баланс <страж которого ты хочешь проверить (Если никого не указать по умолчанию отобразит твой баланс)> Пример: !баланс @Cyber_Garret")]
		[Cooldown(10)]
		public async Task CheckGlimmerBalance(SocketUser mentionedUser = null)
		{
			SocketUser target = mentionedUser ?? Context.User;

			var account = await db.GetUserAccountAsync(target);
			var message = $"Баланс: **{account.Glimmer} {emote.Glimmer}**\n{GetGlimmerCountReaction(account.Glimmer, target.Username)}";
			await ReplyAsync(embed: BuildedEmbeds.BaseGlimmerEmbed(Color.Gold, message, $"Капитал стража {target.Username}"));
		}

		[Command("стата")]
		[Summary("Отображение твоей статистике на сервере (Уровень, опыт, репутация)")]
		[Remarks("!стата <страж которого ты хочешь проверить (Если никого не указать по умолчанию отобразит твою стату)> Пример: !стата @Cyber_Garret")]
		[Cooldown(10)]
		public async Task Stats(SocketUser mentionedUser = null)
		{
			var target = mentionedUser ?? Context.User;

			var userAccount = await db.GetGuildUserAccountAsync((SocketGuildUser)target);
			var requiredXp = (Math.Pow(userAccount.LevelNumber + 1, 2) * 50);

			var auth = new EmbedAuthorBuilder()
			{
				Name = $"Статистика стража {target.Username} на сервере {Context.Guild.Name}",
				IconUrl = target.GetAvatarUrl() ?? target.GetDefaultAvatarUrl(),
			};

			var embed = new EmbedBuilder()
			{
				Author = auth,
				Color = Color.Blue
			};

			embed.AddField("Ур.", userAccount.LevelNumber, true);
			embed.AddField("Опыт", $"{userAccount.XP}/{requiredXp} (Всего {userAccount.XP})", true);
			embed.AddField("Репутация", userAccount.Reputation, true);
			embed.AddField("Предупреждения", 0, true);

			await ReplyAsync(embed: embed.Build());
		}

		[Command("баланс")]
		[Summary("Отображение твоей глобальной статистики (Уровень, опыт)")]
		[Remarks("!баланс <страж которого ты хочешь проверить (Если никого не указать по умолчанию отобразит твой аккаунт)> Пример: !баланс @Cyber_Garret")]
		[Cooldown(10)]
		public async Task WsashiStats(SocketUser mentionedUser = null)
		{
			var target = mentionedUser ?? Context.User;

			var userAccount = await db.GetUserAccountAsync(target);
			var requiredXp = (Math.Pow(userAccount.LevelNumber + 1, 2) * 50);

			var auth = new EmbedAuthorBuilder()
			{
				Name = $"Глобальная статистика стража {target.Username}",
				IconUrl = target.GetAvatarUrl() ?? target.GetDefaultAvatarUrl(),
			};

			var embed = new EmbedBuilder()
			{
				Author = auth,
				Color = Color.Gold
			};

			embed.AddField("Ур.", userAccount.LevelNumber, true);
			embed.AddField("Опыт", $"{userAccount.XP}/{requiredXp} (Всего {userAccount.XP})", true);
			embed.AddField("Блеск", $"{userAccount.Glimmer}{emote.Glimmer}", true);

			await ReplyAsync(embed: embed.Build());
		}

		private string GetGlimmerCountReaction(ulong ammount, string username)
		{
			if (ammount > 100000)
			{
				return $"~~Аху~~ Невероятно, **{username}**! Или этот страж чёртов хакер или он действительно особенный.";
			}
			else if (ammount > 50000)
			{
				return $"Обнаружена попытка взлома! А нет, это просто **{username}** соскучился. Ну привет-привет.";
			}
			else if (ammount > 20000)
			{
				return $"Этого достаточно чтобы купить дом... на кентавре Несс.\n\nЭто реальное место, тихо **{username}**!";
			}
			else if (ammount > 10000)
			{
				return $"Ох, кто же это тут у нас? Да это же воришка **{username}**. Как иначе он смог собрать такую сумму?";
			}
			else if (ammount > 5000)
			{
				return $"Или мне кажется или **{username}** делает свой бюджет немного более серьезным? ";
			}
			else if (ammount > 2500)
			{
				return $"Отлично, **{username}** теперь может отдать весь этот {emote.Glimmer} своей величественной госпоже. МНЕ! ";
			}
			else if (ammount > 1100)
			{
				return $"**{username}** снова хвастается своим кошельком в башне.";
			}
			else if (ammount > 800)
			{
				return $"Океей, **{username}**. Положи {emote.Glimmer} в мешочек и никто не пострадает.";
			}
			else if (ammount > 550)
			{
				return $"Мне нравится, что **{username}** думает, что это может впечатлить.";
			}
			else if (ammount > 200)
			{
				return $"Ой, **{username}** если бы я знала что это все что есть, я бы писала только в личку! Позор!";
			}
			else if (ammount == 0)
			{
				return $"Ага, страж **{username}** банкрот. Как неожиданно!";
			}

			return $"Вся концепция {emote.Glimmer} просто выдумка. Я надеюсь, ты знаешь об этом.";
		}
	}
}
