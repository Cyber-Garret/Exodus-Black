using Newtonsoft.Json;

namespace Neira.Db
{
	internal class Credential
	{
		[JsonProperty("trustedconnection")]
		internal bool TrustedConnection { get; set; }

		[JsonProperty("server")]
		internal string Server { get; set; }

		[JsonProperty("database")]
		internal string Database { get; set; }

		[JsonProperty("user")]
		internal string User { get; set; }

		[JsonProperty("password")]
		internal string Password { get; set; }
	}
}
