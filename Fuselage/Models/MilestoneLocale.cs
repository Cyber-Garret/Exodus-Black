using System.ComponentModel.DataAnnotations;

namespace Fuselage.Models
{
    public class MilestoneLocale
    {
        [Key]
        public int Id { get; set; }

        public int MilestoneId { get; set; }
        public Milestone Milestone { get; set; }

        [MaxLength(5), Display(Name = "Locale")]
        public string Locale { get; set; }

        [MaxLength(100), Display(Name = "Milestone name")]
        public string Name { get; set; }

        [Display(Name = "Simple alias for search")]
        public string Alias { get; set; }

        [MaxLength(100), Display(Name = "Display type")]
        public string Type { get; set; }
    }
}
