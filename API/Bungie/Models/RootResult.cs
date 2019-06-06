using Newtonsoft.Json;

namespace API.Bungie.Models
{
	public partial class RootResult
	{
		[JsonProperty("ErrorCode")]
		public long ErrorCode { get; set; }

		[JsonProperty("ThrottleSeconds")]
		public long ThrottleSeconds { get; set; }

		[JsonProperty("ErrorStatus")]
		public string ErrorStatus { get; set; }

		[JsonProperty("Message")]
		public string Message { get; set; }

		[JsonProperty("MessageData")]
		public MessageData MessageData { get; set; }
	}
	public partial class MessageData { }

}
