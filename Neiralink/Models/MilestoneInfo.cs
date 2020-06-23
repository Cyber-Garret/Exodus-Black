using Dapper.Contrib.Extensions;

namespace Neiralink.Models
{
	public class MilestoneInfo
	{
		[Key]
		public byte RowID { get; set; }
		public string Icon { get; set; }
		public byte MaxSpace { get; set; }
		public MilestoneType MilestoneType { get; set; }
		public GameName Game { get; set; }
	}

	public class MilestoneInfoLocale
	{
		public byte MilestoneInfoRowID { get; set; }
		public LangKey LangKey { get; set; }
		public string Name { get; set; }
		public string Alias { get; set; }
		public string Type { get; set; }
	}
}
