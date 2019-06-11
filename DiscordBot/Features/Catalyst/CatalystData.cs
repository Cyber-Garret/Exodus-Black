using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Core.Models.Db;
using Core;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Catalyst
{
	internal static class CatalystData
	{
		/// Сопостовляет простые строки с Discord реакциями для более удобного использования.
		public static readonly Dictionary<string, Emoji> ReactOptions;

		static CatalystData()
		{
			ReactOptions = new Dictionary<string, Emoji>
			{
				{"ok", new Emoji("🆗")}, { "right", new Emoji("➡") }, {"left", new Emoji("⬅")}
			};
			Categories = GetCategories();
		}

		/// <summary>
		/// Проверяет, является ли представленная реакция связанна с запущенной игрой,
		/// и является ли тот, кто нажал эту реакцию, тем же, кто создал эту игру, - если это так, инициирует обработку игровой механики викторины.
		/// </summary>
		internal static async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			// Запускается только в том случае если нашли сообщение в котором 
			// пользователь который добавил реакцию соответсвует пользователю который начал викторину.
			var catalystMessage = Global.CatalystMessages.FirstOrDefault(message =>
			message.GameMessageId == reaction.MessageId && message.PlayerId == reaction.UserId);
			if (catalystMessage != null)
			{
				var msg = await cache.GetOrDownloadAsync();
				// Моментально удаляет реакции чтобы пользователь мог дальше использовать их как входные данные.
				// Для начала проверяем совпадает ли ID пользователя с ID автора сообщения.
				if (reaction.UserId != msg.Author.Id)
				{
					var user = reaction.User.GetValueOrDefault(null) ?? Program.Client.GetUser(reaction.UserId);
					try
					{
						await msg.RemoveReactionAsync(reaction.Emote, user);
					}
					catch (Exception e)

					{
						await Logger.Log(new LogMessage(LogSeverity.Error, $"Дискорд | Нет прав удалять реакции в канале {msg.Channel}", e.Message, e.InnerException));
					}
				}
				await catalystMessage.HandleReaction(msg, reaction);
			}
		}

		/// <summary>
		/// Отправяет запрос в https://opentdb.com/api.php с параметрами. (Все параметры опциональны).
		/// </summary>
		/// <param name="count">Количество вопросов для получения | По умолчанию 1</param>
		/// <param name="categoryId">Любая строка, либо строка id_string категории | По умолчанию любая категория</param>
		/// <param name="difficulty">Может быть легко, средне, сложно или любая сложность | По умолчанию любая сложность</param>
		/// <param name="type">Тип вопросов может быть варианты ответов, или да\нет | По умолчанию любой тип вопросов</param>
		/// <param name="token">Токен используеться для того чтобы в рамках одной сесии не получать один и тот же вопрос | По умочланию пустая строка означает без использования токена</param>
		/// <returns> Возвращает список вопросов или пустой список если реквест не был успешным.</returns>
		internal static async Task<List<Core.Models.Db.Catalyst>> GetCatalysts(int categoryId = 0)
		{
			using (var Db = new FailsafeContext())
			{
				if (categoryId == 0)
					return await Db.Catalysts.Include(c => c.Category).AsNoTracking().ToListAsync();
				else
				{
					return await Db.Catalysts.Include(c => c.Category.Id == categoryId).AsNoTracking().ToListAsync();
				}
			}

		}

		/// <summary>
		/// Возвращает EmbedBuilder со всей информацией для отображения стартового меню
		/// </summary>
		/// <param name="message">Опционально | Если представлено то принимает такие параметры TriviaGame как (Сложность, Тип вопроса, Категорию)</param>
		internal static EmbedBuilder CatalystStartingEmbed(CatalystCore message = null)
		{
			return new EmbedBuilder()
				.WithAuthor("Добро пожаловать в базу данных катализаторов экзотического оружия Нейроматрицы")
				.WithDescription("Что будем делать?")
				.WithColor(Color.Blue)
				.WithFooter("Используй реакции ниже, чтобы произвести выбор. \n(Только тот, кто вызвал данную команду, сможет использовать их. Остальных я игнорирую.)")
				.AddField(Global.InvisibleString, ReactOptions["ok"] + " **Начинаем**");
		}

		/// <summary>
		/// Устанавливает всю нужную информацию для отображения вопроса как embed сообщение
		/// </summary>
		/// <param name="q">Вопрос для отображения</param>
		/// <param name="emb">EmbedBuilder который наследует такие свойства как (Заголовок, Автор, Футер)</param>
		internal static EmbedBuilder CatalystToEmbed(Core.Models.Db.Catalyst catalyst, EmbedBuilder emb)
		{
			// Наследование информации из представленогого embed сообщения
			var embB = new EmbedBuilder()
				.WithTitle(emb.Title)
				.WithAuthor(emb.Author)
				.WithFooter(emb.Footer);
			// Устанавливает Цвет и Описание embed сообщения
			embB.WithColor(emb.Color.GetValueOrDefault(Color.Gold))
				.WithDescription($"[{catalyst.Category.Value}]\n" +
							$"**{catalyst.Description}**");
			embB.AddField(Global.InvisibleString, $"{catalyst.WeaponName}");

			//for (var i = 1; i <= answersShuffled.Count; i++)
			//{
			//	// Добавляет пустое поле чтобы ответы выглядели как будто они в сетке 2 на 2
			//	// вместо 3 ответов в одной строке добавляя пустое поле внизу ( Если представлено 4 ответа) - чисто косметическая фича, для приятного вида.
			//	if (embB.Fields.Count % 3 == 2) embB.AddField(Settings.InvisibleString, Settings.InvisibleString);
			//	// Получает правильную реакцию для итерации
			//	ReactOptions.TryGetValue(i.ToString(), out var reactWith);
			//	// Добавляет правильную реакцию для итерации и вопроса в поле EmbedBuilder-а
			//	embB.AddField(reactWith.Name, WebUtility.HtmlDecode(answersShuffled[i - 1]), true);
			//}
			return embB;
		}

		/// <summary>
		/// Возвращает список доступных категорий в рамках одной страницы
		/// </summary>
		/// <param name="page">Страница для доступа (Может быть только одна)</param>
		/// <param name="pagesize">Количество категорий на страницу</param>
		/// <returns>Список категорий размером с pagesize (или меньше если это последняя страница)</returns>
		internal static List<Catalyst_Category> CategoriesPaged(int page, int pagesize)
		{
			page--;
			var startIndex = page * pagesize;
			return Categories.GetRange(startIndex, Math.Min(pagesize, Categories.Count - (startIndex)));
		}
	}

	/// <summary>
	/// Возвожные состояния викторины
	/// </summary>
	internal enum MessageStates
	{
		StartPage, ChangingCategory, Browse
	}
}
