using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace API.Bungie.Models.GroupV2.GetGroup
{
    public partial class GetGroupResult : RootResult
    {
        [JsonProperty("Response")]
        public Response Response { get; set; }
    }

    public partial class Response
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

    public partial class Detail
    {
        [JsonProperty("groupId")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long GroupId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("groupType")]
        public long GroupType { get; set; }

        [JsonProperty("membershipIdCreated")]
        [JsonConverter(typeof(ParseStringConverter))]
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
        [JsonConverter(typeof(ParseStringConverter))]
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
        [JsonConverter(typeof(ParseStringConverter))]
        public long LastOnlineStatusChange { get; set; }

        [JsonProperty("groupId")]
        [JsonConverter(typeof(ParseStringConverter))]
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
        [JsonConverter(typeof(ParseStringConverter))]
        public long? SupplementalDisplayName { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        [JsonProperty("membershipType")]
        public long MembershipType { get; set; }

        [JsonProperty("membershipId")]
        public string MembershipId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public partial class GetGroupResult
    {
        public static GetGroupResult FromJson(string json) => JsonConvert.DeserializeObject<GetGroupResult>(json, API.Bungie.Models.GroupV2.GetGroup.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GetGroupResult self) => JsonConvert.SerializeObject(self, API.Bungie.Models.GroupV2.GetGroup.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (long.TryParse(value, out long l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
