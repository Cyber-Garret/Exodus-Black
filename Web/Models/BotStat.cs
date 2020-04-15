namespace Web.Models
{
	public class BotStat
	{
		public int Id { get; set; } = 1;
		/// <summary>
		/// Bot discord servers count
		/// </summary>
		public int Servers { get; set; } = 0;
		/// <summary>
		/// Bot users count
		/// </summary>
		public int Users { get; set; } = 0;
		/// <summary>
		/// Bot registered milestones
		/// </summary>
		public int Milestones { get; set; } = 0;
	}
}
