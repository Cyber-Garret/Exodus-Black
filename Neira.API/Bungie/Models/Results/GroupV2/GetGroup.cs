using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Neira.API.Bungie.Models.Results.GroupV2.GetGroup
{
	public class GetGroup : RootResult
	{
		[JsonProperty("Response")]
		public Response Response { get; set; }
	}
	public class Response
	{
		[JsonProperty("detail")]
		public Detail Detail { get; set; }

		[JsonProperty("founder")]
		public Founder Founder { get; set; }

		[JsonProperty("alliedIds")]
		public object[] AlliedIds { get; set; }

		[JsonProperty("allianceStatus")]
		public long AllianceStatus { get; set; }

		[JsonProperty("groupJoinInviteCount")]
		public long GroupJoinInviteCount { get; set; }

		[JsonProperty("currentUserMemberMap")]
		public MessageData CurrentUserMemberMap { get; set; }

		[JsonProperty("currentUserPotentialMemberMap")]
		public MessageData CurrentUserPotentialMemberMap { get; set; }
	}

	public class Detail
	{
		[JsonProperty("groupId")]
		public long GroupId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("groupType")]
		public long GroupType { get; set; }

		[JsonProperty("membershipIdCreated")]
		public long MembershipIdCreated { get; set; }

		[JsonProperty("creationDate")]
		public DateTimeOffset CreationDate { get; set; }

		[JsonProperty("modificationDate")]
		public DateTimeOffset ModificationDate { get; set; }

		[JsonProperty("about")]
		public string About { get; set; }

		[JsonProperty("tags")]
		public object[] Tags { get; set; }

		[JsonProperty("memberCount")]
		public long MemberCount { get; set; }

		[JsonProperty("isPublic")]
		public bool IsPublic { get; set; }

		[JsonProperty("isPublicTopicAdminOnly")]
		public bool IsPublicTopicAdminOnly { get; set; }

		[JsonProperty("motto")]
		public string Motto { get; set; }

		[JsonProperty("allowChat")]
		public bool AllowChat { get; set; }

		[JsonProperty("isDefaultPostPublic")]
		public bool IsDefaultPostPublic { get; set; }

		[JsonProperty("chatSecurity")]
		public long ChatSecurity { get; set; }

		[JsonProperty("locale")]
		public string Locale { get; set; }

		[JsonProperty("avatarImageIndex")]
		public long AvatarImageIndex { get; set; }

		[JsonProperty("homepage")]
		public long Homepage { get; set; }

		[JsonProperty("membershipOption")]
		public long MembershipOption { get; set; }

		[JsonProperty("defaultPublicity")]
		public long DefaultPublicity { get; set; }

		[JsonProperty("theme")]
		public string Theme { get; set; }

		[JsonProperty("bannerPath")]
		public string BannerPath { get; set; }

		[JsonProperty("avatarPath")]
		public string AvatarPath { get; set; }

		[JsonProperty("conversationId")]
		public long ConversationId { get; set; }

		[JsonProperty("enableInvitationMessagingForAdmins")]
		public bool EnableInvitationMessagingForAdmins { get; set; }

		[JsonProperty("banExpireDate")]
		public DateTimeOffset BanExpireDate { get; set; }

		[JsonProperty("features")]
		public Features Features { get; set; }

		[JsonProperty("clanInfo")]
		public ClanInfo ClanInfo { get; set; }
	}

	public partial class ClanInfo
	{
		[JsonProperty("d2ClanProgressions")]
		public Dictionary<string, Dictionary<string, long>> D2ClanProgressions { get; set; }

		[JsonProperty("clanCallsign")]
		public string ClanCallsign { get; set; }

		[JsonProperty("clanBannerData")]
		public ClanBannerData ClanBannerData { get; set; }
	}

	public partial class ClanBannerData
	{
		[JsonProperty("decalId")]
		public long DecalId { get; set; }

		[JsonProperty("decalColorId")]
		public long DecalColorId { get; set; }

		[JsonProperty("decalBackgroundColorId")]
		public long DecalBackgroundColorId { get; set; }

		[JsonProperty("gonfalonId")]
		public long GonfalonId { get; set; }

		[JsonProperty("gonfalonColorId")]
		public long GonfalonColorId { get; set; }

		[JsonProperty("gonfalonDetailId")]
		public long GonfalonDetailId { get; set; }

		[JsonProperty("gonfalonDetailColorId")]
		public long GonfalonDetailColorId { get; set; }
	}

	public partial class Features
	{
		[JsonProperty("maximumMembers")]
		public long MaximumMembers { get; set; }

		[JsonProperty("maximumMembershipsOfGroupType")]
		public long MaximumMembershipsOfGroupType { get; set; }

		[JsonProperty("capabilities")]
		public long Capabilities { get; set; }

		[JsonProperty("membershipTypes")]
		public long[] MembershipTypes { get; set; }

		[JsonProperty("invitePermissionOverride")]
		public bool InvitePermissionOverride { get; set; }

		[JsonProperty("updateCulturePermissionOverride")]
		public bool UpdateCulturePermissionOverride { get; set; }

		[JsonProperty("hostGuidedGamePermissionOverride")]
		public long HostGuidedGamePermissionOverride { get; set; }

		[JsonProperty("updateBannerPermissionOverride")]
		public bool UpdateBannerPermissionOverride { get; set; }

		[JsonProperty("joinLevel")]
		public long JoinLevel { get; set; }
	}

	public partial class Founder
	{
		[JsonProperty("memberType")]
		public long MemberType { get; set; }

		[JsonProperty("isOnline")]
		public bool IsOnline { get; set; }

		[JsonProperty("lastOnlineStatusChange")]
		public long LastOnlineStatusChange { get; set; }

		[JsonProperty("groupId")]
		public long GroupId { get; set; }

		[JsonProperty("destinyUserInfo")]
		public UserInfo DestinyUserInfo { get; set; }

		[JsonProperty("bungieNetUserInfo")]
		public UserInfo BungieNetUserInfo { get; set; }

		[JsonProperty("joinDate")]
		public DateTimeOffset JoinDate { get; set; }
	}

	public partial class UserInfo
	{
		[JsonProperty("supplementalDisplayName", NullValueHandling = NullValueHandling.Ignore)]
		public string SupplementalDisplayName { get; set; }

		[JsonProperty("iconPath")]
		public string IconPath { get; set; }

		[JsonProperty("membershipType")]
		public long MembershipType { get; set; }

		[JsonProperty("membershipId")]
		public string MembershipId { get; set; }

		[JsonProperty("displayName")]
		public string DisplayName { get; set; }
	}
}