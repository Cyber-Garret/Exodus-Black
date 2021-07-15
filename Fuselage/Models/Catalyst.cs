using System.ComponentModel.DataAnnotations;

namespace Fuselage.Models
{
    public class Catalyst
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(5), Display(Name = "Locale")]
        public string Locale { get; set; }

        [MaxLength(100), Display(Name = "Weapon name")]
        public string Name { get; set; }

        /// <summary>
        /// URL of icon
        /// </summary>
        [Required, MaxLength(2048), Display(Name = "Icon URL")]
        public string Icon { get; set; }

        [MaxLength(200), Display(Name = "Weapon description")]
        public string Description { get; set; }

        [MaxLength(200), Display(Name = "How to obtain")]
        public string DropLocation { get; set; }

        [MaxLength(200), Display(Name = "Objectives")]
        public string Objectives { get; set; }

        [MaxLength(200), Display(Name = "Bonuses")]
        public string Masterwork { get; set; }
    }
}
