﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Neira.Web.Database
{
	public class Clan_Member
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public long DestinyMembershipType { get; set; }
		public string DestinyMembershipId { get; set; }
		public long? BungieMembershipType { get; set; }
		public string BungieMembershipId { get; set; }
		public string IconPath { get; set; }
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
		public DateTimeOffset? ClanJoinDate { get; set; }
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm dd.MM.yyyy}")]
		public DateTimeOffset? DateLastPlayed { get; set; }
		public long ClanId { get; set; }
		public Clan Clan { get; set; }
		public int ErrorCode { get; set; }
	}
}