using System;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace API.Bungie.Models.GroupV2
{
    public partial class GroupV2Result : RootResult
    {
        [JsonProperty("Response")]
        public Response Response { get; set; }
    }

    public partial class Response
    {
        [JsonProperty("results")]
        public Result[] Results { get; set; }

        [JsonProperty("totalResults")]
        public long TotalResults { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("query")]
        public Query Query { get; set; }

        [JsonProperty("useTotalResults")]
        public bool UseTotalResults { get; set; }
    }

    public partial class Query
    {
        [JsonProperty("itemsPerPage")]
        public long ItemsPerPage { get; set; }

        [JsonProperty("currentPage")]
        public long CurrentPage { get; set; }
    }

    public partial class Result
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

        [JsonProperty("joinDate")]
        public DateTimeOffset JoinDate { get; set; }

        [JsonProperty("bungieNetUserInfo", NullValueHandling = NullValueHandling.Ignore)]
        public UserInfo BungieNetUserInfo { get; set; }
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

    public partial class GroupV2Result
    {
        public static GroupV2Result FromJson(string json) => JsonConvert.DeserializeObject<GroupV2Result>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GroupV2Result self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
            long l;
            if (long.TryParse(value, out l))
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
