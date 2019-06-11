using Core.Models.Db;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Features.Catalyst
{
	internal class CatalystCore
	{

		// Используется для идентификации реакций игрока который вызвал команду.
		internal ulong PlayerId;
		// Используется для идентификации реакций сообщения к этой игре.
		internal ulong GameMessageId;

		internal Catalyst_Category Category;

		//private readonly List<Core.Models.Db.Catalyst> previousCatalysts;
		private Core.Models.Db.Catalyst _currentCatalyst;
		private MessageStates _messagestate;
		private EmbedBuilder _emb;
		private int _currentCategoryPage;

		private const int CategoriesPerPage = 4;

		internal CatalystCore(ulong gameMessageId, ulong playerId, string category = "любая")
		{
			PlayerId = playerId;
			GameMessageId = gameMessageId;
			//previousCatalysts = new List<Core.Models.Db.Catalyst>();
			_currentCategoryPage = 1;
			_messagestate = MessageStates.StartPage;
			_emb = CatalystData.CatalystStartingEmbed(this);
		}

		// Основная функция, которая срабатывает когда игрок добавляет реакции к сообщению
		internal async Task HandleReaction(IUserMessage socketMsg, IReaction reaction)
		{
			switch (_messagestate)
			{
				case (MessageStates.StartPage):
					await HandleStartPageInput(socketMsg, reaction);
					break;
				case (MessageStates.ChangingCategory):
					HandleSelectCategoryInput(socketMsg, reaction);
					break;
				case (MessageStates.Browse):
					await HandleBrowsingInput(socketMsg, reaction);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// Обновляет сообщение и всю в нем информацию основываясь на GameStates
			await UpdateMessage(socketMsg);
		}

		/// <summary>
		/// Обновляет сообщение с викториной перестраивая embed
		/// </summary>
		private async Task UpdateMessage(IUserMessage socketMsg)
		{
			await socketMsg.ModifyAsync(message =>
			{
				message.Embed = _emb.Build();
			});
		}

		/// <summary>
		/// Выводит всю необходимую информацию о викторине для выхода в "главное меню" включая возможную статистику.
		/// </summary>
		private void PrepareStartMenu()
		{
			// Получает стартовыое сообщение embed в котором находятся все настройки викторины.
			_emb = CatalystData.CatalystStartingEmbed(this);
			_messagestate = MessageStates.StartPage;
		}

		/// <summary>
		/// Управляет поведением реакций в GameStates.ChangingCategory
		/// </summary>
		/// <param name="socketMsg"></param>
		/// <param name="reaction"></param>
		private void HandleSelectCategoryInput(IUserMessage socketMsg, IReaction reaction)
		{
			if (reaction.Emote.Equals(CatalystData.ReactOptions["ok"]))
			{
				_messagestate = MessageStates.StartPage;
				PrepareStartMenu();
				return;
			}
			if (reaction.Emote.Equals(CatalystData.ReactOptions["left"]))
			{
				_currentCategoryPage = Math.Max(_currentCategoryPage - 1, 1);
				PrepareCategoryEmb();
				return;
			}
			if (reaction.Emote.Equals(CatalystData.ReactOptions["right"]))
			{
				_currentCategoryPage = Math.Min(_currentCategoryPage + 1, 1 + CatalystData.Categories.Count / CategoriesPerPage);
				PrepareCategoryEmb();
				return;
			}
			Category = PickCategory(socketMsg.Embeds.FirstOrDefault(), reaction);
			_messagestate = MessageStates.StartPage;
			PrepareStartMenu();
		}

		/// <summary>
		/// Получает специально форматированный embed и возвращает категорию выбранную при нажатии на реакцию
		/// </summary>
		/// <param name="emb">Embed с категориями в виде отдельных полей и реакциями для их выбора</param>
		/// <param name="reaction">Реакция которую нажал пользователь при выборе категории</param>
		private Catalyst_Category PickCategory(IEmbed emb, IReaction reaction)
		{
			// Ищет какое embed поле соответствует представленой реакции.
			var categoryString = emb.Fields
				.Select(fi => fi.Name)
				.Where(field => field.StartsWith(reaction.Emote.Name));
			var enumerable = categoryString as string[] ?? categoryString.ToArray();
			return !enumerable.Any()
				// Если не была выбрана какая либо из предложеных категорий тогда выбираем "Любая"
				? new Catalyst_Category { Id = 0, Value = "Любая" }
				// Так же обязательно удаляем реакцию и пробелы с обоих сторон из значения, чтобы получить правильную категорию
				: CatalystData.Categories.FirstOrDefault(cat =>
						cat.Value == enumerable.ToArray()[0].Replace(reaction.Emote.Name, ""
					).Trim());
		}

		/// <summary>
		/// Управляет поведением реакций в GameStates.StartPage
		/// </summary>
		private async Task HandleStartPageInput(IUserMessage socketMsg, IReaction reaction)
		{
			var reactionName = reaction.Emote.Name;
			// Если пользователь хочет сменить категорию
			if (reactionName == CatalystData.ReactOptions["1"].Name)
			{
				await socketMsg.AddReactionAsync(CatalystData.ReactOptions["left"]);
				await socketMsg.AddReactionAsync(CatalystData.ReactOptions["right"]);
				PrepareCategoryEmb();
				_messagestate = MessageStates.ChangingCategory;
				return;
			}
			// Если пользователь хочет начать викторину
			if (reactionName == CatalystData.ReactOptions["ok"].Name)
				await PreparePlayEmb(socketMsg, reaction);
		}

		/// <summary>
		/// Переписывает embed викторины с категориями.
		/// </summary>
		private void PrepareCategoryEmb()
		{
			_emb.Fields.Clear();
			_emb.WithDescription(
				"Выбери категорию с вопросами соответствующей реакцией.\n" +
				$"Ты можешь листать страницы при помощи {CatalystData.ReactOptions["left"]} и {CatalystData.ReactOptions["right"]} " +
				$"или нажать {CatalystData.ReactOptions["ok"]} что-бы вернуться назад без смены категории.");
			var categories = CatalystData.CategoriesPaged(_currentCategoryPage, CategoriesPerPage);
			for (var i = 1; i <= categories.Count; i++)
			{
				_emb.AddField(CatalystData.ReactOptions[i.ToString()].Name + "  " + categories[i - 1].Value, Global.InvisibleString);
			}
		}

		/// <summary>
		/// Управляет поведением реакций в GameStates.Playing
		/// </summary>
		private async Task HandleBrowsingInput(IUserMessage socketMsg, IReaction reaction)
		{
			if (reaction.Emote.Equals(CatalystData.ReactOptions["ok"]) && _messagestate == MessageStates.Browse)
			{
				PrepareStartMenu();
				_messagestate = MessageStates.StartPage;
				return;
			}

			await PreparePlayEmb(socketMsg, reaction);
		}

		/// <summary>
		/// Переписывает embed викторины для отображения вопроса + показывает результат предыдущего вопроса.
		/// </summary>
		private async Task PreparePlayEmb(IUserMessage socketMsg, IReaction reaction)
		{
			await NewCatalyst();
			// Меняем режим викторины в случае когда мы впервые пришли из главного меню.
			_messagestate = MessageStates.Browse;
			_emb = CatalystData.CatalystToEmbed(_currentCatalyst, _emb);
			_emb.AddField(Global.InvisibleString, $"Нажми {CatalystData.ReactOptions["ok"]} для возвращения в главное меню.");
		}

		/// <summary>
		/// Запоминает текущий вопрос (для статистики) и выполняет запрос на получение нового вопроса на основе настроек викторины.
		/// </summary>
		private async Task NewCatalyst()
		{
			if (_messagestate == MessageStates.Browse)
				previousCatalysts.Add(_currentCatalyst);
			_currentCatalyst =
			   (await CatalystData.GetCatalysts(
					categoryId: Category.Id))
				.FirstOrDefault();
		}
	}
}
