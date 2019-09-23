using System.ComponentModel.DataAnnotations;

namespace Neira.Db.Models
{
	public class MilestoneUser
	{
		[Key]
		public int Id { get; set; }
		public ulong MessageId { get; set; }
		public ActiveMilestone ActiveMilestone { get; set; }
		public ulong UserId { get; set; }
	}
}
