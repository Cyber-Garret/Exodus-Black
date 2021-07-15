using Neiralink.Enums;

using System.ComponentModel.DataAnnotations;

using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;

namespace Neiralink.Models
{
    public class MilestoneInfo
    {
        [Key]
        public byte RowID { get; set; }
        [Display(Name = "Icon url")]
        public string Icon { get; set; }
        [Display(Name = "Maximum users in battle group"), Range(2, 20, ErrorMessage = "Error: space range must between 2 and 20.")]
        public byte MaxSpace { get; set; }
        [Display(Name = "Milestone type")]
        public MilestoneType MilestoneType { get; set; }
        [Display(Name = "Game name")]
        public GameName Game { get; set; }
    }

    public class MilestoneInfoLocale
    {
        public byte MilestoneInfoRowID { get; set; }
        [Required]
        public LangKey LangKey { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Type { get; set; }
    }
}
