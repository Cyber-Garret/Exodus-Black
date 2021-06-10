using Failsafe.Models.Enums;

#nullable enable
namespace Failsafe.Models
{
    public class MilestoneInfo
    {
        public string Name { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public string? Type { get; set; }
        public string? Icon { get; set; }
        public byte MaxSpace { get; set; }
        public MilestoneType MilestoneType { get; set; }
        public GameName Game { get; set; }
    }
}
