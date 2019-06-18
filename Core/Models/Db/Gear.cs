using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Db
{
	public class Gear
	{
		[Key]
		public int Id { get; set; }

		/// <summary> Пример: Холодное сердце</summary>
		[Display(Name = "Название")]
		public string Name { get; set; }

		/// <summary> Пример: Энергетическая лучевая винтовка</summary>
		[Display(Name = "Тип")]
		public string Type { get; set; }

		/// <summary> Пример: http://159.69.21.188/Icon/Energy/Coldheart.jpg </summary>
		[Display(Name = "Иконка")]
		public string IconUrl { get; set; }

		/// <summary> Пример: http://159.69.21.188/Image/Energy/Coldheart.jpg </summary>
		[Display(Name = "Скриншот")]
		public string ImageUrl { get; set; }

		/// <summary> Пример: Новейшие технологии Omolon позволяют использовать жидкое топливо в качестве хладагента, обеспечивая оружейным системам минусовую температуру.</summary>
		[Display(Name = "Описание")]
		public string Description { get; set; }

		/// <summary> Пример: Холодный синтез</summary>
		[Display(Name = "Основной перк")]
		public string PerkName { get; set; }

		/// <summary> Пример: Это оружие оснащено лазером с термоядерной накачкой, испускающим непрерывный луч.</summary>
		[Display(Name = "Описание перка")]
		public string PerkDescription { get; set; }

		/// <summary> Пример: Самая долгая зима</summary>
		[Display(Name = "Вторичный перк")]
		public string SecondPerkName { get; set; }

		/// <summary> Пример: Чем дольше лазер Холодного сердца наведен на цель, тем больший урон он наносит.</summary>
		[Display(Name = "Описание перка")]
		public string SecondPerkDescription { get; set; }

		/// <summary> Пример: Экзотические энграммы, редкие выпадения в открытом мире.</summary>
		[Display(Name = "Как получить")]
		public string DropLocation { get; set; }

		[Display(Name = "Это оружие?")]
		public bool isWeapon { get; set; }

		/// <summary> Пример: false - Отсутствует. true - Есть.</summary>
		[Display(Name = "Катализатор")]
		public bool Catalyst { get; set; }

		/// <summary> Пример: Убить босса в героическом или Сумрачном налете.</summary>
		[Display(Name = "Как получить катализатор")]
		public string WhereCatalystDrop { get; set; }

		/// <summary> Пример: Убить 300 монстров в любой точке солнечной системы.</summary>
		[Display(Name = "Задание катализатора")]
		public string CatalystQuest { get; set; }

		/// <summary> Пример: Стабильность +20 и перезарядка +20.</summary>
		[Display(Name = "Бонус катализатора")]
		public string CatalystBonus { get; set; }

	}
}
