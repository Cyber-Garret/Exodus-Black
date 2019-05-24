using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Db
{
	public class Destiny2Clan_Member
	{
		public int Id { get; set; }
		[Display(Name = "Имя")]
		public string Name { get; set; }
		public long DestinyMembershipType { get; set; }
		public string DestinyMembershipId { get; set; }
		public long? BungieMembershipType { get; set; }
		public string BungieMembershipId { get; set; }
		[Display(Name = "Аватар")]
		public string IconPath { get; set; }
		[Display(Name = "Дата вступления")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
		public DateTimeOffset? ClanJoinDate { get; set; }
		[Display(Name = "Последний раз в сети")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm dd.MM.yyyy}")]
		public DateTimeOffset? DateLastPlayed { get; set; }

		public long Destiny2ClanId { get; set; }
		public Destiny2Clan Destiny2Clan { get; set; }
	}
}

