using System.ComponentModel.DataAnnotations;

namespace Core.Models.Db
{
	public class Gear
	{
		[Key]
		public int Id { get; set; }
		[Display(Name = "Название")]
		public string Name { get; set; } //Холодное сердце
		[Display(Name = "Тип")]
		public string Type { get; set; } //Энергетическая лучевая винтовка
		[Display(Name = "Иконка")]
		public string IconUrl { get; set; } //http://159.69.21.188/Icon/Energy/Coldheart.jpg
		[Display(Name = "Скриншот")]
		public string ImageUrl { get; set; } //http://159.69.21.188/Image/Energy/Coldheart.jpg
		[Display(Name = "Описание")]
		public string Description { get; set; } //Новейшие технологии Omolon позволяют использовать жидкое топливо в качестве хладагента, обеспечивая оружейным системам минусовую температуру.
		[Display(Name = "Основной перк")]
		public string PerkName { get; set; } //Холодный синтез
		[Display(Name = "Описание перка")]
		public string PerkDescription { get; set; } //Это оружие оснащено лазером с термоядерной накачкой, испускающим непрерывный луч.
		[Display(Name = "Вторичный перк")]
		public string SecondPerkName { get; set; } //Самая долгая зима
		[Display(Name = "Описание перка")]
		public string SecondPerkDescription { get; set; } //Чем дольше лазер Холодного сердца наведен на цель, тем больший урон он наносит.
		[Display(Name = "Как получить")]
		public string DropLocation { get; set; } //Экзотические энграммы, редкие выпадения в открытом мире.
		[Display(Name = "Катализатор")]
		public int Catalyst { get; set; } // 0 - Отсутствует. 1 - Есть.
		[Display(Name = "Как получить катализатор")]
		public string WhereCatalystDrop { get; set; } // Убить босса в героическом или Сумрачном налете.
		[Display(Name = "Задание катализатора")]
		public string CatalystQuest { get; set; }// Убить 300 монстров в любой точке солнечной системы.
		[Display(Name = "Бонус катализатора")]
		public string CatalystBonus { get; set; } // Стабильность +20 и перезарядка +20.

	}
}
