using System.ComponentModel.DataAnnotations;

namespace Neuromatrix.Resources.Database
{
    public class Gear
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } //Холодное сердце
        public string Type { get; set; } //Энергетическая лучевая винтовка
        public string IconUrl { get; set; } //http://159.69.21.188/Icon/Energy/Coldheart.jpg
        public string ImageUrl { get; set; } //http://159.69.21.188/Image/Energy/Coldheart.jpg
        public string Description { get; set; } //Новейшие технологии Omolon позволяют использовать жидкое топливо в качестве хладагента, обеспечивая оружейным системам минусовую температуру.
        public string PerkName { get; set; } //Холодный синтез
        public string PerkDescription { get; set; } //Это оружие оснащено лазером с термоядерной накачкой, испускающим непрерывный луч.
        public string SecondPerkName { get; set; } //Самая долгая зима
        public string SecondPerkDescription { get; set; } //Чем дольше лазер Холодного сердца наведен на цель, тем больший урон он наносит.
        public string DropLocation { get; set; } //Экзотические энграммы, редкие выпадения в открытом мире.
        public int Catalyst { get; set; } // 0 - Отсутствует. 1 - Есть.
        public string WhereCatalystDrop { get; set; } // Убить босса в героическом или Сумрачном налете.
        public string CatalystQuest { get; set; }// Убить 300 монстров в любой точке солнечной системы.
        public string CatalystBonus { get; set; } // Стабильность +20 и перезарядка +20.

    }
}
